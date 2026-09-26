using System;
using System.Collections.Generic;

namespace destruct.CallbackSystem.Signals
{
    public static class SignalManager
    {
        private static readonly Dictionary<Type, List<KeyValuePair<SignalSubscription, SignalSubscriber>>> 
            Subscriptions = new();

        public static void PushSignal<TSignal>(TSignal signal)
            where TSignal : Signal
        {
            if (!Subscriptions.ContainsKey(typeof(TSignal)))
                return;

            foreach (var subscriptionPair in Subscriptions[typeof(TSignal)])
            {
                subscriptionPair.Value.InvokeSubscription(signal);
            }
        }

        public static SignalSubscription SubscribeOnSignal<TSignal>(Action<TSignal> action, bool clearOnLoad = true)
            where TSignal : Signal
        {
            var signalType = typeof(TSignal);
            var subscriber = new TypedSignalSubscriber<TSignal>(action);
            var subscription = new SignalSubscription(subscriber, clearOnLoad);

            if (Subscriptions.TryAdd(signalType, new List<KeyValuePair<SignalSubscription, SignalSubscriber>>() { new (subscription, subscriber) }))
                return subscription;

            Subscriptions[signalType].Add(new KeyValuePair<SignalSubscription, SignalSubscriber>(subscription, subscriber));

            return subscription;
        }
    
        public static void UnsubscribeOnSignal(SignalSubscription subscription)
        {
            Subscriptions
                [subscription.SubscriptionType]
                .Remove(Subscriptions[subscription.SubscriptionType]
                .Find((element) => element.Key == subscription));
        }
    }
}