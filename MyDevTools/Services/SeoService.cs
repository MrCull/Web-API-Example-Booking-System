using Microsoft.AspNetCore.Components;

namespace MyDevTools.Services;

public class SeoService
{
    private readonly NavigationManager _navigationManager;

    public SeoService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public string GetStructuredData()
    {
        var structuredData = new
        {
            context = "https://schema.org",
            type = "WebApplication",
            name = "My Dev Tools",
            description = "Free online developer tools including JSON formatter, SQL formatter, GUID generator, hash generator, and more.",
            url = _navigationManager.BaseUri,
            applicationCategory = "DeveloperApplication",
            operatingSystem = "Any",
            offers = new
            {
                type = "Offer",
                price = "0",
                priceCurrency = "USD"
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(structuredData);
    }
} 