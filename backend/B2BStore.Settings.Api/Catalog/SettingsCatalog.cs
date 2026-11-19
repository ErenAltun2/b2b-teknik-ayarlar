using System.Text.Json;
using System.Text.Json.Serialization;

namespace B2BStore.Settings.Api.Catalog;

/// <summary>
/// settings-catalog.json dosyasını okur; her ayar, kategori ve seçenek için bir C# nesnesi üretir.
/// program calısırken sadece 1 kez calısır addsıngleton seklınde program cs e ekledım
/// </summary>
public sealed class SettingsCatalog
{
    public int Version { get; init; }
    public string Source { get; init; } = "";
    public List<SettingGroup> Groups { get; init; } = [];
    public List<SettingDefinition> Settings { get; init; } = [];
    public List<LegacyOnlyControl> NotInDatabase { get; init; } = [];

    [JsonIgnore]
    public IReadOnlyDictionary<string, SettingDefinition> ByKey { get; private set; } =
        new Dictionary<string, SettingDefinition>();

    public static SettingsCatalog Load(string path)
    {
        var json = File.ReadAllText(path);
        var catalog = JsonSerializer.Deserialize<SettingsCatalog>(json, JsonOptions)
                      ?? throw new InvalidOperationException("Ayar kataloğu okunamadı.");

        catalog.ByKey = catalog.Settings.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var s in catalog.Settings) s.Validate();
        return catalog;
    }

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}

public sealed record SettingGroup(string Key, string Title, string Description);

public sealed record LegacyOnlyControl(string LegacyControlId, string Label);

public sealed class SettingOption
{
    public string Value { get; init; } = "";
    public string Label { get; init; } = "";
    /// <summary>Option exists on the legacy screen but its bit does not fit the column type.</summary>
    public bool Unsupported { get; init; }
}

public sealed class SettingDependency
{
    public string Key { get; init; } = "";
    public List<string>? In { get; init; }
    public List<string>? NotIn { get; init; }
    public string Message { get; init; } = "";
}

public sealed class SettingDefinition
{
    public string Key { get; init; } = "";
    public string Group { get; init; } = "";
    public string Label { get; init; } = "";
    /// <summary>switch | select | bitmask | text | textarea | number | color | image</summary>
    public string Control { get; init; } = "";
    public string SqlType { get; init; } = "";
    public int MaxLength { get; init; }
    public bool Bitwise { get; init; }
    public List<SettingOption>? Options { get; init; }
    public string? Description { get; init; }
    public string? LegacyControlId { get; init; }
    public SettingDependency? DependsOn { get; init; }
    public string? Placeholder { get; init; }
    public decimal? Min { get; init; }
    /// <summary>Sum of all supported option bits (as string: may exceed JS safe integers).</summary>
    public string? KnownMask { get; init; }

    [JsonIgnore] public long KnownMaskValue { get; private set; }

    internal void Validate()
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(Key, "^[A-Za-z0-9_]+$"))
            throw new InvalidOperationException($"Geçersiz kolon adı: {Key}");
        if (Bitwise) KnownMaskValue = long.Parse(KnownMask ?? "0");
    }
}
