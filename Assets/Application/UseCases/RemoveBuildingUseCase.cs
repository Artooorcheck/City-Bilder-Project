using System;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class RemoveBuildingUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IPublisher<BuildingRemovedEvent> _removedPublisher;

        public RemoveBuildingUseCase(ICityRepository cityRepository, IPublisher<BuildingRemovedEvent> removedPublisher)
        {
            _cityRepository = cityRepository;
            _removedPublisher = removedPublisher;
        }

        public void Execute(Guid buildingId)
        {
            var city = _cityRepository.City;
            var building = city.GetBuilding(buildingId);
            city.RemoveBuilding(buildingId);
            _removedPublisher.Publish(new BuildingRemovedEvent(buildingId, building.TypeId));
        }
    }
}
