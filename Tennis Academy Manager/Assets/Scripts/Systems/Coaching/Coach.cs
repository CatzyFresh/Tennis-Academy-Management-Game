using System;
using System.Collections.Generic;

namespace TennisAcademyManager.Systems
{
    [Serializable]
    public class Coach
    {
        public CoachRole Role;
        public int MonthlySalary;
        public int Capacity; // max active players handled
        public int CurrentLoad;
        public List<CoachCertification> Certifications = new();
        public CertificationDefinition ActiveCertification;
        public int CertificationMonthsRemaining;

        public bool IsInCertification => ActiveCertification != null;
        public Coach(CoachRole role, int salary, int capacity)
        {
            Role = role;
            MonthlySalary = salary;
            Capacity = capacity;
            CurrentLoad = 0;
        }

        public bool IsOverloaded => CurrentLoad > Capacity;
    }
}
