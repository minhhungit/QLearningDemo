namespace QLearningDemo
{
    // Choose an action based on the current Q-values

    public class ActionAndPositionEntry
    {
        public AgentAction Action { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public double RandomEpsilon { get; set; }
    }
}