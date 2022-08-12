using GSRecordMining.Models;
using GSRecordMining.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GSRecordMining.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SmbService _smbService;
        private readonly DBService _dbService;

        public DashboardController(DBService dbService, SmbService smbService )
        {
            _smbService = smbService;
            _dbService = dbService;
        }

        public IActionResult Index()
        {
            return Redirect("~/filter.html");
        }
        public IActionResult LogIn()
        {
            ViewBag.RequestPath = HttpContext.Request.Query["RequestPath"].ToString();
            return View();
        }
        
        [HttpPost]
        public async Task<JsonResult> verifyLogin(Models.SystemUser systemUser)
        {
            var result =await _dbService.verifyLogin(systemUser);
            if (result.statusCode == 200)
            {
                var claims = new List<Claim>() {
                                    new Claim(ClaimTypes.Sid, result.data.ToString()??""),
                                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignOutAsync("GSRecordMining");
                await HttpContext.SignInAsync("GSRecordMining", principal, new AuthenticationProperties()
                {
                    IsPersistent = false
                });
            }
            return Json(result);
        }
        
        [HttpPost]
        public async Task<JsonResult> isLogin()
        {
            return Json(User.Identity.IsAuthenticated);
        }
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("GSRecordMining");
            return RedirectToAction("LogIn");
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> checkNASConfig()
        {
            return Json(await _dbService.checkNASConfig());
        }
        
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> getNASConfig()
        {
            return Json(await _dbService.getNASConfig());
        }
        
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> saveNASConfig(Models.NAS nas)
        {
            return Json(await _dbService.saveNASConfig(nas));
        }
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> getIndexedRecord(Models.Filter? filter)
        {
            return Json(await _dbService.getIndexedRecord(filter));
        }
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> buildIndexedRecord()
        {
            return Json(await _dbService.buildIndexedRecord());
        }
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> getFromNAS()
        {
            return Json(await _dbService.getFromNAS());
        }
        [Authorize]
        [HttpGet]
        public async Task<FileStreamResult> getCDR(string id)
        {
            return await _dbService.getCDR(id);
        }
    }
}
