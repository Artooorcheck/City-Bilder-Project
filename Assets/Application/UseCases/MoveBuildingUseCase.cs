using System;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class MoveBuildingUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IPublisher<BuildingMovedEvent> _movedPublisher;

        public MoveBuildingUseCase(ICityRepository cityRepository, IPublisher<BuildingMovedEvent> movedPublisher)
        {
            _cityRepository = cityRepository;
            _movedPublisher = movedPublisher;
        }

        public void Execute(Guid buildingId, GridPosition newPosition, int rotation)
        {
            var city = _cityRepository.City;
            city.MoveBuilding(buildingId, newPosition, rotation);
            var building = city.GetBuilding(buildingId);
            _movedPublisher.Publish(new BuildingMovedEvent(building));
        }
    }
}
