using System;
using System.Collections.Generic;

namespace CityBuilder.Domain
{
    public sealed class CityState
    {
        private readonly Dictionary<Guid, Building> _buildings = new();
        private readonly Dictionary<GridPosition, Guid> _grid = new();

        public CityState(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new DomainException("Grid dimensions must be positive.");
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyCollection<Building> Buildings => _buildings.Values;

        public bool IsWithinBounds(GridPosition position) => position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;

        public bool IsCellOccupied(GridPosition position) => _grid.ContainsKey(position);

        public Building GetBuilding(Guid id)
        {
            if (!_buildings.TryGetValue(id, out var building))
            {
                throw new DomainException($"Building '{id}' not found.");
            }

            return building;
        }

        public Building PlaceBuilding(Guid id, string typeId, GridPosition position, int rotation)
        {
            ValidatePlacement(position);

            var building = new Building(id, typeId, position, rotation, level: 1);
            _buildings.Add(id, building);
            _grid.Add(position, id);
            return building;
        }

        public void RemoveBuilding(Guid id)
        {
            if (!_buildings.TryGetValue(id, out var building))
            {
                throw new DomainException($"Building '{id}' not found.");
            }

            _buildings.Remove(id);
            _grid.Remove(building.Position);
        }

        public void MoveBuilding(Guid id, GridPosition newPosition, int rotation)
        {
            if (!_buildings.TryGetValue(id, out var building))
            {
                throw new DomainException($"Building '{id}' not found.");
            }

            if (building.Position == newPosition && building.Rotation == rotation)
            {
                return;
            }

            ValidatePlacement(newPosition);

            _grid.Remove(building.Position);
            building.Move(newPosition, rotation);
            _grid.Add(newPosition, id);
        }

        private void ValidatePlacement(GridPosition position)
        {
            if (!IsWithinBounds(position))
            {
                throw new DomainException($"Position {position} is outside of the grid bounds.");
            }

            if (IsCellOccupied(position))
            {
                throw new DomainException($"Position {position} is already occupied.");
            }
        }
    }
}
