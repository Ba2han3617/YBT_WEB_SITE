using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ybt.Core.Entities;
using Ybt.Web.Models;

namespace Ybt.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        return View(user);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("strict-limit")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        ModelState.AddModelError(string.Empty, "Admin hesapları yalnızca yönetici giriş ekranından giriş yapabilir.");
                        return View(model);
                    }
                }
            }

            Microsoft.AspNetCore.Identity.SignInResult result;
            if (user != null)
            {
                result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            }
            else
            {
                result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            }

            if (result.Succeeded)
            {
                TempData["Success"] = "Giriş başarılı! Hoş geldiniz.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız çok sayıda hatalı giriş nedeniyle geçici olarak kilitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi ile zaten kayıtlı bir kullanıcı bulunmaktadır.");
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                FullName = $"{model.FirstName} {model.LastName}",
                Faculty = model.Faculty,
                TcNo = model.TcNo,
                StudentNumber = model.StudentNumber,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Yazılım ve Bilişim Topluluğu'na kaydınız başarıyla tamamlandı! Aramıza hoş geldiniz.";
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Başarıyla çıkış yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet("adminstrator")]
    [HttpGet("administrative")]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["IsAdminLogin"] = true;
        return View("AdminLogin");
    }

    [HttpPost("adminstrator")]
    [HttpPost("administrative")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("strict-limit")]
    public async Task<IActionResult> AdminLogin(LoginViewModel model, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        ModelState.AddModelError(string.Empty, "Bu giriş ekranı yalnızca yöneticiler içindir.");
                        ViewData["IsAdminLogin"] = true;
                        return View("AdminLogin", model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Bu giriş ekranı yalnızca yöneticiler içindir.");
                    ViewData["IsAdminLogin"] = true;
                    return View("AdminLogin", model);
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Hesabınız çok sayıda hatalı giriş nedeniyle geçici olarak kilitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                    ViewData["IsAdminLogin"] = true;
                    return View("AdminLogin", model);
                }
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
        }
        ViewData["IsAdminLogin"] = true;
        return View("AdminLogin", model);
    }
}
