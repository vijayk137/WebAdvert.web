using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.web.Models.Accounts;

namespace WebAdvert.web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountsController(SignInManager<CognitoUser> signInManager,
                                  UserManager<CognitoUser> userManager,
                                  CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }


        public async Task<IActionResult> Signup()
        {
            var model = new SignUpModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignUpModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "user exists");
                    return View(model);
                }
                //user.Attributes.Add(CognitoAttributeConstants.name, model.Email);

                var usercreated = await _userManager.CreateAsync(user, model.Password);
                if (usercreated.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
            }
            return View();
        }
        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }
        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password,
                                                                      model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginError", "Email or password mismatch");
                }
            }
            return View("Login", model);
        }
        public async Task<IActionResult> Confirm()
        {
            var model = new ConfirmModel();
            return View(model);
        }
        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> Confirm_post(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "Not found user");
                    return View(model);
                }
                
                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);

                }
            }
            return View();
        }
    }
}