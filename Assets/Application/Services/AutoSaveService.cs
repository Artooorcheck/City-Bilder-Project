using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using CityBuilder.Application.UseCases;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace CityBuilder.Application.Services
{
    public sealed class AutoSaveService : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly SaveGameUseCase _saveGameUseCase;
        private readonly float _intervalSeconds;

        public AutoSaveService(SaveGameUseCase saveGameUseCase, float intervalSeconds)
        {
            _saveGameUseCase = saveGameUseCase;
            _intervalSeconds = Math.Max(5f, intervalSeconds);
            RunAsync().Forget();
        }

        private async UniTaskVoid RunAsync()
        {
            var interval = TimeSpan.FromSeconds(_intervalSeconds);
            try
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(interval, interval))
                {
                    await _saveGameUseCase.ExecuteAsync(_cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            await UniTask.CompletedTask;
        }
    }
}
