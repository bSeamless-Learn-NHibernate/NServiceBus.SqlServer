using NServiceBus;

namespace ReusingTransportConnection
{
    internal class OrderFulfilled : IEvent
    {
        public long OrderId { get; set; }
    }
}