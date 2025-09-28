using System.Collections.Generic;

public class CountryDatabase
{
public static readonly Dictionary<string, string> countryCodes = new Dictionary<string, string>()
{
    { "AU", "Australia" },
    { "GR", "Greece" },
    { "TH", "Thailand" },
    { "HU", "Hungary" },
    { "NZ", "New Zealand" },
    { "US", "United States of America" },
    { "EU", "Europe" },
    { "UK", "United Kingdom" },
    { "CN", "China" },
    { "SC", "Scotland" },
    { "AF", "Afghanistan" },
    { "AT", "Austria" },
    { "BE", "Belgium" },
    { "BO", "Bolivia" },
    { "BR", "Brazil" },
    { "BN", "Brunei" },
    { "BG", "Bulgaria" },
    { "KH", "Cambodia" },
    { "TD", "Chad" },
    { "CL", "Chile" },
    { "HR", "Croatia" },
    { "CY", "Cyprus" },
    { "DK", "Denmark" },
    { "EE", "Estonia" },
    { "FI", "Finland" },
    { "FR", "France" },
    { "GE", "Georgia" },
    { "DE", "Germany" },
    { "IS", "Iceland" },
    { "IN", "India" },
    { "ID", "Indonesia" },
    { "IE", "Ireland" },
    { "IT", "Italy" },
    { "JP", "Japan" },
    { "JO", "Jordan" },
    { "KR", "South Korea" },
    { "UR", "Ukraine" },
    { "SZ", "Switzerland" },
    { "KW", "Kuwait" },
    { "LA", "Laos" },
    { "SP", "Spain" },
    { "CO", "Colombia" },
    { "CB", "Cuba" },
    { "EG", "Egypt" },
    { "JA", "Jamaica" },
    { "MX", "Mexico" },
    { "PU", "Peru" },
    { "KZ", "Kazakhstan" },
    { "ET", "East Timor" },

    // Potential Flag Pack
    { "AQ", "Antarctica" },
    { "RB", "Rainbow" },
    { "UN", "Unicef" },
    { "NK", "North Korea" },
    { "LG", "Lgbt"}



};

    public static string GetCountryName(string code)
    {
        return countryCodes.TryGetValue(code, out var name) ? name : "Australia";
    }
}
