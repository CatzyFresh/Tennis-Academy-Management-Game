using System;
using System.Collections.Generic;
using UnityEngine;

namespace TennisAcademyManager.Systems.Tournaments
{
    [CreateAssetMenu(menuName = "TAM/Tournaments/Points Table", fileName = "PointsTable")]
    public class PointsTableSO : ScriptableObject
    {
        [Serializable]
        public struct RoundPoints
        {
            public Round round;
            public int points;
        }

        public TournamentLevelSO level;

        [Header("Qualifying points (if you want them)")]
        public List<RoundPoints> qualifyingPoints = new();

        [Header("Main draw points")]
        public List<RoundPoints> mainPoints = new();

        public int GetPoints(DrawType draw, Round round)
        {
            var list = draw == DrawType.Qualifying ? qualifyingPoints : mainPoints;

            for (int i = 0; i < list.Count; i++)
                if (list[i].round == round) return list[i].points;

            return 0;
        }
    }
}
