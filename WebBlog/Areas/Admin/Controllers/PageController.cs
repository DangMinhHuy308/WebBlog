using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBlog.Data;
using WebBlog.Models;
using WebBlog.ViewModels;

namespace WebBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, INotyfService notification)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notification = notification;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> About()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
            var vm = new PageVM()
            {
                Id = page!.Id,
                Title = page!.Title,
                ShortDescription = page!.ShortDescription,
                Description = page!.Description,
                ThumbnailUrl = page!.ThumbnailUrl,
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> About(PageVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
            if (page == null) {
                _notification.Error("Page not found");
                return View(vm);
            }
            page.Title = vm.Title;
            page.ShortDescription = vm.ShortDescription;
            page.Description = vm.Description;
            if (vm.Thumbnail != null)
            {
                page.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }
            await _context.SaveChangesAsync();
            _notification.Success("About page updated succesfully");
            return RedirectToAction("About", "Page", new { area = "Admin" });

        }
        private string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, uniqueFileName);
            using (FileStream fileStream = System.IO.File.Create(filePath))
            {
                file.CopyTo(fileStream);
            }
            return uniqueFileName;

        }
    }
}
