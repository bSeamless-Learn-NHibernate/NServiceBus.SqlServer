using NServiceBus;

namespace Sample.SqlServer.NoDTC
{
    internal class NewOrder : ICommand
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
    }
}