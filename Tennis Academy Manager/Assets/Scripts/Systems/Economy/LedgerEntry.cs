using System;

namespace TennisAcademyManager.Systems
{
    [Serializable]
    public class LedgerEntry
    {
        public LedgerEntryType Type;
        public LedgerCategory Category;
        public int Amount;
        public string Description;

        public LedgerEntry(LedgerEntryType type, LedgerCategory category, int amount, string description)
        {
            Type = type;
            Category = category;
            Amount = amount;
            Description = description;
        }
    }
}
