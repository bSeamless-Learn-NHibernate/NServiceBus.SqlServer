using System;
using NServiceBus;

namespace Sample.SqlServer.NoDTC
{
    internal class Sender : IWantToRunWhenBusStartsAndStops
    {
        public IBus Bus { get; set; }

        public void Start()
        {
            Console.Out.WriteLine("Press Enter to place order");

            ConsoleKey key;
            while ((key = Console.ReadKey().Key) != ConsoleKey.Escape)
            {
                if (key == ConsoleKey.Enter)
                {
                    Bus.SendLocal(new NewOrder { Product = "iPhone 4S", Quantity = 5 });
                }
            }
        }

        public void Stop()
        {
        }
    }
}