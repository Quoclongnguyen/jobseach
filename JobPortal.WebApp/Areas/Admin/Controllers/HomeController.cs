using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using JobPortal.Data.DataContext;
using X.PagedList;

namespace JobPortal.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly DataDbContext _context;

        public HomeController(DataDbContext context)
        {
            _context = context;
        }

        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            // Đếm số lượng nhà tuyển dụng (Status = 2 là đã xác nhận)
            var employerCount = _context.AppUsers.Count(e => e.Status == 2);
            ViewBag.CountEmployer = employerCount;

            // Đếm số lượng người dùng (Status != -1 và != 2 là người dùng bình thường)
            var userCount = _context.AppUsers.Count(u => u.Status != -1 && u.Status != 2);
            ViewBag.CountUser = userCount;

            // Đếm số lượng công việc
            var jobCount = _context.Jobs.Count();
            ViewBag.CountJob = jobCount;

            return View();
        }

        [Route("list-employer")]
        public IActionResult ListEmployer(int? page)
        {
            int pageSize = 5; // Số lượng nhà tuyển dụng trên mỗi trang
            var employers = _context.AppUsers
                                    .OrderByDescending(e => e.Id)
                                    .Where(e => e.Status == 2) // Chỉ lấy các nhà tuyển dụng đã xác nhận
                                    .ToList();

            return View(employers.ToPagedList(page ?? 1, pageSize));
        }

        [Route("list-user")]
        public IActionResult ListUser(int? page)
        {
            int pageSize = 5; // Số lượng người dùng trên mỗi trang
            var users = _context.AppUsers
                                .OrderByDescending(u => u.Id)
                                .Where(u => u.Status != -1 && u.Status != 2) // Chỉ lấy người dùng bình thường
                                .ToList();

            return View(users.ToPagedList(page ?? 1, pageSize));
        }
    }
}
