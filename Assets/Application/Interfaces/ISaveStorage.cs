using System.Threading;
using System.Threading.Tasks;
using CityBuilder.Application.Data;

namespace CityBuilder.Application.Interfaces
{
    public interface ISaveStorage
    {
        Task SaveAsync(CitySaveData data, CancellationToken cancellationToken);

        Task<CitySaveData> LoadAsync(CancellationToken cancellationToken);

        string LastSaveLocation { get; }
    }
}
