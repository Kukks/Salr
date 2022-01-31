using System;

namespace Salr.Abstractions.Contracts
{
    public interface ICounterState
    {
        int CurrentCount { get; }
        void IncrementCount();
        event Action StateChanged;
    }
}