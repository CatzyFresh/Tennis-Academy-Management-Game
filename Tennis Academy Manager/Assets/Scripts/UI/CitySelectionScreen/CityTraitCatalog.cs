using TennisAcademyManager.Systems.City;

namespace TennisAcademyManager.UI.CitySelection
{
    public readonly struct CityTraits
    {
        public readonly string traitA;
        public readonly string traitB;

        public CityTraits(string a, string b)
        {
            traitA = a;
            traitB = b;
        }
    }

    public static class CityTraitCatalog
    {
        public static CityTraits GetTraitsFor(CityType type)
        {
            return type switch
            {
                CityType.Tier1Metro => new CityTraits(
                    "High Demand • Faster enrollments",
                    "High Pressure • Higher costs & competition"
                ),

                CityType.Tier2City => new CityTraits(
                    "Balanced Demand • Steady growth",
                    "Balanced Costs • Stable operations"
                ),

                CityType.Tier3Town => new CityTraits(
                    "Low Costs • Cheaper to run",
                    "Limited Talent • Smaller player pool"
                ),

                _ => new CityTraits("—", "—")
            };
        }
    }
}
