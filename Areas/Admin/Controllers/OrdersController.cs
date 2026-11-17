using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH110604_LTW.Models; 

namespace _25DH110604_LTW.Areas.Admin.Controllers
{
   
    public class OrdersController : Controller
    {
        private MyStore2Entities db = new MyStore2Entities();

       
        public ActionResult Index(string customerName, string paymentStatus, DateTime? orderDate, int page = 1)
        {
            int pageSize = 15;

            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            // Tìm theo Tên khách hàng (Dùng CustomerName thay vì FullName)
            if (!string.IsNullOrEmpty(customerName))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(customerName));
            }

            // Tìm theo Trạng thái thanh toán
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus);
            }

            // Tìm theo Ngày đặt hàng
            if (orderDate.HasValue)
            {
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) == orderDate.Value);
            }

            orders = orders.OrderByDescending(o => o.OrderDate);
            int totalItems = orders.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CustomerName = customerName;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderDate = orderDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedOrders);
        }

       
        public ActionResult DetailedOrder(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = db.Orders
                          .Include(o => o.Customer)
                          .Include(o => o.OrderDetails.Select(od => od.Product))
                          .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return HttpNotFound();

            return View(order);
        }

       
        [HttpGet]
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string paymentStatus, string shippingMethod)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            order.PaymentStatus = paymentStatus;
            order.ShippingMethod = shippingMethod;

            db.SaveChanges();
            TempData["Message"] = "Cập nhật trạng thái đơn hàng thành công!";

            return RedirectToAction("DetailedOrder", new { id = order.OrderID });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}