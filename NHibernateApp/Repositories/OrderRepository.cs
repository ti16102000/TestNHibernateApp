using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;
using NHibernateApp.Models;
using System.Transactions;

namespace NHibernateApp.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public async Task<bool> AddOrderAndItemsAsync(Order order, bool isUpdate)
        {
            using (var session = NHibernateSession.OpenSession())
            {
                var existOrder = await session.Query<Order>().AnyAsync(a => a.OrderNumber.ToLower().Equals(order.OrderNumber.ToLower()));
                //check if create
                if (existOrder && !isUpdate) return false;
                //check if update
                if(!existOrder && isUpdate) return false;

                using (var transaction = session.BeginTransaction())
                {
                    foreach (var item in order.OrderItems)
                    {
                        item.Order = order;
                    }                                       
                    session.Evict(order); // Detach the order entity from the session
                    session.Clear();// Clear the entire session to detach all entities
                    if (!session.Contains(order))
                    {
                        session.SaveOrUpdate(order); // Save or update the order entity
                    }
                    //session.Merge(order);
                    transaction.Commit();
                    return true;
                }
            }
        }

        public async Task<bool> DeleteAsync(List<Guid> guidIds, Guid orderId, bool isDeleteOrder)
        {
            using (var session = NHibernateSession.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (isDeleteOrder)
                    {
                        var orders = await session.Query<Order>().Where(a => guidIds.Contains(a.Id)).ToListAsync();
                        foreach (var item in orders)
                        {
                            await session.DeleteAsync(item);
                        }
                    }
                    else
                    {
                        var order = await session.Query<Order>().Where(a=>a.Id == orderId).FirstOrDefaultAsync();
                        if(order == null) return false;                       

                        var delOrderItems = await session.Query<OrderItem>().Where(a => guidIds.Contains(a.Id)).ToListAsync();
                        foreach(var item in delOrderItems)
                        {
                            order.OrderItems.Remove(item);
                        }
                        await session.SaveOrUpdateAsync(order);
                    }                   
                    transaction.Commit();
                    return true;
                }
            }               
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using (var session = NHibernateSession.OpenSession())
            {
                var order = await session.Query<Order>()
                                        .Where(o => o.Id == id)
                                        .FirstOrDefaultAsync();
                if (order == null) return null;

                NHibernateUtil.Initialize(order.OrderItems);
                return order;
            }
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            using (var session = NHibernateSession.OpenSession())
            {
                var order = await session.Query<Order>().FirstOrDefaultAsync(a => a.OrderNumber.ToLower().Equals(orderNumber));
                if (order == null) return null;

                NHibernateUtil.Initialize(order.OrderItems);
                return order;
            }                
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            using (var session = NHibernateSession.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var orders = await session.Query<Order>().Fetch(o => o.OrderItems).ToListAsync();

                    transaction.Commit();
                    return orders;
                }
                
            }
        }
    }
}
