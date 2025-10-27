namespace CityBuilder.Domain
{
    public sealed class BuildingLevelDefinition
    {
        public BuildingLevelDefinition(int level, int cost, int income)
        {
            Level = level;
            Cost = cost;
            Income = income;
        }

        public int Level { get; }

        public int Cost { get; }

        public int Income { get; }
    }
}
