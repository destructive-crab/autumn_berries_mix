using System;

public class SignalSubscriber
{
    public Type SubscriptionType { get; protected set; }
    
    public virtual void InvokeSubscription(Signal nonTypedEvent) { }
}