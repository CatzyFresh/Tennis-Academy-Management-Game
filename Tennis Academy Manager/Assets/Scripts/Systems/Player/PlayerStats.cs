using System;
using UnityEngine;

namespace TennisAcademyManager.Systems.Players
{
    [Serializable]
    public struct PlayerStats
    {
        [Range(0, 100)] public int serve;
        [Range(0, 100)] public int forehand;
        [Range(0, 100)] public int backhand;
        [Range(0, 100)] public int movement;
        [Range(0, 100)] public int mental;

        public int Overall =>
            Mathf.RoundToInt((serve + forehand + backhand + movement + mental) / 5f);
    }
}
