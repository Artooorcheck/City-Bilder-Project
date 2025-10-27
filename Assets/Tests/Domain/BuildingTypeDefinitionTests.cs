using System.Collections.Generic;
using CityBuilder.Domain;
using NUnit.Framework;

namespace CityBuilder.Tests.Domain
{
    public sealed class BuildingTypeDefinitionTests
    {
        [Test]
        public void TryGetNextLevel_WhenAvailable_ReturnsDefinition()
        {
            var definition = new BuildingTypeDefinition(
                "House",
                "House",
                new List<BuildingLevelDefinition>
                {
                    new(1, 100, 2),
                    new(2, 150, 4)
                });

            var result = definition.TryGetNextLevel(1, out var nextLevel);

            Assert.IsTrue(result);
            Assert.NotNull(nextLevel);
            Assert.AreEqual(2, nextLevel!.Level);
        }

        [Test]
        public void TryGetNextLevel_WhenMaxed_ReturnsFalse()
        {
            var definition = new BuildingTypeDefinition(
                "House",
                "House",
                new List<BuildingLevelDefinition>
                {
                    new(1, 100, 2)
                });

            var result = definition.TryGetNextLevel(1, out var nextLevel);

            Assert.IsFalse(result);
            Assert.IsNull(nextLevel);
        }
    }
}
