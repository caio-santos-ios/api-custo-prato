namespace api_custo_prato.src.Calculations
{
    /// <summary>Conversão de unidades. Base: g (massa), ml (volume), un (unidade).</summary>
    public static class UnitConverter
    {
        public const string Mass = "mass";
        public const string Volume = "volume";
        public const string Unit = "unit";

        public static readonly string[] SupportedUnits = ["kg", "g", "L", "ml", "un"];

        /// <summary>Normaliza a escrita da unidade (ex.: "KG" → "kg", "l" → "L", "unid" → "un").</summary>
        public static string Normalize(string? unit)
        {
            string u = (unit ?? "").Trim().ToLowerInvariant();
            return u switch
            {
                "kg" or "quilo" or "kilo" => "kg",
                "g" or "gr" or "grama" or "gramas" => "g",
                "l" or "lt" or "litro" or "litros" => "L",
                "ml" or "mililitro" or "mililitros" => "ml",
                "un" or "und" or "unid" or "unidade" or "unidades" or "u" => "un",
                _ => ""
            };
        }

        public static bool IsValid(string? unit) => Normalize(unit) != "";

        public static string FamilyOf(string? unit) => Normalize(unit) switch
        {
            "kg" or "g" => Mass,
            "L" or "ml" => Volume,
            "un" => Unit,
            _ => ""
        };

        public static string BaseUnitOf(string? unit) => FamilyOf(unit) switch
        {
            Mass => "g",
            Volume => "ml",
            Unit => "un",
            _ => ""
        };

        /// <summary>Quantos "base" existem em 1 unidade (kg → 1000 g, L → 1000 ml).</summary>
        public static decimal FactorToBase(string? unit) => Normalize(unit) switch
        {
            "kg" => 1000m,
            "L" => 1000m,
            "g" or "ml" or "un" => 1m,
            _ => 0m
        };

        public static bool AreCompatible(string? a, string? b)
        {
            string fa = FamilyOf(a);
            return fa != "" && fa == FamilyOf(b);
        }

        public static decimal ToBase(decimal quantity, string? unit) => quantity * FactorToBase(unit);

        /// <summary>Converte entre unidades da mesma família (ex.: 0,25 kg → 250 g).</summary>
        public static decimal Convert(decimal quantity, string? from, string? to)
        {
            if (!AreCompatible(from, to))
                throw new ArgumentException($"Não é possível converter {from} para {to}.");
            return ToBase(quantity, from) / FactorToBase(to);
        }
    }
}
