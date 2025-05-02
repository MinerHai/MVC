// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using App.Areas.Identity.Models.ManageViewModels;
using App.ExtendMethods;
using App.Models;
using App.Models.Blog;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;

namespace App.Areas.Identity.Controllers
{

    [Authorize]
    [Area("Identity")]
    public class ManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ManageController> _logger;
        [TempData]
        public string StatusMessage { get; set; }

        public ManageController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailSender emailSender,
        ILogger<ManageController> logger,
        AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
        }

        //
        // GET: /Manage/Index
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Đã thay đổi mật khẩu."
                : message == ManageMessageId.SetPasswordSuccess ? "Đã đặt lại mật khẩu."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "Có lỗi."
                : message == ManageMessageId.AddPhoneSuccess ? "Đã thêm số điện thoại."
                : message == ManageMessageId.RemovePhoneSuccess ? "Đã bỏ số điện thoại."
                : "";


            var user = await GetCurrentUserAsync();
            ViewData["User"] = user;

            var editProfileModel = new AccountProfileModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Gmail = user.Email
            };
            ViewData["EditProfileModel"] = editProfileModel;

            var billingAddressModel = new BillingAdressModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Gmail = user.Email,
                ZipCode = user.ZipCode,
                City = user.City,
                Country = user.Country,
                StreetAdress = user.StreetAdress

            };
            ViewData["BillingAddressModel"] = billingAddressModel;
            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                AuthenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user),

            };


            return View(model);
        }
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                ModelState.AddModelError(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }
        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }
                ModelState.AddModelError(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "Đã loại bỏ liên kết tài khoản."
                : message == ManageMessageId.AddLoginSuccess ? "Đã thêm liên kết tài khoản"
                : message == ManageMessageId.Error ? "Có lỗi."
                : "";
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var otherLogins = schemes.Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }


        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }
            var result = await _userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }


        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }
        //
        // GET: /Manage/AddPhoneNumber
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var user = await GetCurrentUserAsync();
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            await _emailSender.SendSmsAsync(model.PhoneNumber, "Mã xác thực là: " + code);
            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }
        //
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUserAsync(), phoneNumber);
            // Send an SMS to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }
            // If we got this far, something failed, redisplay the form
            ModelState.AddModelError(string.Empty, "Lỗi thêm số điện thoại");
            return View(model);
        }
        //
        // GET: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }


        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(2, "User disabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }
        //
        // POST: /Manage/ResetAuthenticatorKey
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticatorKey()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                _logger.LogInformation(1, "User reset authenticator key.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // POST: /Manage/GenerateRecoveryCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCode()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
                _logger.LogInformation(1, "User generated new recovery code.");
                return View("DisplayRecoveryCodes", new DisplayRecoveryCodesViewModel { Codes = codes });
            }
            return View("Error");
        }
        [Route("Identity/Manage/EditProfileAsync")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileAsync(AccountProfileModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), "Manage");
            }
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error updating profile for user {user.Id}: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["StatusMessage"] = "Lỗi: Cập nhật hồ sơ thất bại.";

                return RedirectToAction(nameof(Index), "Manage");
            }
            else
            {
                TempData["StatusMessage"] = "Đã cập nhật thông tin cá nhân thành công.";
                return RedirectToAction(nameof(Index), "Manage");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBillingAdress(BillingAdressModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), "Manage");
            }
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.City = model.City;
            user.ZipCode = model.ZipCode;
            user.Country = model.Country;
            user.StreetAdress = model.StreetAdress;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error updating profile for user {user.Id}: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["StatusMessage"] = "Lỗi: Cập nhật hồ sơ thất bại.";

                return RedirectToAction(nameof(Index), "Manage");
            }
            else
            {
                TempData["StatusMessage"] = "Đã cập nhật thông tin địa chỉ thành công.";
                return RedirectToAction(nameof(Index), "Manage");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoAPI(string? id, [Bind("FileUpLoad")] UploadOneFile f)
        {
            var user = _context.Users.FirstOrDefault(p => p.Id == id);
            if (user == null)
            {
                return NotFound("Không tồn tại người dùng!!");
            }

            if (f?.FileUpLoad == null || f.FileUpLoad.Length == 0)
            {
                ModelState.AddModelError("FileUpLoad", "Vui lòng chọn một file để upload.");
                return View(f);
            }

            var uploadFolder = Path.Combine("Uploads", "Users");

            if (!string.IsNullOrEmpty(user.AvatarImg))
            {
                var oldFile = Path.Combine(uploadFolder, user.AvatarImg);
                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpLoad.FileName);
            var finalFilePath = Path.Combine(uploadFolder, fileName);

            using (var fileStream = new FileStream(finalFilePath, FileMode.Create))
            {
                await f.FileUpLoad.CopyToAsync(fileStream);
            }

            // Cập nhật ảnh đại diện mới cho user
            user.AvatarImg = fileName;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(fileName);
        }

    }

}
