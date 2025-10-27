using System;
using CityBuilder.Domain;
using NUnit.Framework;

namespace CityBuilder.Tests.Domain
{
    public sealed class CityStateTests
    {
        [Test]
        public void PlaceBuilding_WithinBounds_Succeeds()
        {
            var city = new CityState(4, 4);
            var position = new GridPosition(1, 1);

            var building = city.PlaceBuilding(Guid.NewGuid(), "House", position, 0);

            Assert.AreEqual(position, building.Position);
            Assert.IsTrue(city.IsCellOccupied(position));
        }

        [Test]
        public void PlaceBuilding_OccupiedCell_Throws()
        {
            var city = new CityState(4, 4);
            var position = new GridPosition(1, 1);
            city.PlaceBuilding(Guid.NewGuid(), "House", position, 0);

            Assert.Throws<DomainException>(() => city.PlaceBuilding(Guid.NewGuid(), "Farm", position, 0));
        }

        [Test]
        public void MoveBuilding_ToFreeCell_UpdatesPosition()
        {
            var city = new CityState(4, 4);
            var id = Guid.NewGuid();
            city.PlaceBuilding(id, "House", new GridPosition(0, 0), 0);
            var newPosition = new GridPosition(2, 2);

            city.MoveBuilding(id, newPosition, 90);

            var building = city.GetBuilding(id);
            Assert.AreEqual(newPosition, building.Position);
            Assert.AreEqual(90, building.Rotation);
        }
    }
}
