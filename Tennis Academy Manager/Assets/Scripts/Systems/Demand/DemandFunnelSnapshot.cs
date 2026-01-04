using System;

namespace TennisAcademyManager.Systems
{
    [Serializable]
    public class DemandFunnelSnapshot
    {
        public int Inquiries;
        public int Trials;
        public int Enrolled;
        public int Active; // retained, paying members

        public DemandFunnelSnapshot(int inquiries = 0, int trials = 0, int enrolled = 0, int active = 0)
        {
            Inquiries = inquiries;
            Trials = trials;
            Enrolled = enrolled;
            Active = active;
        }
    }
}
