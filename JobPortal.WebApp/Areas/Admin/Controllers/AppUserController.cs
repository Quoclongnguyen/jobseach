using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using JobPortal.Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.WebApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("admin/users")]
    public class AppUserController : Controller
    {
        private readonly RoleManager<AppRole> roleManager;
        private readonly UserManager<AppUser> userManager;
        private readonly DataDbContext _context;

        public AppUserController(RoleManager<AppRole> roleManager, DataDbContext context, UserManager<AppUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this._context = context;
        }

        [Route("")]
        public IActionResult Index()
        {
            // Lấy danh sách tất cả người dùng (trừ admin)
            var users = userManager.Users.Where(u => u.Status != -1).ToList();
            var userRoles = new List<Dictionary<string, string>>();

            foreach (var user in users)
            {
                var roles = userManager.GetRolesAsync(user).Result;
                var roleNames = new List<string>();

                foreach (var role in roles)
                {
                    var roleName = roleManager.FindByNameAsync(role).Result.Name;
                    roleNames.Add(roleName);
                }

                userRoles.Add(new Dictionary<string, string>
                {
                    { "userId", user.Id.ToString()},
                    { "Email", user.Email },
                    { "Avatar", user.UrlAvatar },
                    { "RoleNames", string.Join(", ", roleNames) }
                });
            }

            return View(userRoles);
        }

        // Phân quyền người dùng
        [Route("manage-user-roles/{userId}")]
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(Guid userId)
        {
            ViewBag.userId = userId;

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                ViewBag.ErrorMessage = $"Không tìm thấy người dùng với ID = {userId}";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();

            foreach (var role in roleManager.Roles)
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }

                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [Route("manage-user-roles/{userId}")]
        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                ViewBag.ErrorMessage = $"Không tìm thấy người dùng với ID = {userId}";
                return View("NotFound");
            }

            // Lấy danh sách các vai trò của người dùng hiện tại
            var roles = await userManager.GetRolesAsync(user);

            // Xóa các vai trò hiện tại của người dùng
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa các vai trò hiện tại của người dùng");
                return View(model);
            }

            // Thêm các vai trò mới cho người dùng
            result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể thêm các vai trò đã chọn cho người dùng");
                return View(model);
            }

            // Lấy danh sách các vai trò được chọn
            var selectedRoles = model.Where(x => x.IsSelected).Select(y => y.RoleName).ToList();

            // Cập nhật trạng thái người dùng dựa trên vai trò được chọn
            if (selectedRoles.Contains("Admin"))
            {
                user.Status = -1; // Trạng thái = -1 nếu có vai trò "Admin"
            }
            else if (selectedRoles.Contains("Employer"))
            {
                user.Status = 2; // Trạng thái = 2 nếu có vai trò "Confirmed"
            }
            else if (selectedRoles.Contains("User"))
            {
                user.Status = 1; // Trạng thái = 1 nếu có vai trò "Waiting"
            }

            // Cập nhật người dùng trong cơ sở dữ liệu
            _context.AppUsers.Update(user);

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Sau khi phân quyền thành công, chuyển hướng về trang Index
            return RedirectToAction("Index");
        }

    }
}
