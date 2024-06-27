using NHibernateApp.Models;

namespace NHibernateApp.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetOrdersAsync();
        Task<bool> AddOrderAndItemsAsync(Order order, bool isUpdate);
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<bool> DeleteAsync(List<Guid> guidIds, Guid orderId, bool isDeleteOrder);
    }
}
