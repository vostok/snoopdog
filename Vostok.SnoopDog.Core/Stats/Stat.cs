namespace Vostok.SnoopDog.Core.Stats
{
    public abstract class Stat
    {
        protected Stat(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title { get; }
        public string Description { get; }
    }
}