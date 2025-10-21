using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly AppDbContext _context;

        public TodoController(AppDbContext context)
        {
            _context = context;
        }

        // Index with Search + Filter
        public async Task<IActionResult> Index(string searchString, string status)
        {
            IQueryable<TodoItem> items = _context.TodoItems;

            // Filter by search text
            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(t => t.Title.Contains(searchString));
            }

            // Filter by status
            if (status == "completed")
                items = items.Where(t => t.IsComplete);
            else if (status == "pending")
                items = items.Where(t => !t.IsComplete);

            var itemList = await items.ToListAsync();

            // Store search/filter values for the view
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = status;

            return View(itemList);
        }

        // Create (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // Edit (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TodoItem item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.TodoItems.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Toggle Complete
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsComplete = !item.IsComplete;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Details
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        public IActionResult Filter(string status)
        {
            var todos = _context.TodoItems.AsQueryable();

            if (status == "completed")
            {
                todos = todos.Where(t => t.IsComplete);
            }
            else if (status == "pending")
            {
                todos = todos.Where(t => !t.IsComplete);
            }

            return View("Index", todos.ToList());
        }

    }
}
