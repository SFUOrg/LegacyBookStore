using LegacyBookStore.Data;
using LegacyBookStore.Interfaces;
using LegacyBookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using LegacyBookStore.DTO;

namespace LegacyBookStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("BooksRateLimit")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound(new { error = "Book not found" });

            return Ok(book);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBook([FromBody] BookRequest request)

        {   
            try
            {
                Book book = new Book
                {
                    Title = request.Title,
                    Author = request.Author,
                    Price = request.Price,
                    Description = request.Description

                };
                var createdBook = await _bookService.CreateBookAsync(book);
                return CreatedAtAction(nameof(GetBooks), new { id = createdBook.Id }, createdBook);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok("Deleted");
        }
    }
}
