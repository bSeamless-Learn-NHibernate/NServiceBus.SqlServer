using NServiceBus;

namespace ReusingTransportConnection
{
    internal class NewOrder : ICommand
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
    }
}