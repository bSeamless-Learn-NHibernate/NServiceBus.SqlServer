using System;
using NServiceBus;
using NServiceBus.Persistence.NHibernate;

namespace ReusingTransportConnection
{
    internal class OrderFulfilledHandler : IHandleMessages<OrderFulfilled>
    {
        public NHibernateStorageContext NHibernateStorageContext { get; set; }

        public void Handle(OrderFulfilled message)
        {
            Console.Out.WriteLine("Order #{0} being shipped now", message.OrderId);

            #region Ship
            var order = NHibernateStorageContext.Session.Get<Entities.Order>(message.OrderId);

            order.Shipped = true;
            #endregion
        }
    }
}