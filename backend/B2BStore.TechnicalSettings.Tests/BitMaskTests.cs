using B2BStore.TechnicalSettings.Api.Catalog;
using B2BStore.TechnicalSettings.Api.Services;

namespace B2BStore.TechnicalSettings.Tests;

public class BitMaskTests
{
    [Fact]
    public void Apply_changes_only_the_requested_bits()
    {
        // 1 + 8 + 32 selected; user turns on 4 and turns off 8
        var result = BitMask.Apply(current: 41, set: 4, clear: 8);
        Assert.Equal(1 + 4 + 32, result);
    }

    [Fact]
    public void Apply_keeps_bits_unknown_to_the_screen()
    {
        // bit 62 is not on the screen any more but must survive a save
        const long unknown = 1L << 62;
        var result = BitMask.Apply(current: unknown | 2, set: 1, clear: 2);
        Assert.Equal(unknown | 1, result);
    }

    [Fact]
    public void Apply_rejects_setting_and_clearing_the_same_bit()
        => Assert.Throws<ArgumentException>(() => BitMask.Apply(0, 4, 4));

    [Fact]
    public void Bits_lists_set_bits()
        => Assert.Equal(new long[] { 1, 4, 8 }, BitMask.Bits(13).ToArray());

    [Theory]
    [InlineData("smallint", 32767)]
    [InlineData("tinyint", 255)]
    public void MaxFor_matches_sql_types(string sqlType, long max)
        => Assert.Equal(max, BitMask.MaxFor(sqlType));

    [Fact]
    public void Catalog_is_valid_and_every_bitwise_option_is_a_single_bit()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Catalog", "settings-catalog.json");
        var catalog = SettingsCatalog.Load(path);

        Assert.NotEmpty(catalog.Settings);
        foreach (var s in catalog.Settings.Where(s => s.Bitwise))
        foreach (var o in s.Options!)
        {
            var v = long.Parse(o.Value);
            Assert.True(v > 0 && (v & (v - 1)) == 0, $"{s.Key}: {o.Value} tek bit değil");
            if (!o.Unsupported) Assert.True(v <= BitMask.MaxFor(s.SqlType), $"{s.Key}: {o.Value} kolona sığmıyor");
        }
    }

    [Fact]
    public void ToWire_formats_values_for_json()
    {
        Assert.Equal("1", SettingsService.ToWire(true));
        Assert.Equal("4611686018427387904", SettingsService.ToWire(1L << 62));
        Assert.Equal("50", SettingsService.ToWire(50.00m));
        Assert.Null(SettingsService.ToWire(null));
    }
}
