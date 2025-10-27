using System;
using CityBuilder.Domain;

namespace CityBuilder.Application.Events
{
    public readonly struct BuildingMovedEvent
    {
        public BuildingMovedEvent(Building building)
        {
            Building = building;
        }

        public Building Building { get; }
    }
}
