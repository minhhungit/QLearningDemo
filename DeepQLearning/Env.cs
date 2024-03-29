namespace DeepQLearning
{
    public class Env
    {
        private int gridSize;
        private int[] catPosition;
        private int[] mousePosition;
        private int[] dogPosition;
        private Random rand;

        public Env(int gridSize)
        {
            this.gridSize = gridSize;
            rand = new Random();
            Reset();
        }

        public void Reset()
        {
            int catX = -1, catY = -1, mouseX = -1, mouseY = -1, dogX = -1, dogY = -1;

            do
            {
                catX = rand.Next(rand.Next(gridSize));
                catY = rand.Next(rand.Next(gridSize));
                mouseX = rand.Next(rand.Next(gridSize));
                mouseY = rand.Next(rand.Next(gridSize));
                dogX = rand.Next(rand.Next(gridSize));
                dogY = rand.Next(rand.Next(gridSize));
            } while ((catX == mouseX && catY == mouseY) || (catX == dogX && catY == dogY) || (mouseX == dogX && mouseY == dogY));

            catPosition = [catX, catY];
            mousePosition = [mouseX, mouseY];
            dogPosition = [dogX, dogY];
        }

        public double[] GetStateVector()
        {
            return new double[]
            {
                catPosition[0], catPosition[1],
                mousePosition[0], mousePosition[1],
                dogPosition[0], dogPosition[1]
            };
        }

        public bool Step(int action, out double[] nextState, out double reward)
        {
            int[] newCatPosition = GetNewPosition(catPosition, action);
            reward = GetReward(newCatPosition);

            if (reward == 1 || reward == -1)
            {
                Reset();
                nextState = GetStateVector();
                return true;
            }

            catPosition = newCatPosition;
            nextState = GetStateVector();
            return false;
        }

        private int[] GetNewPosition(int[] position, int action)
        {
            int x = position[0];
            int y = position[1];

            switch (action)
            {
                case 0: // left
                    x = Math.Max(x - 1, 0);
                    break;
                case 1: // right
                    x = Math.Min(x + 1, gridSize - 1);
                    break;
                case 2: // up
                    y = Math.Max(y - 1, 0);
                    break;
                case 3: // down
                    y = Math.Min(y + 1, gridSize - 1);
                    break;
            }

            return new[] { x, y };
        }

        private double GetReward(int[] newCatPosition)
        {
            if (newCatPosition[0] == mousePosition[0] && newCatPosition[1] == mousePosition[1])
                return 1; // Cat caught the mouse
            else if (newCatPosition[0] == dogPosition[0] && newCatPosition[1] == dogPosition[1])
                return -1; // Cat ran into the dog
            else
                return 0; // No reward
        }
    }
}
