namespace B2BStore.TechnicalSettings.Api.Services;

/// <summary>
/// Bitwise helpers. Several DealerSettings columns (ParameterMenu, ParameterProductDetail ...)
/// store many on/off options in one integer: every option owns one bit (1, 2, 4, 8 ...).
/// </summary>
public static class BitMask
{
    /// <summary>
    /// Applies a change without touching any bit that was not explicitly changed:
    /// <c>result = (current | set) &amp; ~clear</c>.
    /// Bits that the screen does not know about (e.g. options removed from the UI)
    /// therefore survive every save.
    /// </summary>
    public static long Apply(long current, long set, long clear)
    {
        if ((set & clear) != 0)
            throw new ArgumentException("Aynı bit hem açılıp hem kapatılamaz.");
        return (current | set) & ~clear;
    }

    /// <summary>True when every bit of <paramref name="bits"/> is inside <paramref name="allowed"/>.</summary>
    public static bool IsSubsetOf(long bits, long allowed) => (bits & ~allowed) == 0;

    /// <summary>Largest value a SQL integer column can store.</summary>
    public static long MaxFor(string sqlType) => sqlType switch
    {
        "tinyint" => byte.MaxValue,
        "smallint" => short.MaxValue,
        "int" => int.MaxValue,
        "bigint" => long.MaxValue,
        _ => long.MaxValue // nvarchar columns used as bitmask (UseFiltering)
    };

    /// <summary>Lists the bit values that are set, e.g. 13 → [1, 4, 8].</summary>
    public static IEnumerable<long> Bits(long value)
    {
        for (var i = 0; i < 63; i++)
        {
            var bit = 1L << i;
            if ((value & bit) != 0) yield return bit;
        }
    }
}
