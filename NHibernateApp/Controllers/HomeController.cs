using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernateApp.Models;
using NHibernateApp.Repositories;
using Remotion.Linq.Clauses;
using System.Diagnostics;

namespace NHibernateApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOrderRepository _orderRepository;

        public HomeController(ILogger<HomeController> logger
            , IOrderRepository orderRepository)
        {
            _logger = logger;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderItems(Guid orderId, string orderNumber)
        {
            var orderItems = new List<OrderItem>();
            if (!string.IsNullOrEmpty(orderNumber))
            {
                var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
                orderItems = order?.OrderItems.ToList();
            }
            else
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                orderItems = order?.OrderItems.ToList();
            }
            return PartialView("PartialOrderItems", orderItems);
        }
        [HttpPost]
        [Route("api/Home/AddOrUpdateOrder")]
        public async Task<IActionResult> AddOrUpdateOrder(Order order, bool isUpdate)
        {
            try
            {
                var res = await _orderRepository.AddOrderAndItemsAsync(order, isUpdate);
                //check if update
                if (!res && !isUpdate) return Json(new ApiResponse("Order number exists!"));
                //check if create
                if (!res && isUpdate) return Json(new ApiResponse("This order no longer exists!"));

                return Json(new ApiResponse());
            }
            catch (Exception ex) {
                return Json(new ApiResponse(ex.Message));
            }
            
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderItemsForm(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            var orderItems = order?.OrderItems.ToList();
            return PartialView("PartialOrderItemsForm", orderItems);
        }
        [HttpPost]
        [Route("api/Home/DeleteOrdersOrItems")]
        public async Task<IActionResult> DeleteOrdersOrItems(List<Guid> guidIds, Guid orderId, bool isDeleteOrder)
        {
            try
            {
                if(guidIds == null || !guidIds.Any()) return Json(new ApiResponse("Missing data"));
                var res = await _orderRepository.DeleteAsync(guidIds, orderId, isDeleteOrder);
                if (!res) return Json(new ApiResponse("Something occurs error!"));

                return Ok(new ApiResponse());
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse(ex.Message));
            }
        }
        [HttpPost]
        [Route("api/Home/UpdateOrderItem")]
        public async Task<IActionResult> UpdateOrderItem(OrderItem orderItem, Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return Json(new ApiResponse("This order no longer exists"));

                var item = order.OrderItems.FirstOrDefault(x=> x.Order.Id == orderId);
                if (item == null) return Json(new ApiResponse("This item no longer exists"));

                item.ProductSku = orderItem.ProductSku;
                item.ItemPrice = orderItem.ItemPrice;

                var res = await _orderRepository.AddOrderAndItemsAsync(order, true);
                if (!res) return Json(new ApiResponse("Something occurs error!"));

                return Ok(new ApiResponse());
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse(ex.Message));
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
