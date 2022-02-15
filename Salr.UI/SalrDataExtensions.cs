using NNostr.Client;

namespace Salr.UI;

public static class SalrDataExtensions
{
    public static NostrSubscriptionFilter[] GetDirectMessageThreadFilters(string pubkey, string pubkey2)
    {
        return new[]
        {
            new NostrSubscriptionFilter()
            {
                Authors = new[] { pubkey },
                PublicKey = new[] { pubkey2 },
                Kinds = new[] { 4 },
            },
            new NostrSubscriptionFilter()
            {
                Authors = new[] { pubkey2 },
                PublicKey = new[] { pubkey },
                Kinds = new[] { 4 },
            }
        };
    }
    public static NostrSubscriptionFilter[] GetThreadFilters(string id)
    {
        return new[]
        {
            new NostrSubscriptionFilter()
            {
                EventId = new[] { id },
                Kinds = new[] { 1 },
            },
            new NostrSubscriptionFilter()
            {
                Ids = new[] { id },
            }
        };
    }
}