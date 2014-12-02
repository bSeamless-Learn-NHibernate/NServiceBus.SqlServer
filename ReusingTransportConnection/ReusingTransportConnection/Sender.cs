using System;
using NServiceBus;

namespace ReusingTransportConnection
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
                    #region Sender
                    Bus.SendLocal(new NewOrder { Product = "iPhone 4S", Quantity = 5 });
                    #endregion
                }
            }
        }

        public void Stop()
        {
        }
    }
}