﻿using ChatAp103.Hubs;
using ChatAp103.Models;
using ChatAp103.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAp103.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IHubContext<ChatHub> _hubContext;


        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _userManager=userManager;
            _signInManager=signInManager;
            _hubContext=hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Chat()
        {
            List<AppUser> users = _userManager.Users.ToList();
            return View(users);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser dbUser = _userManager.FindByNameAsync(login.Username).Result;
            if (dbUser==null)
            {
                ModelState.AddModelError("", "userrname or pasword is not valid");
            }
            Microsoft.AspNetCore.Identity.SignInResult result = _signInManager.PasswordSignInAsync(dbUser, login.Password, true, true).Result;

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "userrname or pasword is not valid");
            }
           await  _signInManager.SignInAsync(dbUser, true);
            return RedirectToAction("chat", "home");
        }

        public IActionResult Register()
        {
            AppUser user1 = new AppUser { UserName="_ilkin", Fullname="Ilkin" };
            AppUser user2 = new AppUser { UserName="_yusif", Fullname="Yusif" };
            AppUser user3 = new AppUser { UserName="_jale", Fullname="Jale" };
            AppUser user4 = new AppUser { UserName="_nergiz", Fullname="Nergiz" };

           var result1= _userManager.CreateAsync(user1, "User@123").Result;
            var result2 = _userManager.CreateAsync(user2, "User@123").Result;
            var result3 = _userManager.CreateAsync(user3, "User@123").Result;
            var result4 = _userManager.CreateAsync(user4, "User@123").Result;

            return Content("userlar craeted");
        }
        public async Task< IActionResult> Logout()
        {
             await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        public async Task<IActionResult> SendSpecificUser(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);

           await  _hubContext.Clients.Client(user.ConnectionId).SendAsync("test", user.Fullname);

            return View();
        }
    }
}
