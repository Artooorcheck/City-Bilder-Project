namespace CityBuilder.Domain
{
    public sealed class EconomyState
    {
        public EconomyState(int gold)
        {
            if (gold < 0)
            {
                throw new DomainException("Gold cannot be negative.");
            }

            Gold = gold;
        }

        public int Gold { get; private set; }

        public bool CanAfford(int cost) => Gold >= cost;

        public void Spend(int amount)
        {
            if (amount < 0)
            {
                throw new DomainException("Cannot spend negative amount.");
            }

            if (Gold < amount)
            {
                throw new DomainException("Not enough gold.");
            }

            Gold -= amount;
        }

        public void Earn(int amount)
        {
            if (amount < 0)
            {
                throw new DomainException("Cannot earn negative amount.");
            }

            Gold += amount;
        }
    }
}
