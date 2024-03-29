namespace DeepQLearning
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int gridSize = 4;
            const int numActions = 4; // left, right, up, down
            const int numEpisodes = 1000;
            const int batchSize = 32;

            Env env = new Env(gridSize);
            QNetwork qNetwork = new QNetwork(6, numActions); // 6 input nodes for (cat_x, cat_y, mouse_x, mouse_y, dog_x, dog_y)
            QNetwork targetNetwork = new QNetwork(6, numActions);
            ReplayMemory memory = new ReplayMemory(10000);

            double epsilon = 1.0; // Exploration rate
            double epsilonDecay = 0.995;
            double epsilonMin = 0.01;

            double gamma = 0.99; // Discount factor
            double learningRate = 0.001;

            for (int episode = 0; episode < numEpisodes; episode++)
            {
                env.Reset();
                double[] state = env.GetStateVector();
                bool done = false;

                while (!done)
                {
                    int action = qNetwork.EpsilonGreedyAction(state, epsilon);
                    double[] nextState;
                    double reward;
                    done = env.Step(action, out nextState, out reward);

                    memory.AddExperience(state, action, reward, nextState, done);

                    if (memory.Count >= batchSize)
                    {
                        qNetwork.TrainBatch(memory.SampleBatch(batchSize), targetNetwork, gamma, learningRate);
                    }

                    state = nextState;
                }

                if (episode % 10 == 0)
                {
                    targetNetwork.CopyFrom(qNetwork);
                }

                epsilon = Math.Max(epsilon * epsilonDecay, epsilonMin);
            }

            // Evaluate the trained network
            double totalReward = 0.0;
            for (int episode = 0; episode < 100; episode++)
            {
                env.Reset();
                double[] state = env.GetStateVector();
                bool done = false;

                while (!done)
                {
                    int action = qNetwork.GreedyAction(state);
                    double[] nextState;
                    double reward;
                    done = env.Step(action, out nextState, out reward);
                    totalReward += reward;
                    state = nextState;
                }
            }

            Console.WriteLine($"Average reward: {totalReward / 100}");
        }
    }
}
