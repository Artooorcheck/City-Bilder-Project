using CityBuilder.Application.UseCases;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CityBuilder.Application.Services
{
    public sealed class EconomyTickService : IAsyncDisposable
    {
        private readonly Subject<int> _incomeSubject = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly CollectIncomeUseCase _collectIncomeUseCase;
        private readonly float _tickSeconds;

        public EconomyTickService(CollectIncomeUseCase collectIncomeUseCase, float tickSeconds)
        {
            _collectIncomeUseCase = collectIncomeUseCase;
            _tickSeconds = Math.Max(0.1f, tickSeconds);
            RunAsync().Forget();
        }

        // <-- используем R3.IObservable, а не System.IObservable
        public Observable<int> IncomeStream => _incomeSubject.AsObservable();

        private async UniTaskVoid RunAsync()
        {
            try
            {
                var interval = TimeSpan.FromSeconds(_tickSeconds);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(interval, interval))
                {
                    var income = _collectIncomeUseCase.Execute();
                    if (income > 0)
                        _incomeSubject.OnNext(income);
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _incomeSubject.OnCompleted();
            _incomeSubject.Dispose();
            _cts.Dispose();
            await UniTask.CompletedTask;
        }
    }
}
