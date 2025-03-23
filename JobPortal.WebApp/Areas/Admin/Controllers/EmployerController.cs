using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using Microsoft.AspNetCore.Identity;
using JobPortal.Data.ViewModel;
using X.PagedList;

namespace JobPortal.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/apply-employer")]
    public class EmployerController : Controller
    {
        private readonly DataDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public EmployerController(DataDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("index/{status}")]
        [Route("{status}")]
        public async Task<IActionResult> Index(int status, int? page)
        {
            int pageSize = 5; // Số lượng người dùng trên mỗi trang

            var employersQuery = (from emp in _context.AppUsers
                                  orderby emp.CreateDate descending
                                  select new ListEmployersViewWModel()
                                  {
                                      Id = emp.Id,
                                      FullName = emp.FullName,
                                      Description = emp.Description,
                                      Contact = emp.Contact,
                                      Location = emp.Location,
                                      WebsiteURL = emp.WebsiteURL,
                                      UrlAvatar = emp.UrlAvatar,
                                      RegisterDate = emp.CreateDate,
                                      ProvinceName = emp.Province.Name,
                                      Status = emp.Status
                                  });

            List<ListEmployersViewWModel> employers;

            switch (status)
            {
                case 0: // Bị từ chối
                    employers = await employersQuery.Where(p => p.Status == 0).ToListAsync();
                    break;
                case 1: // Đang chờ duyệt
                    employers = await employersQuery.Where(p => p.Status == 1).ToListAsync();
                    break;
                case 2: // Đã xác nhận
                    employers = await employersQuery.Where(p => p.Status == 2).ToListAsync();
                    break;
                case 3: // Tất cả ngoại trừ Admin và tài khoản mới tạo
                    employers = await employersQuery.Where(p => p.Status != null && p.Status != -1).ToListAsync();
                    break;
                default:
                    employers = await employersQuery.Where(p => p.Status != null && p.Status != -1).ToListAsync();
                    break;
            }

            return View(employers.ToPagedList(page ?? 1, pageSize));
        }

        [Route("update-employer-status/{id}/{status}")]
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(Guid id, int status)
        {
            // Tìm nhà tuyển dụng theo ID
            var employer = await _context.AppUsers.FirstOrDefaultAsync(user => user.Id == id);

            if (employer == null)
            {
                return NotFound($"Không tìm thấy nhà tuyển dụng với ID: {id}");
            }

            // Cập nhật trạng thái
            employer.Status = status;
            _context.AppUsers.Update(employer);
            await _context.SaveChangesAsync();

            // Lấy thông tin người dùng
            var user = await _userManager.FindByIdAsync(id.ToString());

            // Kiểm tra và cập nhật vai trò theo trạng thái
            if (status == 0) // Bị từ chối
            {
                if (await _userManager.IsInRoleAsync(user, "Employer"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Employer");
                    await _userManager.AddToRoleAsync(user, "User");
                }
            }
            else if (status == 2) // Đã xác nhận
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Employer");
            }

            return Redirect("/admin/apply-employer/" + status);
        }
    }
}
