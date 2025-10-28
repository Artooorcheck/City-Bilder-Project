using System;
using CityBuilder.Application.Services;
using VContainer.Unity;

namespace CityBuilder.Infrastructure.DI
{
    public sealed class GameLoopEntryPoint : IDisposable
    {
        private readonly EconomyTickService _economyTickService;
        private readonly AutoSaveService _autoSaveService;

        public GameLoopEntryPoint(EconomyTickService economyTickService, AutoSaveService autoSaveService)
        {
            _economyTickService = economyTickService;
            _autoSaveService = autoSaveService;
        }

        public void Dispose()
        {
            _ = _economyTickService.DisposeAsync();
            _ = _autoSaveService.DisposeAsync();
        }
    }
}
