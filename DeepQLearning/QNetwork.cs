namespace DeepQLearning
{
    public class QNetwork
    {
        private double[] weights;
        private int numInputs;
        private int numActions;
        private Random random;

        public QNetwork(int numInputs, int numActions)
        {
            this.numInputs = numInputs;
            this.numActions = numActions;
            random = new Random();
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            // Initialize weights randomly
            weights = new double[(numInputs + 1) * numActions];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = random.NextDouble() * 2 - 1; // Random value between -1 and 1
            }
        }

        public int EpsilonGreedyAction(double[] state, double epsilon)
        {
            if (random.NextDouble() < epsilon)
            {
                // Explore: Choose a random action
                return random.Next(numActions);
            }
            else
            {
                // Exploit: Choose the action with the highest Q-value
                return GreedyAction(state);
            }
        }

        public int GreedyAction(double[] state)
        {
            double[] qValues = GetQValues(state);
            int bestAction = 0;
            double maxQValue = qValues[0];

            for (int i = 1; i < numActions; i++)
            {
                if (qValues[i] > maxQValue)
                {
                    maxQValue = qValues[i];
                    bestAction = i;
                }
            }

            return bestAction;
        }

        public double[] GetQValues(double[] state)
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
                    targetQValues[experience.Action] = experience.Reward;
                }
                else
                {
                    double maxNextQValue = targetNetwork.GetQValues(experience.NextState).Max();
                    targetQValues[experience.Action] = experience.Reward + gamma * maxNextQValue;
                }

                for (int input = 0; input < numInputs; input++)
                {
                    double delta = targetQValues[experience.Action] - currentQValues[experience.Action];
                    weights[input * numActions + experience.Action] += learningRate * delta * experience.State[input];
                }
                weights[numInputs * numActions + experience.Action] += learningRate * (targetQValues[experience.Action] - currentQValues[experience.Action]);
            }
        }

        public void CopyFrom(QNetwork other)
        {
            other.weights.CopyTo(weights, 0);
        }
    }
}
