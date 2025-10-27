using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using CityBuilder.Application.Data;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public sealed class SaveGameUseCase
    {
        private readonly ICityRepository _cityRepository;
        private readonly IEconomyRepository _economyRepository;
        private readonly ISaveStorage _saveStorage;
        private readonly IPublisher<GameSavedEvent> _savedPublisher;

        public SaveGameUseCase(
            ICityRepository cityRepository,
            IEconomyRepository economyRepository,
            ISaveStorage saveStorage,
            IPublisher<GameSavedEvent> savedPublisher)
        {
            _cityRepository = cityRepository;
            _economyRepository = economyRepository;
            _saveStorage = saveStorage;
            _savedPublisher = savedPublisher;
        }

        public async UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            var city = _cityRepository.City;
            var economy = _economyRepository.Economy;
            var data = new CitySaveData
            {
                Width = city.Width,
                Height = city.Height,
                Gold = economy.Gold,
                Buildings = city.Buildings
                    .Select(b => new BuildingSaveData
                    {
                        Id = b.Id,
                        TypeId = b.TypeId,
                        Level = b.Level,
                        X = b.Position.X,
                        Y = b.Position.Y,
                        Rotation = b.Rotation
                    })
                    .ToArray()
            };

            await _saveStorage.SaveAsync(data, cancellationToken);
            _savedPublisher.Publish(new GameSavedEvent(_saveStorage.LastSaveLocation));
        }
    }
}
