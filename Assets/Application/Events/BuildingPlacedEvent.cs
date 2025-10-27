using System;
using CityBuilder.Domain;

namespace CityBuilder.Application.Events
{
    public readonly struct BuildingPlacedEvent
    {
        public BuildingPlacedEvent(Building building, BuildingTypeDefinition definition)
        {
            Building = building;
            Definition = definition;
        }

        public Building Building { get; }

        public BuildingTypeDefinition Definition { get; }
    }
}
