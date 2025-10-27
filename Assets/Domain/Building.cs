using System;

namespace CityBuilder.Domain
{
    public sealed class Building
    {
        public Building(Guid id, string typeId, GridPosition position, int rotation, int level)
        {
            Id = id;
            TypeId = typeId;
            Position = position;
            Rotation = rotation;
            Level = level;
        }

        public Guid Id { get; }

        public string TypeId { get; }

        public GridPosition Position { get; private set; }

        public int Rotation { get; private set; }

        public int Level { get; private set; }

        public void Move(GridPosition newPosition, int rotation)
        {
            Position = newPosition;
            Rotation = rotation;
        }

        public void Upgrade()
        {
            Level += 1;
        }
    }
}
