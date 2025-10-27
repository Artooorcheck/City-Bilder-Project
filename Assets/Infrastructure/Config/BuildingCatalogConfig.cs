using System;
using System.Collections.Generic;
using System.Linq;
using CityBuilder.Domain;
using UnityEngine;
using TriInspector;

namespace CityBuilder.Infrastructure.Config
{
    [CreateAssetMenu(menuName = "City Builder/Building Catalog", fileName = "BuildingCatalog")]
    public sealed class BuildingCatalogConfig : ScriptableObject
    {
        [SerializeField]
        private List<BuildingTypeConfig> types = new();

        public IReadOnlyList<BuildingTypeConfig> Types => types;

        public IEnumerable<BuildingTypeDefinition> BuildDefinitions() => types.Select(t => t.ToDefinition());
    }

    [Serializable]
    public sealed class BuildingTypeConfig
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = "Building";

        [SerializeField]
        private List<BuildingLevelConfig> levels = new();

        public string Id => id;

        public string DisplayName => displayName;

        public IEnumerable<BuildingLevelConfig> Levels => levels;

        public BuildingTypeDefinition ToDefinition()
        {
            if (levels.Count == 0)
            {
                throw new DomainException($"Building type '{id}' must contain at least one level.");
            }

            return new BuildingTypeDefinition(id, displayName, levels.Select(l => l.ToDefinition()));
        }
    }

    [Serializable]
    public sealed class BuildingLevelConfig
    {
        [SerializeField]
        [Min(1)]
        private int level = 1;

        [SerializeField]
        [Min(0)]
        private int cost = 100;

        [SerializeField]
        private int income = 1;

        public int Level => level;

        public int Cost => cost;

        public int Income => income;

        public BuildingLevelDefinition ToDefinition() => new(level, cost, income);
    }
}
