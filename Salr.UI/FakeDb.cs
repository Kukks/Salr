using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin.Secp256k1;
using NNostr.Client;
using Relay;

namespace Salr.UI;

public class Db
{
    public class UserMetadata
    {
    }
    public ECPrivKey Key { get; set; }
    public ECXOnlyPubKey PubKey => Key.CreateXOnlyPubKey();
    public string PubKeyHex => PubKey.ToBytes().ToHex();
    
    public HashSet<string> NIP4Authors { get; set; } = new();
    public Dictionary<string,string> DecryptedNIP4Content { get; set; } = new();


    public Dictionary<string,NostrEvent> Events { get; set; }= new();
    public MultiValueDictionary<string, string> AuthorToEvent { get; set; }= new();
    public Dictionary<string, string> ReplyToEvent { get; set; }= new();
    public Dictionary<string, string> ReferencedUserToEvent { get; set; }= new();

    public Dictionary<Uri,ActiveRelay> ActiveRelays { get; set; } = new();
    public HashSet<Uri> KnownRelays { get; set; }= new()
    {
        new Uri("wss://localhost:5001")
    };

    public class ActiveRelay
    {
        public RelayStatus Status { get; set; }
        public NostrRelayListener Listener { get; set; }
    }
    
    public enum RelayStatus
    {
        Connecting,
        Connected,
        Disconnected
    }

    public void ProcessEvents(NostrEvent[] eEvents)
    {
        foreach (var nostrEvent in eEvents)
        {
            ProcessEvent(nostrEvent);
        }
    }

    void ProcessEvent(NostrEvent e)
    {
        if (!Events.TryAdd(e.Id, e)) return;
        AuthorToEvent.Add(e.PublicKey, e.Id);
        var taggedEvent = e.Tags.FirstOrDefault(tag => tag.TagIdentifier == "e" && tag.Data.Any())?.Data?.First();
        if (taggedEvent is not null)
        {
            ReplyToEvent.TryAdd(taggedEvent, e.Id);
        }
        
        var taggedPubKey = e.Tags.FirstOrDefault(tag => tag.TagIdentifier == "p" && tag.Data.Any())?.Data?.First();
        if (e.Kind == 4  )
        {
            if (e.PublicKey == PubKeyHex )
            {
                NIP4Authors.Add(e.PublicKey);
                DecryptedNIP4Content.TryAdd(e.Id, e.DecryptNip04Event(Key));
            }else if (taggedPubKey == PubKeyHex)
            {
                NIP4Authors.Add(taggedPubKey);
                DecryptedNIP4Content.TryAdd(e.Id, e.DecryptNip04Event(Key));
            }
        }
        if(taggedPubKey is not null)
            ReferencedUserToEvent.TryAdd(taggedPubKey,e.Id);
    }

    public IEnumerable<string> GetDirectMessageThread(string pubkey)
    {
       return  Events.Where(pair =>
            //evt is a dm
            pair.Value.Kind == 4 &&
            //evt is to or from pubkey
            (
                pair.Value.PublicKey == pubkey &&
                pair.Value.Tags.Any(tag => tag.TagIdentifier == "p" && tag.Data.First() == PubKeyHex))
            ||
            pair.Value.PublicKey == PubKeyHex &&
            pair.Value.Tags.Any(tag => tag.TagIdentifier == "p" && tag.Data.First() == pubkey)
        ).OrderBy(pair => pair.Value.CreatedAt)
           .Select(pair => pair.Key);
    }
    
    
}