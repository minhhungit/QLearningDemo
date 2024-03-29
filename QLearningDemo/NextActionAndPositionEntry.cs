namespace QLearningDemo
{
    public class NextActionAndPositionEntry
    {
        public AgentAction Action { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public double RandomEpsilon { get; set; }
    }
}