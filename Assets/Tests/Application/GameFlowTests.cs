using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CityBuilder.Application.Data;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Application.UseCases;
using CityBuilder.Domain;
using CityBuilder.Infrastructure.Repositories;
using MessagePipe;
using NUnit.Framework;

namespace CityBuilder.Tests.Application
{
    public sealed class GameFlowTests
    {
        [Test]
        public async Task PlaceSaveLoad_RestoresState()
        {
            var catalog = new TestCatalog();
            var cityRepository = new CityRepository(new CityState(8, 8));
            var economyRepository = new EconomyRepository(new EconomyState(500));
            var storage = new InMemorySaveStorage();
            var publisher = new DummyPublisher<BuildingPlacedEvent>();
            var economyPublisher = new DummyPublisher<EconomyChangedEvent>();
            var notEnoughPublisher = new DummyPublisher<NotEnoughGoldEvent>();
            var savedPublisher = new DummyPublisher<GameSavedEvent>();
            var loadedPublisher = new DummyPublisher<GameLoadedEvent>();

            var placeUseCase = new PlaceBuildingUseCase(cityRepository, economyRepository, catalog, publisher, notEnoughPublisher, economyPublisher);
            var saveUseCase = new SaveGameUseCase(cityRepository, economyRepository, storage, savedPublisher);
            var loadUseCase = new LoadGameUseCase(cityRepository, economyRepository, storage, loadedPublisher, economyPublisher);

            Assert.IsTrue(placeUseCase.Execute("House", new GridPosition(2, 3), 0, out var building));
            Assert.NotNull(building);

            await saveUseCase.ExecuteAsync(CancellationToken.None);

            cityRepository.Replace(new CityState(8, 8));
            economyRepository.Replace(new EconomyState(0));

            var loaded = await loadUseCase.ExecuteAsync(CancellationToken.None);

            Assert.IsTrue(loaded);
            var restored = cityRepository.City.GetBuilding(building!.Id);
            Assert.AreEqual(new GridPosition(2, 3), restored.Position);
            Assert.AreEqual(1, restored.Level);
            Assert.AreEqual(400, economyRepository.Economy.Gold);
        }

        private sealed class TestCatalog : IBuildingCatalog
        {
            private readonly Dictionary<string, BuildingTypeDefinition> _definitions;

            public TestCatalog()
            {
                _definitions = new Dictionary<string, BuildingTypeDefinition>
                {
                    {
                        "House",
                        new BuildingTypeDefinition("House", "House", new[]
                        {
                            new BuildingLevelDefinition(1, 100, 2),
                            new BuildingLevelDefinition(2, 150, 4)
                        })
                    },
                    {
                        "Farm",
                        new BuildingTypeDefinition("Farm", "Farm", new[]
                        {
                            new BuildingLevelDefinition(1, 120, 5)
                        })
                    }
                };
            }

            public IEnumerable<BuildingTypeDefinition> All => _definitions.Values;

            public BuildingTypeDefinition GetById(string id) => _definitions[id];
        }

        private sealed class DummyPublisher<T> : IPublisher<T>
        {
            public void Publish(T message)
            {
            }
        }

        private sealed class InMemorySaveStorage : ISaveStorage
        {
            private CitySaveData _data;

            public Task SaveAsync(CitySaveData data, CancellationToken cancellationToken)
            {
                _data = new CitySaveData
                {
                    Width = data.Width,
                    Height = data.Height,
                    Gold = data.Gold,
                    Buildings = data.Buildings == null ? Array.Empty<BuildingSaveData>() : System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(data.Buildings, b => new BuildingSaveData
                    {
                        Id = b.Id,
                        TypeId = b.TypeId,
                        Level = b.Level,
                        X = b.X,
                        Y = b.Y,
                        Rotation = b.Rotation
                    }))
                };
                return Task.CompletedTask;
            }

            public Task<CitySaveData> LoadAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_data);
            }

            public string LastSaveLocation => "memory";
        }
    }
}
