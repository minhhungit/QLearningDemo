
namespace DeepQLearning
{
    public class GameConfig
    {
        public static long TOTAL_NBR_OF_PLAYED_GAMES = 0;

        public const int ENV_SIZE = 4;

        // Define the Q-learning parameters
        public const double LEARNING_RATE = 0.7;// Alpha
        public const double DISCOUNT_FACTOR = 0.95;// Gamma

        // Epsilon-greedy strategy (explore or exploit)
        public const double START_EPSILON = 1.0; // start training by selecting purely random actions
        public const double MIN_EPSILON = 0.05;   // the lowest epsilon allowed to decay to
        public const double DECAY_RATE = 0.00000001;   // epsilon will gradually decay so we do less exploring and more exploiting as Q-function improves
    }
}
