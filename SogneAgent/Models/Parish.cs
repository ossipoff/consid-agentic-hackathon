using System.Text.Json.Serialization;

namespace SogneAgent.Models;

/// <summary>
/// Represents a Danish parish (sogn) from the Dataforsyningen API
/// </summary>
public class Parish
{
    [JsonPropertyName("navn")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("kode")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("visueltcenter")]
    public double[]? VisualCenter { get; set; }

    [JsonPropertyName("bbox")]
    public double[]? BoundingBox { get; set; }

    [JsonPropertyName("ændret")]
    public DateTime? Changed { get; set; }

    [JsonPropertyName("geo_ændret")]
    public DateTime? GeoChanged { get; set; }

    [JsonPropertyName("geo_version")]
    public int? GeoVersion { get; set; }

    public override string ToString()
    {
        var center = VisualCenter != null && VisualCenter.Length >= 2
            ? $"[{VisualCenter[0]:F4}, {VisualCenter[1]:F4}]"
            : "N/A";

        return $"Parish: {Name} (Code: {Code}, Center: {center})";
    }
}
