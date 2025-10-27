using System;

namespace CityBuilder.Application.Data
{
    [Serializable]
    public sealed class CitySaveData
    {
        public int Width;
        public int Height;
        public int Gold;
        public BuildingSaveData[] Buildings = Array.Empty<BuildingSaveData>();
    }
}
