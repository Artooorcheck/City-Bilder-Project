using TriInspector;
using UnityEngine;

namespace CityBuilder.Infrastructure.Config
{
    [CreateAssetMenu(menuName = "City Builder/Gameplay Settings", fileName = "GameplaySettings")]
    public sealed class GameplaySettings : ScriptableObject
    {
        [Header("Grid")]
        [Min(4)]
        [SerializeField]
        private int gridWidth = 32;

        [Min(4)]
        [SerializeField]
        private int gridHeight = 32;

        [Header("Economy")]
        [SerializeField]
        private int startingGold = 500;

        [SerializeField]
        [Min(0.5f)]
        private float incomeTickSeconds = 5f;

        [Header("Saving")]
        [SerializeField]
        [Min(5f)]
        private float autoSaveSeconds = 30f;

        public int GridWidth => gridWidth;

        public int GridHeight => gridHeight;

        public int StartingGold => startingGold;

        public float IncomeTickSeconds => incomeTickSeconds;

        public float AutoSaveSeconds => autoSaveSeconds;
    }
}
