using System;

namespace destruct.CallbackSystem.Signals
{
    public class TypedSignalSubscriber<TSubscription> : SignalSubscriber
        where TSubscription : Signal
    {
        private readonly Action<TSubscription> _subscription;

        public TypedSignalSubscriber(Action<TSubscription> subscription)
        {
            _subscription = subscription;
            SubscriptionType = typeof(TSubscription);
        }

        public override void InvokeSubscription(Signal nonTypedEvent)
        {
            _subscription?.Invoke(nonTypedEvent as TSubscription);
        }
    }
}
