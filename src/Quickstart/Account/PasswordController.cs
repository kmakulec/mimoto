using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityService.Quickstart.Account
{
    public class PasswordController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public PasswordController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> Recover()
        {
            var vm = new RecoverPasswordViewModel();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Recover(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(RecoverConfirmation));
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "CheckIt4You reset hasła", callback);
            return RedirectToAction(nameof(RecoverConfirmation));
        }

        [HttpGet]
        public async Task<IActionResult> RecoverConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var vm = new ResetPasswordViewModel { Token = token, Email = email };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));
            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

    }
}
