namespace DeepQLearning
{
    public class NextAction
    {
        public int X { get; set; }
        public int Y { get; set; }
        public AgentAction Action { get; set; }
        public double RandomEpsilon { get; set; }
        public double DecayedEpsilon { get; set; }
    }
}