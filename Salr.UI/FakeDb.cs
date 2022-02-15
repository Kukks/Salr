using Microsoft.Extensions.Logging;
using NBitcoin.Secp256k1;
using NNostr.Client;
using Relay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NNostr.UI;

namespace Salr.UI;

public class Db:ISimilarHostedService
{
    private readonly ILoggerFactory _loggerFactory;
    public ECPrivKey? Key { get; set; }
    public ECXOnlyPubKey? PubKey => Key?.CreateXOnlyPubKey();
    public string PubKeyHex => PubKey?.ToBytes()?.ToHex();

    public HashSet<string> NIP4Authors { get; set; } = new();
    public Dictionary<string, string> DecryptedNIP4Content { get; set; } = new();


    public Dictionary<string, NostrEvent> Events { get; set; } = new();
    public Dictionary<string, NostrEvent> UnseenEvents { get; set; } = new();
    public MultiValueDictionary<string, string> AuthorToEvent { get; set; } = new();
    public MultiValueDictionary<Uri, string> RelayNotices { get; set; } = new();
    public MultiValueDictionary<string, string> ReplyToEvent { get; set; } = new();
    public Dictionary<string, string> ReferencedUserToEvent { get; set; } = new();

    private readonly Channel<NostrEvent> PendingIncomingEvents = Channel.CreateUnbounded<NostrEvent>();
    public Dictionary<Uri, NostrRelayListener> ActiveRelays { get; set; } = new();

    public HashSet<Uri> KnownRelays { get; set; } = new()
    {
        new Uri("wss://nostr-relay.freeberty.net"),
        new Uri("wss://localhost:5001")
    };

    public enum RelayStatus
    {
        Connecting,
        Connected,
        Disconnected
    }

    public Db(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;

    }


    async Task<bool> ProcessEvent(NostrEvent e, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"Processing event {e.Id}");
            if (!Events.TryAdd(e.Id, e)) return true;
            if (UnseenEvents.Remove(e.Id))
            {
                EventNoLongerPending?.Invoke(this, e.Id);
            }

            AuthorToEvent.Add(e.PublicKey, e.Id);
            var taggedEvent = e.Tags.FirstOrDefault(tag => tag.TagIdentifier == "e" && tag.Data.Any())?.Data?.First();
            if (taggedEvent is not null)
            {
                ReplyToEvent.Add(taggedEvent, e.Id);
            }

            var taggedPubKey = e.Tags.FirstOrDefault(tag => tag.TagIdentifier == "p" && tag.Data.Any())?.Data?.First();
            if (e.Kind == 4)
            {
                if (e.PublicKey == PubKeyHex)
                {
                    NIP4Authors.Add(e.PublicKey);
                    DecryptedNIP4Content.TryAdd(e.Id, await e.DecryptNip04Event(Key));
                }
                else if (taggedPubKey == PubKeyHex)
                {
                    NIP4Authors.Add(taggedPubKey);
                    DecryptedNIP4Content.TryAdd(e.Id, await e.DecryptNip04Event(Key));
                }
            }

            if (taggedPubKey is not null)
                ReferencedUserToEvent.TryAdd(taggedPubKey, e.Id);

