using System.ComponentModel.DataAnnotations;

namespace DeepQLearning
{
    public class QNetwork
    {
        private double[] weights;
        private int numInputs;
        private int numActions;
        private Random rand;

        public QNetwork(int numInputs, int numActions)
        {
            this.numInputs = numInputs;
            this.numActions = numActions;
            rand = new Random();
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            // Initialize weights randomly
            weights = new double[(numInputs + 1) * numActions];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = rand.NextDouble() * 2 - 1; // Random value between -1 and 1
            }
        }

        public AgentAction? EpsilonGreedyAction(List<Tuple<int, int>> prevPositions, int[] state, double epsilon)
        {
            if (rand.NextDouble() < epsilon)
            {
                // Explore: Choose a random action

                var x = state[0]; // catX
                var y = state[1]; // catY

                var availableActions = GetAvailableAction(prevPositions, x, y);

                if (availableActions.Count() == 0)
                {
                    return null;
                }

                int randomIndex = rand.Next(0, availableActions.Count);
                return availableActions[randomIndex];
            }
            else
            {
                // Exploit: Choose the action with the highest Q-value
                return GreedyAction(prevPositions, state);
            }
        }

        public List<AgentAction> GetAvailableAction(List<Tuple<int, int>> prevPositions, int x, int y)
        {
            var blockAction = new List<AgentAction>();
            if (x == 0)
            {
                blockAction.Add(AgentAction.LEFT);
            }
            if (y == 0)
            {
                blockAction.Add(AgentAction.UP);
            }

            if (x == GameConfig.ENV_SIZE - 1)
            {
                blockAction.Add(AgentAction.RIGHT);
            }

            if (y == GameConfig.ENV_SIZE - 1)
            {
                blockAction.Add(AgentAction.DOWN);
            }

            if (x == 0 && y == 0)
            {
                blockAction.Add(AgentAction.LEFT);
                blockAction.Add(AgentAction.UP);
            }

            if (x == GameConfig.ENV_SIZE - 1 && y == 0)
            {
                blockAction.Add(AgentAction.UP);
                blockAction.Add(AgentAction.RIGHT);
            }

            if (x == GameConfig.ENV_SIZE - 1 && y == GameConfig.ENV_SIZE - 1)
            {
                blockAction.Add(AgentAction.RIGHT);
                blockAction.Add(AgentAction.DOWN);
            }

            if (x == 0 && y == GameConfig.ENV_SIZE - 1)
            {
                blockAction.Add(AgentAction.DOWN);
                blockAction.Add(AgentAction.LEFT);
            }

            blockAction = blockAction.Distinct().ToList();

            var availableActions = new List<AgentAction>();
            foreach (AgentAction a in (AgentAction[])Enum.GetValues(typeof(AgentAction)))
            {
                if (blockAction.Contains(a))
                {
                    continue;
                }
                else
                {
                    NextAction positionByMove = GameHelper.MoveAgent(x, y, a);
                    if (!prevPositions.Contains(new Tuple<int, int>(positionByMove.X, positionByMove.Y)))
                    {
                        availableActions.Add(a);
                    }
                }
            }

            return availableActions;
        }

        public AgentAction? GreedyAction(List<Tuple<int, int>> prevPositions, int[] state)
        {
            int x = state[0];
            int y = state[1];
            
            var availableActions = GetAvailableAction(prevPositions, x, y);

            if (availableActions.Count == 0)
            {
                return null;
            }

            double[] qValues = GetQValues(state);
            int bestAction = -1;
            double maxQValue = double.MinValue;

            foreach (var a in availableActions)
            {
                if (qValues[(int)a] > maxQValue) // @TODO: NEED TO CHECK 
                {
                    maxQValue = qValues[(int)a];
                    bestAction = (int)a;
                }
            }

            return (AgentAction)bestAction;
        }

        public double[] GetQValues(int[] state)
        {
            double[] qValues = new double[numActions];

            for (int action = 0; action < numActions; action++)
            {
                double sum = 0.0;
                for (int input = 0; input < numInputs; input++)
                {
                    sum += state[input] * weights[input * numActions + action];
                }
                sum += weights[numInputs * numActions + action]; // Bias term
                qValues[action] = sum;
            }

            return qValues;
        }

        public void TrainBatch(List<Experience> batch, QNetwork targetNetwork, double gamma, double learningRate)
        {
            double[] targetQValues = new double[numActions];

            foreach (Experience experience in batch)
            {
                double[] currentQValues = GetQValues(experience.State);
                if (experience.Done)
                {
                    targetQValues[(int)experience.Action] = experience.Reward;
                }
                else
                {
                    double maxNextQValue = targetNetwork.GetQValues(experience.NextState).Max();
                    targetQValues[(int)experience.Action] = experience.Reward + gamma * maxNextQValue;
                }

                for (int input = 0; input < numInputs; input++)
                {
                    double delta = targetQValues[(int)experience.Action] - currentQValues[(int)experience.Action];
                    weights[input * numActions + (int)experience.Action] += learningRate * delta * experience.State[input];
                }
                weights[numInputs * numActions + (int)experience.Action] += learningRate * (targetQValues[(int)experience.Action] - currentQValues[(int)experience.Action]);
            }
        }

        public void CopyFrom(QNetwork other)
        {
            other.weights.CopyTo(weights, 0);
        }

        public void SaveModel(string filePath)
        {
            //using (FileStream fs = new FileStream(filePath, FileMode.Create))
            //{
            //    using (BinaryWriter writer = new BinaryWriter(fs))
            //    {
            //        writer.Write(weights.Length);
            //        foreach (double weight in weights)
            //        {
            //            writer.Write(weight);
            //        }
            //    }
            //}

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(weights.Length);
                for (int i = 0; i < weights.Length; i++)
                {
                    writer.WriteLine(weights[i]);
                }
            }
        }

        public void LoadModel(string filePath)
        {
            //using (FileStream fs = new FileStream(filePath, FileMode.Open))
            //{
            //    using (BinaryReader reader = new BinaryReader(fs))
            //    {
            //        int weightsCount = reader.ReadInt32();
            //        weights = new double[weightsCount];
            //        for (int i = 0; i < weightsCount; i++)
            //        {
            //            weights[i] = reader.ReadDouble();
            //        }
            //    }
            //}

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    int weightsCount = int.Parse(reader.ReadLine());
                    weights = new double[weightsCount];
                    for (int i = 0; i < weightsCount; i++)
                    {
                        weights[i] = double.Parse(reader.ReadLine());
                    }
                }
            }
        }
    }
}
