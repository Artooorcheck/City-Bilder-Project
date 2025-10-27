using System.Threading;
using Cysharp.Threading.Tasks;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class LoadGameUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IEconomyRepository _economyRepository;
        private readonly ISaveStorage _saveStorage;
        private readonly IPublisher<GameLoadedEvent> _loadedPublisher;
        private readonly IPublisher<EconomyChangedEvent> _economyChangedPublisher;

        public LoadGameUseCase(
            ICityRepository cityRepository,
            IEconomyRepository economyRepository,
            ISaveStorage saveStorage,
            IPublisher<GameLoadedEvent> loadedPublisher,
            IPublisher<EconomyChangedEvent> economyChangedPublisher)
        {
            _cityRepository = cityRepository;
            _economyRepository = economyRepository;
            _saveStorage = saveStorage;
            _loadedPublisher = loadedPublisher;
            _economyChangedPublisher = economyChangedPublisher;
        }

        public async UniTask<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            var data = await _saveStorage.LoadAsync(cancellationToken);
            if (data == null)
            {
                return false;
            }

            var city = new CityState(data.Width, data.Height);
            foreach (var buildingData in data.Buildings)
            {
                var building = city.PlaceBuilding(buildingData.Id, buildingData.TypeId, new GridPosition(buildingData.X, buildingData.Y), buildingData.Rotation);
                while (building.Level < buildingData.Level)
                {
                    building.Upgrade();
                }
            }

            _cityRepository.Replace(city);
            var economy = new EconomyState(data.Gold);
            _economyRepository.Replace(economy);
            _economyChangedPublisher.Publish(new EconomyChangedEvent(economy.Gold));
            _loadedPublisher.Publish(new GameLoadedEvent());
            return true;
        }
    }
}
