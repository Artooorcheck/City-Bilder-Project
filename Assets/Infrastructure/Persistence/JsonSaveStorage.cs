using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CityBuilder.Application.Data;
using CityBuilder.Application.Interfaces;
using UnityEngine;

namespace CityBuilder.Infrastructure.Persistence
{
    public sealed class JsonSaveStorage : ISaveStorage
    {
        private readonly string _filePath;

        public JsonSaveStorage()
        {
            _filePath = Path.Combine(UnityEngine.Application.persistentDataPath, "city_save.json");
        }

        public string LastSaveLocation => _filePath;

        public async Task SaveAsync(CitySaveData data, CancellationToken cancellationToken)
        {
            var json = JsonUtility.ToJson(data, true);
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await Task.Run(() => File.WriteAllText(_filePath, json), cancellationToken);
        }

        public async Task<CitySaveData> LoadAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            var json = await Task.Run(() => File.ReadAllText(_filePath), cancellationToken);
            return JsonUtility.FromJson<CitySaveData>(json);
        }
    }
}
