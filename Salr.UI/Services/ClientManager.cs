using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NNostr.Client;
using NNostr.UI;
using Salr.UI;


public class NostrRelayListener : IHostedService
{
    private NostrClient _nostrClient;

    public Db.RelayStatus Status
    {
        get => _status;
        set
        {
            _logger.LogInformation($" relay {_uri} status: {value} ");
            if (_status == value) return;
            _status = value;
            StatusChanged.Invoke(this, EventArgs.Empty);
        }
    }

    private readonly Uri _uri;
    private readonly ILogger<NostrRelayListener> _logger;
    private CancellationToken _ct;
    public EventHandler<(string subscriptionId, NostrEvent[] events)> EventsReceived;
    public EventHandler<string> NoticeReceived;
    public EventHandler StatusChanged;
    private Db.RelayStatus _status = Db.RelayStatus.Disconnected;

    public NostrRelayListener(Uri uri, ILogger<NostrRelayListener> logger)
    {
        _uri = uri;
        _logger = logger;
    }

    public void Dispose()
    {
        _nostrClient?.Dispose();
    }

    public async Task StartAsync(CancellationToken token)
    {
        _logger.LogInformation($"Starting to listen on relay {_uri}");
        _ct = token;

        _nostrClient = new NostrClient(_uri);
        _nostrClient.NoticeReceived += NoticeReceived;
        _nostrClient.EventsReceived += EventsReceived;
        _nostrClient.MessageReceived += (sender, s) => { _logger.LogInformation($"Relay {_uri} sent message: {s}"); };
        _ = Task.Factory.StartNew(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                Status = Db.RelayStatus.Connecting;
                await _nostrClient.ConnectAndWaitUntilConnected(token);
                Status = Db.RelayStatus.Connected;
                foreach (var subscription in Subscriptions)
                {
                    _logger.LogInformation($" relay {_uri} subscribing {subscription.Key}");
                    await _nostrClient.CreateSubscription(subscription.Key, subscription.Value, token);
                }

                await _nostrClient.ListenForMessages();
                Status = Db.RelayStatus.Disconnected;
                await Task.Delay(2000, token);
            }
        }, token);
    }


    public async Task StopAsync(CancellationToken token)
    {
        await _nostrClient.Disconnect();
    }

    public async Task Unsubscribe(string directDm)
    {
        if (Subscriptions.Remove(directDm))
        {
            try
            {
                await _nostrClient.CloseSubscription(directDm, _ct);
            }
            catch (Exception e)
            {
            }
        }
    }

    public async Task Subscribe(string directDm, NostrSubscriptionFilter[] getMessageThreadFilters)
    {
        await Unsubscribe(directDm);
        if (Subscriptions.TryAdd(directDm, getMessageThreadFilters) && Status == Db.RelayStatus.Connected)
        {
            try
            {
                await _nostrClient.CreateSubscription(directDm, getMessageThreadFilters, _ct);
            }
            catch (Exception e)
            {
            }
        }
    }

    public Dictionary<string, NostrSubscriptionFilter[]> Subscriptions { get; set; } = new();

    public async Task SendEvent(NostrEvent evt)
    {
        await _nostrClient.PublishEvent(evt, _ct);
    }
}