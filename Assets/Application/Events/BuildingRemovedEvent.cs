using System;
using CityBuilder.Domain;

namespace CityBuilder.Application.Events
{
    public readonly struct BuildingRemovedEvent
    {
        public BuildingRemovedEvent(Guid buildingId, string buildingTypeId)
        {
            BuildingId = buildingId;
            BuildingTypeId = buildingTypeId;
        }

        public Guid BuildingId { get; }

        public string BuildingTypeId { get; }
    }
}
