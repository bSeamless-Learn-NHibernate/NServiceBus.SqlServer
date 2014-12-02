using NServiceBus;

namespace Sample.SqlServer.NoDTC
{
    internal class OrderPlaced : IMessage
    {
        public long OrderId { get; set; }
    }
}