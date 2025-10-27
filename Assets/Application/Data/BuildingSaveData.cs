using System;
using UnityEngine;

namespace CityBuilder.Application.Data
{
    [Serializable]
    public sealed class BuildingSaveData
    {
        public string TypeId = string.Empty;
        public int Level;
        public int X;
        public int Y;
        public int Rotation;

        [SerializeField] private string id;

        public Guid Id
        {
            get => Guid.Parse(id);
            set => id = value.ToString();
        }
    }
}
