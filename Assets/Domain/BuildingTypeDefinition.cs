using System.Collections.Generic;
using System.Linq;

namespace CityBuilder.Domain
{
    public sealed class BuildingTypeDefinition
    {
        private readonly Dictionary<int, BuildingLevelDefinition> _levels;

        public BuildingTypeDefinition(string id, string displayName, IEnumerable<BuildingLevelDefinition> levels)
        {
            Id = id;
            DisplayName = displayName;
            _levels = levels.ToDictionary(level => level.Level);
            if (_levels.Count == 0)
            {
                throw new DomainException($"Building type '{id}' must define at least one level.");
            }

            MaxLevel = _levels.Keys.Max();
        }

        public string Id { get; }

        public string DisplayName { get; }

        public int MaxLevel { get; }

        public BuildingLevelDefinition GetLevel(int level)
        {
            if (!_levels.TryGetValue(level, out var definition))
            {
                throw new DomainException($"Building type '{Id}' does not contain level {level}.");
            }

            return definition;
        }

        public BuildingLevelDefinition GetFirstLevel() => GetLevel(1);

        public bool TryGetNextLevel(int currentLevel, out BuildingLevelDefinition? definition)
        {
            var nextLevel = currentLevel + 1;
            if (nextLevel > MaxLevel)
            {
                definition = null;
                return false;
            }

            definition = GetLevel(nextLevel);
            return true;
        }
    }
}
