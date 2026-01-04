using System.Collections.Generic;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    [CreateAssetMenu(menuName = "TAM/Courts/Court Database", fileName = "CourtDatabase")]
    public class CourtDatabase : ScriptableObject
    {
        public List<CourtDefinition> Courts = new();
    }
}
