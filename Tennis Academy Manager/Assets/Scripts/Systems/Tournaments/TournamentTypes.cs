namespace TennisAcademyManager.Systems.Tournaments
{
    public enum TennisCircuit { AITA_Juniors, ITF_Juniors, Pro }

    public enum JuniorAgeGroup { U10, U12, U14, U16, U18 }

    public enum AitaSeries { Talent, Championship, Super, NationalSeries, Nationals }
    public enum ItfJuniorGrade { J5, J4, J3, J2, J1 }
    public enum ProLevel { NationalPro, M15, M25, W15, W25, Challenger, ATP_WTA }

    public enum DrawType { Qualifying, Main }

    public enum Round
    {
        // Qualifying
        Qual_R32, Qual_R16, Qual_QF, Qual_SF, Qual_F,

        // Main
        R128, R64, R32, R16, QF, SF, F, W
    }
}
