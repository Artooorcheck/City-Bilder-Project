namespace CityBuilder.Application.Events
{
    public readonly struct EconomyChangedEvent
    {
        public EconomyChangedEvent(int gold)
        {
            Gold = gold;
        }

        public int Gold { get; }
    }
}
