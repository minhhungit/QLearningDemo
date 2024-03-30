namespace DeepQLearning
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int numActions = Enum.GetNames(typeof(AgentAction)).Length; // left, right, up, down
            const int numEpisodes = 10000;
            const int batchSize = 32;

            Env env = new Env(GameConfig.ENV_SIZE);
            QNetwork qNetwork = new QNetwork(6, numActions); // 6 input nodes for (cat_x, cat_y, mouse_x, mouse_y, dog_x, dog_y)
            QNetwork targetNetwork = new QNetwork(6, numActions);
            ReplayMemory memory = new ReplayMemory(1000);
            //string modelFilePath = "trained_model.dat";
            string modelFilePath = "trained_model.txt";

            qNetwork.LoadModel(modelFilePath);
            Console.WriteLine("Model loaded successfully.");

            double epsilon = 1.0; // Exploration rate
            double epsilonDecay = 0.995;
            double epsilonMin = 0.01;

            double gamma = 0.99; // Discount factor
            double learningRate = 0.001;

            for (int episode = 0; episode < numEpisodes; episode++)
            {
                if (episode > 0 && episode % 10_001 == 0)
                {
                    qNetwork.SaveModel(modelFilePath);
                    Console.WriteLine("Model saved successfully.");

                    Console.WriteLine($"{episode}");
                }

                env.Reset();
                int[] state = env.GetStateVector();
                bool done = false;

                List<Tuple<int, int>> prevPositions = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(state[0], state[1])
                };

                while (!done)
                {
                    AgentAction? action = qNetwork.EpsilonGreedyAction(prevPositions, state, epsilon);

                    if (action == null)
                    {
                        //tbl[currentX, currentY] = "⭙";

                        //if (enableLog)
                        //{
                        //    Console.WriteLine($"Game {episode}\tStep {steps + 1}\tGOT STUCK");
                        //}

                        break;
                    }
                    else
                    {
                        int[] nextState;
                        double reward;
                        done = env.Step((AgentAction)action, out nextState, out reward);
                        prevPositions.Add(new Tuple<int, int>(nextState[0], nextState[1]));

                        memory.AddExperience(state, (AgentAction)action, reward, nextState, done);

                        if (memory.Count >= batchSize)
                        {
                            qNetwork.TrainBatch(memory.SampleBatch(batchSize), targetNetwork, gamma, learningRate);
                        }

                        state = nextState;
                    }                    
                }

                if (episode % 10 == 0)
                {
                    targetNetwork.CopyFrom(qNetwork);
                }

                epsilon = Math.Max(epsilon * epsilonDecay, epsilonMin);
            }

            // Evaluate the trained network
            double avgReward = 0.0;
            for (int episode = 0; episode < 100; episode++)
            {
                env.Reset();
                int[] state = env.GetStateVector();
                bool done = false;
                List<Tuple<int, int>> prevPositions = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(state[0], state[1])
                };

                double reward = 0;

                while (!done)
                {
                    AgentAction? action = qNetwork.GreedyAction(prevPositions, state);

                    if (action == null)
                    {
                        //tbl[currentX, currentY] = "⭙";

                        //if (enableLog)
                        //{
                        //    Console.WriteLine($"Game {episode}\tStep {steps + 1}\tGOT STUCK");
                        //}

                        break;
                    }
                    else
                    {
                        int[] nextState;
                        double stepReward;
                        done = env.Step((AgentAction)action, out nextState, out stepReward);
                        prevPositions.Add(new Tuple<int, int>(nextState[0], nextState[1]));

                        reward += stepReward;
                        state = nextState;
                    }
                }

                Console.WriteLine($"episode {episode + 1} reward {reward}");

                avgReward += reward;
            }

            Console.WriteLine($"Average reward: {avgReward / 100}");
        }
    }
}
