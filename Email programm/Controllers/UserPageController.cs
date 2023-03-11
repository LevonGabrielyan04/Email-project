using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Email_programm.Data;
using Email_programm.Models;

namespace Email_programm.Controllers
{
    public class UserPageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserPageController(AppDbContext context,IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _currentUser = _context.Users.Find(1);
        }

        // GET: UserPage
        public async Task<IActionResult> Index()
        {
            return _context.Users != null ?
                        View(_context.Messages.Where(m=>m.Sender == _currentUser.Adress || m.Reciver == _currentUser.Adress).ToList()) :
                          Problem("Entity set 'AppDbContext.Users'  is null.");
        }


        // GET: UserPage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserPage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Sender,Reciver")] Message message)
        {
            message.Sender = _currentUser.Adress;
            _context.Add(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       

        public IActionResult Resend()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resend(int id,[Bind("Id,Content,Sender,Reciver")] Message message)
        {
            Message toEdit = _context.Messages.Find(id);
            toEdit.Reciver = message.Reciver;
            toEdit.Id = 0;
            _context.Add(toEdit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserPage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var user = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserPage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Messages == null)
            {
                return Problem("Entity set 'AppDbContext.Users'  is null.");
            }
            var user = await _context.Messages.FindAsync(id);
            if (user != null)
            {
                _context.Messages.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Exit()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("SingedInUser");
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("Password");
            return RedirectToAction("SignIn", "Authentication");
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }


    }
}
