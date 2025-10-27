namespace CityBuilder.Application.Events
{
    public readonly struct NotEnoughGoldEvent
    {
        public NotEnoughGoldEvent(int required, int current)
        {
            Required = required;
            Current = current;
        }

        public int Required { get; }

        public int Current { get; }
    }
}
