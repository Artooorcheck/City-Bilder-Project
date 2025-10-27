using System;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class PlaceBuildingUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IEconomyRepository _economyRepository;
        private readonly IBuildingCatalog _catalog;
        private readonly IPublisher<BuildingPlacedEvent> _placedPublisher;
        private readonly IPublisher<NotEnoughGoldEvent> _notEnoughPublisher;
        private readonly IPublisher<EconomyChangedEvent> _economyChangedPublisher;

        public PlaceBuildingUseCase(
            ICityRepository cityRepository,
            IEconomyRepository economyRepository,
            IBuildingCatalog catalog,
            IPublisher<BuildingPlacedEvent> placedPublisher,
            IPublisher<NotEnoughGoldEvent> notEnoughPublisher,
            IPublisher<EconomyChangedEvent> economyChangedPublisher)
        {
            _cityRepository = cityRepository;
            _economyRepository = economyRepository;
            _catalog = catalog;
            _placedPublisher = placedPublisher;
            _notEnoughPublisher = notEnoughPublisher;
            _economyChangedPublisher = economyChangedPublisher;
        }

        public bool Execute(string buildingTypeId, GridPosition position, int rotation, out Building building)
        {
            var definition = _catalog.GetById(buildingTypeId);
            var level = definition.GetFirstLevel();
            var economy = _economyRepository.Economy;

            if (!economy.CanAfford(level.Cost))
            {
                _notEnoughPublisher.Publish(new NotEnoughGoldEvent(level.Cost, economy.Gold));
                building = null;
                return false;
            }

            economy.Spend(level.Cost);
            var created = _cityRepository.City.PlaceBuilding(Guid.NewGuid(), definition.Id, position, rotation);
            building = created;
            _placedPublisher.Publish(new BuildingPlacedEvent(created, definition));
            _economyChangedPublisher.Publish(new EconomyChangedEvent(economy.Gold));
            return true;
        }
    }
}
