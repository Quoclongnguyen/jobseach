﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using JobPortal.Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using JobPortal.Common;
using X.PagedList;

namespace JobPortal.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/skill")]
    [Authorize(Roles = "Admin")]
    public class SkillController : Controller
    {
        private readonly DataDbContext _context;

        public SkillController(DataDbContext context)
        {
            _context = context;
        }

        [Route("index")]
        [Route("")]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10; // Số lượng kỹ năng hiển thị trên mỗi trang

            var skills = await _context.Skills.OrderBy(i => i.Name).ToListAsync();
            return View(skills.ToPagedList(page ?? 1, pageSize));
        }

        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSkillViewModel model)
        {
            if (ModelState.IsValid)
            {
                string POST_IMAGE_PATH = "images/skills/";

                if (model.Logo != null)
                {
                    // Upload hình ảnh logo
                    var logo = UploadImage.UploadImageFile(model.Logo, POST_IMAGE_PATH);

                    Skill skill = new Skill()
                    {
                        Name = model.Name,
                        Slug = TextHelper.ToUnsignString(model.Name).ToLower(), // Tạo slug từ tên
                        Logo = logo
                    };
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        [Route("update/{id}")]
        public IActionResult Update(int id)
        {
            // Lấy kỹ năng theo ID
            var skill = _context.Skills.Where(u => u.Id == id).First();
            return View(skill);
        }

        [Route("update/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateSkillViewModel model)
        {
            // Cập nhật thông tin kỹ năng
            Skill skill = _context.Skills.Where(u => u.Id == id).First();
            skill.Name = model.Name;
            skill.Slug = TextHelper.ToUnsignString(skill.Name).ToLower();
            _context.Skills.Update(skill);
            await _context.SaveChangesAsync();
            return Redirect("/admin/skill");
        }

        [Route("update-image/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImage(int id, IFormFile Logo)
        {
            string POST_IMAGE_PATH = "images/skills/";

            if (Logo != null)
            {
                // Lấy kỹ năng và cập nhật hình ảnh
                Skill skill = _context.Skills.Where(u => u.Id == id).First();
                string oldLogoImage = skill.Logo;
                var newLogoImage = UploadImage.UploadImageFile(Logo, POST_IMAGE_PATH);
                skill.Logo = newLogoImage;
                _context.Update(skill);
                await _context.SaveChangesAsync();

                // Xóa hình ảnh cũ nếu tồn tại
                if (!string.IsNullOrEmpty(oldLogoImage))
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "skills", oldLogoImage);
                    DeleteImage.DeleteImageFile(oldImagePath);
                }

                return Redirect("/admin/skill/update/" + id);
            }
            return Redirect("/admin/skill/update/" + id);
        }

        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                // Xóa kỹ năng và hình ảnh liên quan
                Skill skill = _context.Skills.Where(p => p.Id == id).First();
                if (skill != null)
                {
                    string imageName = skill.Logo;
                    _context.Skills.Remove(skill);
                    _context.SaveChanges();
                    if (!string.IsNullOrEmpty(imageName))
                    {
                        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "skills", imageName);
                        DeleteImage.DeleteImageFile(imagePath);
                    }
                }
                return Redirect("/admin/skill");
            }
            catch (System.Exception)
            {
                return Redirect("/admin/skill");
            }
        }

        [HttpPost("delete-selected")]
        public async Task<IActionResult> DeleteSelected(int[] listDelete)
        {
            // Xóa danh sách các kỹ năng được chọn
            foreach (int id in listDelete)
            {
                var skill = await _context.Skills.FindAsync(id);
                if (skill != null)
                {
                    string imageName = skill.Logo;
                    _context.Skills.Remove(skill);
                    if (!string.IsNullOrEmpty(imageName))
                    {
                        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "skills", imageName);
                        DeleteImage.DeleteImageFile(imagePath);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
