using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Email_programm.Data;
using Email_programm.Models;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Azure;
using System.Globalization;

namespace Email_programm.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationController(AppDbContext context,IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // GET: Authentication/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authentication/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Adress,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SignIn));
            }
            return View(user);
        }

        public IActionResult SignIn(string adress,string password)
        {
            if (adress != null)
            {
                User RealUser = _context.Users.First(m => m.Adress == adress);
                if (RealUser.Password == password)
                {
                    User userToUpdate = _context.Users.Find(1);
                    userToUpdate.Adress = adress;
                    _context.Update(userToUpdate);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "UserPage");
                }
                else
                {
                    return RedirectToAction(nameof(SignIn));
                }
            }
            
            string address =  _httpContextAccessor.HttpContext.Request.Cookies["SingedInUser"];
            string passwordFromCookie = _httpContextAccessor.HttpContext.Request.Cookies["Password"];
            if (address != null)
            {
                return RedirectToAction("SignIn", new
                {
                    adress = address,
                    password = passwordFromCookie 
                });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([Bind("Id,Adress,Password")] User user)
        {
            var cookieOptions = new CookieOptions();
            cookieOptions.Expires = DateTime.Now.AddDays(30);
            cookieOptions.Path = "/"; 
            User userFromDb = _context.Users.First(u => u.Adress == user.Adress);
            if(userFromDb.Password == user.Password)
            {
                User userToUpdate = _context.Users.Find(1);
                userToUpdate.Adress = user.Adress;
                _context.Update(userToUpdate);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("SingedInUser",userFromDb.Adress , cookieOptions);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("Password",userFromDb.Password , cookieOptions);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index","UserPage");  

            }
            return RedirectToAction("Index","Authentication");  
        }
       

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
