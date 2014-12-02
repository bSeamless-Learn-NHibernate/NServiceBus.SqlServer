using System;
using NServiceBus.Persistence.NHibernate;
using NServiceBus.Saga;
using ReusingTransportConnection.Entities;

namespace ReusingTransportConnection
{
    internal class OrderLifecycleSaga : Saga<OrderLifecycleSaga.OrderData>, IAmStartedByMessages<NewOrder>,
        IHandleTimeouts<ProcessingCompleted>
    {
        public NHibernateStorageContext NHibernateStorageContext { get; set; }

        public void Handle(NewOrder message)
        {
            Console.Out.WriteLine("Processing order");

            #region OrderProcessing
            RequestTimeout(TimeSpan.FromSeconds(5), new ProcessingCompleted());

            Data.Product = message.Product;
            Data.Quantity = message.Quantity;
            #endregion
        }

        public void Timeout(ProcessingCompleted state)
        {
            Console.Out.WriteLine("Order fulfilled");

            #region NotifyFulfilled
            var order = new Order
            {
                Product = Data.Product,
                Quantity = Data.Quantity
            };

            NHibernateStorageContext.Session.Save(order);

            Bus.Publish(new OrderFulfilled
            {
                OrderId = order.Id,
            });
            #endregion

            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderData> mapper)
        {
        }

        internal class OrderData : ContainSagaData
        {
            public virtual int Quantity { get; set; }
            public virtual string Product { get; set; }
        }
    }
}