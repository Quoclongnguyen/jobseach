using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JobPortal.Data.Entities;
using JobPortal.Data.ViewModel;

namespace JobPortal.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/auth")]
    [Route("admin")]
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                            model.Email,
                            model.Password,
                            model.RememberMe,
                            false);
                if (result.Succeeded)
                {
                    // Lấy email từ trang đăng nhập và kiểm tra
                    var user = await userManager.FindByEmailAsync(model.Email);

                    if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
                    {
                        ModelState.AddModelError(string.Empty, "Đăng nhập không thành công.");
                        return View(model);
                    }

                    // Lấy vai trò của người dùng
                    var roles = await userManager.GetRolesAsync(user);
                    // Kiểm tra vai trò
                    if (!roles.Contains("Admin"))
                    {
                        await signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Trang này chỉ dành cho tài khoản quản trị viên.");
                    }
                    else if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không hợp lệ. Vui lòng thử lại!");
                    return View(model);
                }
            }
            return View(model);
        }
    }
}
