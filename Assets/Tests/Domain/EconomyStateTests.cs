using CityBuilder.Domain;
using NUnit.Framework;

namespace CityBuilder.Tests.Domain
{
    public sealed class EconomyStateTests
    {
        [Test]
        public void Spend_WhenEnoughGold_DecreasesBalance()
        {
            var economy = new EconomyState(200);

            economy.Spend(50);

            Assert.AreEqual(150, economy.Gold);
        }

        [Test]
        public void Spend_WhenNotEnoughGold_Throws()
        {
            var economy = new EconomyState(10);

            Assert.Throws<DomainException>(() => economy.Spend(100));
        }

        [Test]
        public void Earn_AddsGold()
        {
            var economy = new EconomyState(0);

            economy.Earn(75);

            Assert.AreEqual(75, economy.Gold);
        }
    }
}
