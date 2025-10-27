using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Application.Services;
using CityBuilder.Application.UseCases;
using CityBuilder.Domain;
using CityBuilder.Infrastructure.Config;
using CityBuilder.Infrastructure.Persistence;
using CityBuilder.Infrastructure.Repositories;
using MessagePipe;
using Serilog;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CityBuilder.Infrastructure.DI
{
    /// <summary>
    /// Configures dependency injection bindings for the game runtime scope.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private BuildingCatalogConfig _catalogConfig;

        [SerializeField]
        private GameplaySettings _gameplaySettings;

        /// <summary>
        /// Registers dependencies and services required for gameplay execution.
        /// </summary>
        /// <param name="builder">The container builder used to configure the scope.</param>
        protected override void Configure(IContainerBuilder builder)
        {
            _catalogConfig ??= Resources.Load<BuildingCatalogConfig>("Settings/BuildingCatalog");
            _gameplaySettings ??= Resources.Load<GameplaySettings>("Settings/GameplaySettings");

            if (_catalogConfig == null || _gameplaySettings == null)
            {
                Log.Error("[GameLifetimeScope.Configure] Gameplay settings or building catalog are missing from Resources/Settings.");
                return;
            }

            var options = builder.RegisterMessagePipe();

            builder.RegisterMessageBroker<BuildingPlacedEvent>(options);
            builder.RegisterMessageBroker<BuildingRemovedEvent>(options);
            builder.RegisterMessageBroker<BuildingMovedEvent>(options);
            builder.RegisterMessageBroker<BuildingUpgradedEvent>(options);
            builder.RegisterMessageBroker<EconomyChangedEvent>(options);
            builder.RegisterMessageBroker<NotEnoughGoldEvent>(options);
            builder.RegisterMessageBroker<GameSavedEvent>(options);
            builder.RegisterMessageBroker<GameLoadedEvent>(options);

            builder.RegisterInstance(_catalogConfig);
            builder.RegisterInstance(_gameplaySettings);
            builder.Register<BuildingCatalogProvider>(Lifetime.Singleton).As<IBuildingCatalog>();

            builder.RegisterInstance(new CityState(_gameplaySettings.GridWidth, _gameplaySettings.GridHeight));
            builder.RegisterInstance(new EconomyState(_gameplaySettings.StartingGold));
            builder.Register<CityRepository>(Lifetime.Singleton).As<ICityRepository>();
            builder.Register<EconomyRepository>(Lifetime.Singleton).As<IEconomyRepository>();
            builder.Register<JsonSaveStorage>(Lifetime.Singleton).As<ISaveStorage>();

            builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton);
            builder.Register<RemoveBuildingUseCase>(Lifetime.Singleton);
            builder.Register<MoveBuildingUseCase>(Lifetime.Singleton);
            builder.Register<UpgradeBuildingUseCase>(Lifetime.Singleton);
            builder.Register<CollectIncomeUseCase>(Lifetime.Singleton);
            builder.Register<SaveGameUseCase>(Lifetime.Singleton);
            builder.Register<LoadGameUseCase>(Lifetime.Singleton);

            builder.Register<EconomyTickService>(Lifetime.Singleton).WithParameter<float>(_gameplaySettings.IncomeTickSeconds);
            builder.Register<AutoSaveService>(Lifetime.Singleton).WithParameter<float>(_gameplaySettings.AutoSaveSeconds);
            builder.RegisterEntryPoint<GameLoopEntryPoint>(Lifetime.Singleton);
        }
    }
}
