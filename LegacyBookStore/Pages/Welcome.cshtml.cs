using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LegacyBookStore.Pages;

public class WelcomeModel : PageModel
{
    public string Name { get; private set; } = "Guest";

    public void OnGet(string name = null)
    {
        Name = string.IsNullOrEmpty(name) ? "Guest" : name;
    }
}