namespace CityBuilder.Application.Events
{
    public readonly struct GameSavedEvent
    {
        public GameSavedEvent(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