            EventProcessed.Invoke(this, e);
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return false;
        }
    }


    public async Task Unsubscribe(string directDm)
    {
        Subscriptions.Remove(directDm);

        foreach (var activeRelay in ActiveRelays.Values)
        {
            await activeRelay.Unsubscribe(directDm);
        }
    }

    public async Task Subscribe(string directDm, NostrSubscriptionFilter[] getMessageThreadFilters)
    {
        await Unsubscribe(directDm);
        Subscriptions.TryAdd(directDm, getMessageThreadFilters);
        foreach (var activeRelay in ActiveRelays.Values)
        {
            await activeRelay.Subscribe(directDm, getMessageThreadFilters);
        }
    }

    public EventHandler<(string subscriptionId, NostrEvent[] events, Uri known)> EventsReceived;
    public EventHandler<(string tuple, Uri known)> NoticeReceived;
    public EventHandler<string> EventNoLongerPending;
    public EventHandler<NostrEvent> EventProcessed;
    public EventHandler RelayStateChanged;

    public Dictionary<string, NostrSubscriptionFilter[]> Subscriptions { get; set; } = new();
    public Dictionary<string, string> Threads { get; set; } = new();

    public async Task ToggleRelay(Uri known)
    {
        void StatusChangedCore(object? sender, EventArgs e)
        {
            RelayStateChanged.Invoke(sender, e);
        }

        void NoticeReceivedCore(object? sender, string e)
        {
            RelayNotices.Add(known, e);
            NoticeReceived.Invoke(sender, (e, known));
        }

        void EventsReceivedCore(object? sender, (string subscriptionId, NostrEvent[] events) e)
        {
            foreach (var nostrEvent in e.events)
            {
                PendingIncomingEvents.Writer.TryWrite(nostrEvent);
            }

            EventsReceived.Invoke(sender, (e.subscriptionId, e.events, known));
        }

        if (ActiveRelays.TryGetValue(known, out var activeRelay))
        {
            await activeRelay.StopAsync(CancellationToken.None);
            activeRelay.Dispose();
            if (ActiveRelays.Remove(known))
            {
                activeRelay.EventsReceived -= EventsReceivedCore;
                activeRelay.NoticeReceived -= NoticeReceivedCore;
                activeRelay.StatusChanged -= StatusChangedCore;
            }
        }
        else
        {
            KnownRelays.Add(known);
            var relay = new NostrRelayListener(known, _loggerFactory.CreateLogger<NostrRelayListener>());
            relay.EventsReceived += EventsReceivedCore;
            relay.NoticeReceived += NoticeReceivedCore;
            relay.StatusChanged += StatusChangedCore;

            if (ActiveRelays.TryAdd(known, relay))
            {
                relay.Subscriptions = Subscriptions;
                await relay.StartAsync(CancellationToken.None);
            }
        }
    }


    private async Task ProcessChannel<T>(Channel<T> channel, Func<T, CancellationToken, Task<bool>> processor,
        CancellationToken cancellationToken = default)
    {
        while (await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            if (channel.Reader.TryPeek(out var evt))
            {
                var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linked.CancelAfter(5000);
                if (await processor(evt, linked.Token))
                {
                    channel.Reader.TryRead(out _);
                }
            }
        }
    }

    public async Task SendEvent(NostrEvent evt)
    {
        UnseenEvents.Add(evt.Id, evt);
        foreach (var keyValuePair in ActiveRelays)
        {
            await keyValuePair.Value.SendEvent(evt);
        }
    }


    public string GetContent(string eventId, out NostrEvent nostrEvent)
    {
        if (Events.TryGetValue(eventId, out nostrEvent) || UnseenEvents.TryGetValue(eventId, out nostrEvent))
        {
            return GetContent(nostrEvent);
        }

        return "Event unavailable";
    }

    public string GetContent(NostrEvent nostrEvent)
    {
        if (nostrEvent.Kind == 4 && DecryptedNIP4Content.TryGetValue(nostrEvent.Id, out var decrypted))
        {
            return decrypted;
        }
        else if (nostrEvent.Kind == 4)
        {
            return "Encrypted data";
        }
        else
        {
            return nostrEvent.Content;
        }
    }

    public NostrEvent? GetEvent(string id, out bool pending)
    {
        pending = false;
        if (!Events.TryGetValue(id, out var nostrEvent))
        {
            if (UnseenEvents.TryGetValue(id, out nostrEvent))
            {
                pending = true;
            }
        }

        return nostrEvent;
    }

    public Task StartAsync(CancellationToken token)
    {
        _ = ProcessChannel(PendingIncomingEvents, ProcessEvent, token);

        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}