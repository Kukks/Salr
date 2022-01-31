using System;
using System.Threading;
using System.Threading.Tasks;
using NNostr.Client;
using NNostr.UI;
using Salr.UI;


public class NostrRelayListener : IHostedService
{
    private readonly NostrClient _nostrClient;
    public Db.RelayStatus Status { get; set; } = Db.RelayStatus.Disconnected;
    private readonly Uri _uri;
    private readonly Db _db;

    public NostrRelayListener(Uri uri, Db db)
    {
        _nostrClient = new NostrClient(uri);
        _uri = uri;
        _db = db;
    }

    public void Dispose()
    {
        _nostrClient.Dispose();
    }

    public async Task StartAsync(CancellationToken token)
    {
        _nostrClient.NoticeReceived += NoticeReceived;
        _nostrClient.EventsReceived += EventsReceived;
        _ = Task.Factory.StartNew(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                Status = Db.RelayStatus.Connecting;
                await _nostrClient.ConnectAndWaitUntilConnected(token);
                Status = Db.RelayStatus.Connected;
                await _nostrClient.ListenForMessages();
                Status = Db.RelayStatus.Disconnected;
                await Task.Delay(2000, token);
            }
            
        }, token);

    }

    private void EventsReceived(object? sender, (string subscriptionId, NostrEvent[] events) e)
    {
        _db.ProcessEvents(e.events);
    }

    private void NoticeReceived(object? sender, string e)
    {
            
    }
        

    public async Task StopAsync(CancellationToken token)
    {
        await _nostrClient.Disconnect();
        
    }
        
}