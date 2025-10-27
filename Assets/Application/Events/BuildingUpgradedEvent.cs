using CityBuilder.Domain;

namespace CityBuilder.Application.Events
{
    public readonly struct BuildingUpgradedEvent
    {
        public BuildingUpgradedEvent(Building building, BuildingLevelDefinition newLevel)
        {
            Building = building;
            NewLevel = newLevel;
        }

        public Building Building { get; }

        public BuildingLevelDefinition NewLevel { get; }
    }
}
