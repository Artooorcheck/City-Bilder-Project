using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class CollectIncomeUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IEconomyRepository _economyRepository;
        private readonly IBuildingCatalog _catalog;
        private readonly IPublisher<EconomyChangedEvent> _economyChangedPublisher;

        public CollectIncomeUseCase(
            ICityRepository cityRepository,
            IEconomyRepository economyRepository,
            IBuildingCatalog catalog,
            IPublisher<EconomyChangedEvent> economyChangedPublisher)
        {
            _cityRepository = cityRepository;
            _economyRepository = economyRepository;
            _catalog = catalog;
            _economyChangedPublisher = economyChangedPublisher;
        }

        public int Execute()
        {
            var totalIncome = 0;
            foreach (var building in _cityRepository.City.Buildings)
            {
                var definition = _catalog.GetById(building.TypeId);
                var level = definition.GetLevel(building.Level);
                totalIncome += level.Income;
            }

            if (totalIncome <= 0)
            {
                return 0;
            }

            var economy = _economyRepository.Economy;
            economy.Earn(totalIncome);
            _economyChangedPublisher.Publish(new EconomyChangedEvent(economy.Gold));
            return totalIncome;
        }
    }
}
