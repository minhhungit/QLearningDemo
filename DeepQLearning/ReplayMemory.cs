using System;

namespace DeepQLearning
{
    public class ReplayMemory
    {
        private List<Experience> memory;
        private int maxSize;
        private Random random;

        public ReplayMemory(int maxSize)
        {
            this.maxSize = maxSize;
            memory = new List<Experience>();
            random = new Random();
        }

        public int Count
        {
            get { return memory.Count; }
        }

        public void AddExperience(int[] state, AgentAction action, double reward, int[] nextState, bool done)
        {
            memory.Add(new Experience
            {
                State = state,
                Action = action,
                Reward = reward,
                NextState = nextState,
                Done = done
            });

            if (memory.Count > maxSize)
            {
                memory.RemoveAt(0);
            }
        }

        public List<Experience> SampleBatch(int batchSize)
        {
            List<Experience> batch = new List<Experience>();
            List<int> indices = new List<int>();

            for (int i = 0; i < memory.Count; i++)
            {
                indices.Add(i);
            }

            for (int i = 0; i < batchSize; i++)
            {
                int index = random.Next(indices.Count);
                batch.Add(memory[indices[index]]);
                indices.RemoveAt(index);
            }

            return batch;
        }
    }
}
