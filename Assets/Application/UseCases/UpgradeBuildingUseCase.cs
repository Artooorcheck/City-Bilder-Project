using System;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class UpgradeBuildingUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IEconomyRepository _economyRepository;
        private readonly IBuildingCatalog _catalog;
        private readonly IPublisher<BuildingUpgradedEvent> _upgradedPublisher;
        private readonly IPublisher<NotEnoughGoldEvent> _notEnoughPublisher;
        private readonly IPublisher<EconomyChangedEvent> _economyChangedPublisher;

        public UpgradeBuildingUseCase(
            ICityRepository cityRepository,
            IEconomyRepository economyRepository,
            IBuildingCatalog catalog,
            IPublisher<BuildingUpgradedEvent> upgradedPublisher,
            IPublisher<NotEnoughGoldEvent> notEnoughPublisher,
            IPublisher<EconomyChangedEvent> economyChangedPublisher)
        {
            _cityRepository = cityRepository;
            _economyRepository = economyRepository;
            _catalog = catalog;
            _upgradedPublisher = upgradedPublisher;
            _notEnoughPublisher = notEnoughPublisher;
            _economyChangedPublisher = economyChangedPublisher;
        }

        public bool Execute(Guid buildingId)
        {
            var city = _cityRepository.City;
            var building = city.GetBuilding(buildingId);
            var definition = _catalog.GetById(building.TypeId);

            if (!definition.TryGetNextLevel(building.Level, out var nextLevel) || nextLevel == null)
            {
                return false;
            }

            var economy = _economyRepository.Economy;
            if (!economy.CanAfford(nextLevel.Cost))
            {
                _notEnoughPublisher.Publish(new NotEnoughGoldEvent(nextLevel.Cost, economy.Gold));
                return false;
            }

            economy.Spend(nextLevel.Cost);
            building.Upgrade();
            _upgradedPublisher.Publish(new BuildingUpgradedEvent(building, nextLevel));
            _economyChangedPublisher.Publish(new EconomyChangedEvent(economy.Gold));
            return true;
        }
    }
}
