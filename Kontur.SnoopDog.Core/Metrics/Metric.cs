namespace Kontur.SnoopDog.Core.Metrics
{
    public class Metric
    {
        public Metric(string name, long value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public long Value { get; }
    }
}