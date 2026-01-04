using System;

namespace TennisAcademyManager.Systems
{
    [Serializable]
    public class PricingProfile
    {
        public int BasePrice;
        public int RecommendedPrice;
        public int CurrentPrice;

        // Limits (± % from recommended)
        public float MaxIncreasePct = 0.20f;
        public float MaxDecreasePct = 0.20f;

        public PricingProfile(int basePrice)
        {
            BasePrice = basePrice;
            RecommendedPrice = basePrice;
            CurrentPrice = basePrice;
        }

        public bool CanSetPrice(int newPrice)
        {
            int min = (int)(RecommendedPrice * (1f - MaxDecreasePct));
            int max = (int)(RecommendedPrice * (1f + MaxIncreasePct));
            return newPrice >= min && newPrice <= max;
        }
    }
}
