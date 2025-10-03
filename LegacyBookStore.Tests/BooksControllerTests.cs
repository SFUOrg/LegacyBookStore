using LegacyBookStore.Data;
using LegacyBookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LegacyBookStore.Tests;

public class BooksControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BooksControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void ResetDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    private void SeedDatabase(Action<AppDbContext> seedAction = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        ResetDatabase();
        seedAction?.Invoke(context);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetBooks_ReturnsOkWithListOfBooks()
    {
        // Arrange
        SeedDatabase(context => Seeding.InitializeTestDb(context));

        // Act
        var response = await _client.GetAsync("api/books");

        // Assert
        response.EnsureSuccessStatusCode();
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.All(books, book =>
        {
            Assert.NotNull(book.Title);
            Assert.NotNull(book.Author);
            Assert.True(book.Price > 0);
        });
    }

    [Fact]
    public async Task GetBooks_ReturnsBook_WhenOneBookExists()
    {
        // Arrange
        SeedDatabase(context =>
        {
            context.Books.Add(new Book
            {
                Title = "Integration Test Book",
                Author = "Test Author",
                Price = 12.99m,
                Description = "A book for testing"
            });
        });

        // Act
        var response = await _client.GetAsync("/api/books");

        // Assert
        response.EnsureSuccessStatusCode();
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        Assert.NotNull(books);
        var book = books[2];
        Assert.Equal("Integration Test Book", book.Title);
        Assert.Equal("Test Author", book.Author);
        Assert.Equal(12.99m, book.Price);
    }

    [Fact]
    public async Task AddBook_SuccessfullyAddsBook()
    {
        // Arrange
        ResetDatabase();
        var newBook = new Book
        {
            Title = "Integration Test Book",
            Author = "Test Author",
            Price = 12.99m,
            Description = "A book for testing"
        };
        var content = new StringContent(JsonSerializer.Serialize(newBook), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("api/books", content);

        // Assert
        response.EnsureSuccessStatusCode();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booksInDb = await context.Books.ToListAsync();
        Assert.Equal(3, booksInDb.Count);
        var book = booksInDb[2];
        Assert.Equal(newBook.Title, book.Title);
        Assert.Equal(newBook.Author, book.Author);
        Assert.Equal(newBook.Price, book.Price);
        Assert.Equal(newBook.Description, book.Description);
    }

    [Theory]
    [InlineData("", "application/json", HttpStatusCode.BadRequest)]
    [InlineData("{\"title\":\"\",\"author\":\"\",\"price\":-12.99}", "application/json", HttpStatusCode.BadRequest)]
    public async Task AddBook_InvalidData_ReturnsBadRequest(string requestBody, string contentType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        ResetDatabase();
        var content = new StringContent(requestBody, Encoding.UTF8, contentType);

        // Act
        var response = await _client.PostAsync("api/books", content);

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBookById_DeletesBook()
    {
        // Arrange
        SeedDatabase(context => Seeding.InitializeTestDb(context));
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookToDelete = context.Books.First();

        // Act
        var response = await _client.DeleteAsync($"/api/books/{bookToDelete.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.False(context.Books.Any(b => b.Id == bookToDelete.Id));
    }

    [Fact]
    public async Task DeleteNotExistingBookById_ReturnsNotFound()
    {
        // Arrange
        SeedDatabase(context => Seeding.InitializeTestDb(context));

        // Act
        var response = await _client.DeleteAsync("/api/books/15");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}