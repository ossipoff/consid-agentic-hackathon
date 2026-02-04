using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using SogneAgent.Models;

namespace SogneAgent.Plugins;

/// <summary>
/// Semantic Kernel plugin for querying Danish parish data from Dataforsyningen API
/// </summary>
public class ParishPlugin
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.dataforsyningen.dk/sogne";

    public ParishPlugin()
    {
        _httpClient = new HttpClient();
    }

    [KernelFunction("search_parishes_by_name")]
    [Description("Search for Danish parishes (sogne) by name. Returns a list of parishes matching the search query.")]
    public async Task<string> SearchByNameAsync(
        [Description("The name or partial name of the parish to search for (e.g., 'København', 'Roskilde', 'Trinitatis')")]
        string query)
    {
        try
        {
            var url = $"{BaseUrl}?q={Uri.EscapeDataString(query)}";
            var response = await _httpClient.GetStringAsync(url);
            var parishes = JsonSerializer.Deserialize<List<Parish>>(response);

            if (parishes == null || parishes.Count == 0)
            {
                return $"No parishes found matching '{query}'.";
            }

            var results = parishes.Select(p =>
                $"- {p.Name} (Code: {p.Code})" +
                (p.VisualCenter != null && p.VisualCenter.Length >= 2
                    ? $" - Center: [{p.VisualCenter[0]:F4}, {p.VisualCenter[1]:F4}]"
                    : ""));

            return $"Found {parishes.Count} parish(es) matching '{query}':\n{string.Join("\n", results)}";
        }
        catch (Exception ex)
        {
            return $"Error searching for parishes: {ex.Message}";
        }
    }

    [KernelFunction("get_parish_details")]
    [Description("Get detailed information about a specific Danish parish by its code. Use this when you have a parish code and need full details.")]
    public async Task<string> GetParishDetailsAsync(
        [Description("The unique parish code (e.g., '7003' for Trinitatis, '7002' for Vor Frue)")]
        string code)
    {
        try
        {
            var url = $"{BaseUrl}/{Uri.EscapeDataString(code)}";
            var response = await _httpClient.GetStringAsync(url);
            var parish = JsonSerializer.Deserialize<Parish>(response);

            if (parish == null)
            {
                return $"Parish with code '{code}' not found.";
            }

            var details = new List<string>
            {
                $"Name: {parish.Name}",
                $"Code: {parish.Code}"
            };

            if (parish.VisualCenter != null && parish.VisualCenter.Length >= 2)
            {
                details.Add($"Visual Center (Longitude, Latitude): [{parish.VisualCenter[0]:F6}, {parish.VisualCenter[1]:F6}]");
            }

            if (parish.BoundingBox != null && parish.BoundingBox.Length >= 4)
            {
                details.Add($"Bounding Box: [MinLon: {parish.BoundingBox[0]:F4}, MinLat: {parish.BoundingBox[1]:F4}, MaxLon: {parish.BoundingBox[2]:F4}, MaxLat: {parish.BoundingBox[3]:F4}]");
            }

            if (parish.Changed.HasValue)
            {
                details.Add($"Last Changed: {parish.Changed.Value:yyyy-MM-dd}");
            }

            if (parish.GeoChanged.HasValue)
            {
                details.Add($"Geometry Changed: {parish.GeoChanged.Value:yyyy-MM-dd}");
            }

            return string.Join("\n", details);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return $"Parish with code '{code}' not found.";
        }
        catch (Exception ex)
        {
            return $"Error getting parish details: {ex.Message}";
        }
    }

    [KernelFunction("list_parishes")]
    [Description("List Danish parishes, optionally filtered by a partial name. Use this to discover available parishes or browse parishes in a region.")]
    public async Task<string> ListParishesAsync(
        [Description("Optional: partial name to filter parishes (e.g., 'Køben' for Copenhagen area parishes). Leave empty to get a sample of all parishes.")]
        string? nameFilter = null,
        [Description("Maximum number of parishes to return (default: 20, max: 100)")]
        int limit = 20)
    {
        try
        {
            limit = Math.Min(Math.Max(limit, 1), 100);

            var url = string.IsNullOrWhiteSpace(nameFilter)
                ? $"{BaseUrl}?per_side={limit}"
                : $"{BaseUrl}?q={Uri.EscapeDataString(nameFilter)}&per_side={limit}";

            var response = await _httpClient.GetStringAsync(url);
            var parishes = JsonSerializer.Deserialize<List<Parish>>(response);

            if (parishes == null || parishes.Count == 0)
            {
                return nameFilter != null
                    ? $"No parishes found matching '{nameFilter}'."
                    : "No parishes found.";
            }

            var results = parishes.Select(p => $"- {p.Name} (Code: {p.Code})");
            var header = nameFilter != null
                ? $"Parishes matching '{nameFilter}' (showing {parishes.Count}):"
                : $"Sample of Danish parishes (showing {parishes.Count}):";

            return $"{header}\n{string.Join("\n", results)}";
        }
        catch (Exception ex)
        {
            return $"Error listing parishes: {ex.Message}";
        }
    }
}
