using System;

namespace DeepQLearning
{
    public class Experience
    {
        public int[] State { get; set; }
        public AgentAction Action { get; set; }
        public double Reward { get; set; }
        public int[] NextState { get; set; }
        public bool Done { get; set; }
    }
}
