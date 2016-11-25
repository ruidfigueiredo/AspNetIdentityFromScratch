using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace AspNetIdentityFromScratch.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMessageService _messageService;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IMessageService messageService)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._messageService = messageService;
        }

        public IActionResult Register()
        {                                    
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Register(string email, string password, string repassword)
        {
            if (password != repassword)
            {
                ModelState.AddModelError(string.Empty, "Password don't match");
                return View();
            }

            var newUser = new IdentityUser 
            {
                UserName = email,
                Email = email
            };

            var userCreationResult = await _userManager.CreateAsync(newUser, password);
            if (!userCreationResult.Succeeded)
            {
                foreach(var error in userCreationResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View();
            }

            await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, "Administrator"));
            
            
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var tokenVerificationUrl = Url.Action("VerifyEmail", "Account", new {id = newUser.Id, token = emailConfirmationToken}, Request.Scheme);

            await _messageService.Send(email, "Verify your email", $"Click <a href=\"{tokenVerificationUrl}\">here</a> to verify your email");

            return Content("Check your email for a verification link");
        }


        public async Task<IActionResult> VerifyEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
                throw new InvalidOperationException();
            
            var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, token);
            if (!emailConfirmationResult.Succeeded)            
                return Content(emailConfirmationResult.Errors.Select(error => error.Description).Aggregate((allErrors, error) => allErrors += ", " + error));                            

            return Content("Email confirmed, you can now log in");
        }

        public IActionResult Login()
        {
            return View();
        }        

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login");
                return View();
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Confirm your email first");
                return View();
            }

            var passwordSignInResult = await _signInManager.PasswordSignInAsync(user, password, isPersistent: rememberMe, lockoutOnFailure: false);
            
            if (!passwordSignInResult.Succeeded)
            {                
                await _userManager.AccessFailedAsync(user);
                ModelState.AddModelError(string.Empty, "Invalid login");
                return View();                
            }

            return Redirect("~/");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Content("Check your email for a password reset link");

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResetUrl = Url.Action("ResetPassword", "Account", new {id = user.Id, token = passwordResetToken}, Request.Scheme);

            await _messageService.Send(email, "Password reset", $"Click <a href=\"" + passwordResetUrl + "\">here</a> to reset your password");

            return Content("Check your email for a password reset link");
        }

        public IActionResult ResetPassword(string id, string token)
        {           
            return View();            
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id, string token, string password, string repassword)
        {           
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException();

            if (password != repassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match");
                return View();
            }

            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, password);
            if (!resetPasswordResult.Succeeded)
            {
                foreach(var error in resetPasswordResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View();                
            }

            return Content("Password updated");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }                    
    }
}
