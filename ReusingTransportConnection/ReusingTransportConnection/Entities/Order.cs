using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Sample.SqlServer.NoDTC.Entities
{
    internal class Order
    {
        public virtual long Id { get; set; }
        public virtual int Quantity { get; set; }
        public virtual string Product { get; set; }
        public virtual bool Shipped { get; set; }
    }

    internal class OrderMap : ClassMapping<Order>
    {
        public OrderMap()
        {
            Table("[Order]");
            Id(x => x.Id, m => m.Generator(Generators.Native));
            Property(p => p.Quantity);
            Property(p => p.Product);
            Property(p => p.Shipped);
        }
    }
}