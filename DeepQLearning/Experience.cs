namespace DeepQLearning
{
    public class Experience
    {
        public double[] State { get; set; }
        public int Action { get; set; }
        public double Reward { get; set; }
        public double[] NextState { get; set; }
        public bool Done { get; set; }
    }
}
