using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Controllers
{
    public class OrderController : Controller
    {
        private  NorthwindContext _context = new NorthwindContext();

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult List()
        {

            loadView();

            return View(_context.Orders.ToList());
        }

        [HttpPost]
        public ActionResult List(int EmployeeId, string CustomerId, DateTime? StartDate, DateTime? EndDate)
        {
            loadView();
            var query = _context.Orders.Where(o =>
                ((EmployeeId == 0 || o.Employee.EmployeeId == EmployeeId) || EmployeeId == 0) &&
                (string.IsNullOrEmpty(CustomerId) || o.Customer.CustomerId == CustomerId || string.IsNullOrEmpty(CustomerId))
            );

            if (StartDate.HasValue && EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= StartDate && o.OrderDate <= EndDate);
            }
            else if (StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= StartDate);
            }
            else if (EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= EndDate);
            }

            var orders = query.ToList();

            return View(orders);
        }
        public IActionResult Detail(int id)
        {
            var order = _context.Orders
                .Include(o => o.Employee)
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails) // Bao gồm chi tiết đơn hàng
                .ThenInclude(od => od.Product) // Bao gồm thông tin sản phẩm
                .SingleOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(); 
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var order = _context.Orders.Include(o => o.OrderDetails).SingleOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(); 
            }

            _context.OrderDetails.RemoveRange(order.OrderDetails);

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return RedirectToAction("List"); 
        }

        public IActionResult SortByTotalPrice(int modeP)
        {
            loadView();
            if(modeP == 0)
            {
                var orders = _context.Orders
                .Include(o => o.OrderDetails)
                .Select(order => new
                {
                    Order = order,
                    TotalAmount = order.OrderDetails.Sum(od => od.UnitPrice * od.Quantity)
                })
                .OrderBy(orderWithTotal => orderWithTotal.TotalAmount)
                .Select(o => o.Order)
                .ToList();
                ViewBag.modeP = 1;
                return View("List", orders);
            }
            else
            {
                loadView();
                var orders = _context.Orders
                    .Include(o => o.OrderDetails)
                    .Select(order => new
                    {
                        Order = order,
                        TotalAmount = order.OrderDetails.Sum(od => od.UnitPrice * od.Quantity)
                    })
                    .OrderByDescending(orderWithTotal => orderWithTotal.TotalAmount)
                    .Select(o => o.Order)
                    .ToList();
                ViewBag.modeP = 0;
                return View("List", orders);
            }
            
        }

        public IActionResult SortByOrderDate(int modeD)
        {
            if(modeD == 0)
            {
                loadView();
                var orders = _context.Orders
                    .OrderBy(order => order.OrderDate)
                    .ToList();
                ViewBag.modeD = 1;
                return View("List", orders);
            }
            else
            {
                loadView();
                var orders = _context.Orders
                    .OrderByDescending(order => order.OrderDate)
                    .ToList();
                ViewBag.modeD = 0;
                return View("List", orders);
            }
        }

        public void loadView()
        {
            var customers = _context.Customers.ToList();
            var employees = _context.Employees.ToList();

            ViewBag.Customers = customers;
            ViewBag.Employees = employees;
            ViewBag.modeP = 1;
            ViewBag.modeD = 1;
        }



    }
}
