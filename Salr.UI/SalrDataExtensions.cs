using NNostr.Client;

namespace Salr.UI;

public static class SalrDataExtensions
{
    public static NostrSubscriptionFilter[] GetDirectMessageThreadFilters(string pubkey)
    {
        return new[]
        {
            new NostrSubscriptionFilter()
            {
                Authors = new[] { pubkey },
                Kinds = new[] { 4 },
            },
            new NostrSubscriptionFilter()
            {
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