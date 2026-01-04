using System;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.Systems.Players
{
    public sealed class PlayerGenerator
    {
        private readonly System.Random rng;

        public PlayerGenerator(System.Random rng)
        {
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public PlayerInstance Generate(PlayerSegment segment)
        {
            string id = Guid.NewGuid().ToString("N");
            string name = GenerateName();
            int age = RollAge(segment);

            var traits = RollTraits(segment);
            var stats = RollStats(segment);

            var p = new PlayerInstance(id, name, age, segment, traits, stats);
            // baseline health (can later come from config/archetypes)
            p.InitializeHealth(startingFatigue: 10f, startingRisk: 5f);
            return p;
        }

        private int RollAge(PlayerSegment s)
        {
            return s switch
            {
                PlayerSegment.HobbyKids => rng.Next(6, 14),
                PlayerSegment.CompetitiveJuniors => rng.Next(10, 18),
                PlayerSegment.EliteProspects => rng.Next(12, 19),
                PlayerSegment.Adults => rng.Next(18, 45),
                _ => rng.Next(10, 18)
            };
        }

        private PlayerTraitFlags RollTraits(PlayerSegment s)
        {
            PlayerTraitFlags t = PlayerTraitFlags.None;

            // simple probabilities (tune later)
            if (rng.NextDouble() < 0.10) t |= PlayerTraitFlags.HighEndurance;
            if (rng.NextDouble() < 0.10) t |= PlayerTraitFlags.Durable;
            if (rng.NextDouble() < 0.10) t |= PlayerTraitFlags.InjuryProne;
            if (rng.NextDouble() < 0.10) t |= PlayerTraitFlags.Overtrainer;

            // elite prospects slightly more endurance
            if (s == PlayerSegment.EliteProspects && rng.NextDouble() < 0.15)
                t |= PlayerTraitFlags.HighEndurance;

            return t;
        }

        private PlayerStats RollStats(PlayerSegment s)
        {
            // segment-driven ranges (tune later)
            (int min, int max) = s switch
            {
                PlayerSegment.HobbyKids => (15, 45),
                PlayerSegment.CompetitiveJuniors => (25, 60),
                PlayerSegment.EliteProspects => (40, 75),
                PlayerSegment.Adults => (20, 55),
                _ => (20, 55)
            };

            return new PlayerStats
            {
                serve = rng.Next(min, max + 1),
                forehand = rng.Next(min, max + 1),
                backhand = rng.Next(min, max + 1),
                movement = rng.Next(min, max + 1),
                mental = rng.Next(min, max + 1),
            };
        }

        private string GenerateName()
        {
            string[] first = { "Arjun", "Karthik", "Vishal", "Sai", "Rahul", "Aarya", "Meera", "Nisha", "Ananya", "Diya" };
            string[] last = { "Kumar", "Iyer", "Sharma", "Rao", "Menon", "Reddy", "Nair", "Singh", "Das", "Patel" };
            return $"{first[rng.Next(first.Length)]} {last[rng.Next(last.Length)]}";
        }
    }
}
