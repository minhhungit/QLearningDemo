using ConsoleTableExt;

namespace QLearningDemo
{
    public class GameInstance
    {
        int[,] env;
        private Random rand = new Random();

        public GameInstance()
        {
            env = new int[GameConfig.ENV_SIZE, GameConfig.ENV_SIZE];
        }

        public void TrainAgent(long episode, bool enableLog = false)
        {
            var isGreedyOnly = GameConfig.GREEDLY_ONLY_MODE_LEARNING;

            //Console.WriteLine($"New game {DateTime.Now:HH:mm:ss fff}");

            // Train the Q-learning agent if no saved Q-table is found
            // Initialize the game environment
            int catX = -1, catY = -1, mouseX = -1, mouseY = -1, dogX = -1, dogY = -1;

            GameHelper.InitPositions(ref catX, ref catY, ref mouseX, ref mouseY, ref dogX, ref dogY);

            // Reset the game environment
            Array.Clear(env, 0, env.Length);

            env[catX, catY] = (int)Animal.CAT;
            env[mouseX, mouseY] = (int)Animal.MOUSE;
            env[dogX, dogY] = (int)Animal.DOG;

            if (enableLog)
            {
                Console.WriteLine($"Initial CAT [{catX},{catY}] - MOUSE [{mouseX},{mouseY}] - DOG [{dogX},{dogY}] - GreedyOnly ({(isGreedyOnly ? "true" : "false")})");
            }

            int currentX = catX, currentY = catY;
            bool isGameOver = false;
            int steps = 0; // Keep track of the number of steps

            string[,] tbl = new string[GameConfig.ENV_SIZE, GameConfig.ENV_SIZE];

            if (enableLog)
            {
                Console.WriteLine();

                for (int i = 0; i < GameConfig.ENV_SIZE; i++)
                {
                    for (int j = 0; j < GameConfig.ENV_SIZE; j++)
                    {
                        tbl[i, j] = $"{i},{j}";
                    }
                }

                tbl[catX, catY] = "C";
                tbl[mouseX, mouseY] = "M";
                tbl[dogX, dogY] = "D";
            }

            List<Tuple<int, int>> listPreviousPosition = new List<Tuple<int, int>>();

            //AgentAction? lastAction = null;
            bool caughtMouse = false;
            double reward = 0;
            while (!isGameOver && steps < GameConfig.MAX_GAME_STEPS)
            {
                listPreviousPosition.Add(new Tuple<int, int>(currentX, currentY));

                // Try to pick an action based on the current Q-values
                NextAction? nextAction = GameHelper.NextAction(listPreviousPosition, currentX, currentY, isGreedyOnly);

                if (nextAction == null)
                {
                    if (enableLog)
                    {
                        tbl[currentX, currentY] = "⭙";
                        Console.WriteLine($"Game {episode}\tStep {steps + 1}\tGOT STUCK");
                    }

                    break;
                }
                else
                {
                    // Perform the action and get the new state
                    int newX = nextAction.X, newY = nextAction.Y;

                    // Calculate the reward for the new state
                    var currentReward = GameHelper.GetReward(env, newX, newY, ref caughtMouse);
                    reward += currentReward;

                    if (enableLog)
                    {
                        string? epsilonInfo = string.Empty;
                        if (!isGreedyOnly)
                        {
                            epsilonInfo = $" - Ran.Dec/Epsilon {nextAction.RandomEpsilon}/{nextAction.DecayedEpsilon} {(nextAction.RandomEpsilon < nextAction.DecayedEpsilon ? "RANDOM" : "BEST")}";
                        }

                        Console.WriteLine($"Game {episode}\tStep {steps + 1}\t{currentX},{currentY} {nextAction.Action,-5} {newX},{newY}\tReward {currentReward} {epsilonInfo}");

                        var actionIcon = string.Empty;
                        if (nextAction == null)
                        {
                            actionIcon = "⭙";
                        }
                        else
                        {
                            switch (nextAction.Action)
                            {
                                case AgentAction.LEFT:
                                    actionIcon = "←";
                                    break;
                                case AgentAction.RIGHT:
                                    actionIcon = "→";
                                    break;
                                case AgentAction.UP:
                                    actionIcon = "↑";
                                    break;
                                case AgentAction.DOWN:
                                    actionIcon = "↓";
                                    break;
                                default:
                                    break;
                            }
                        }

                        if ($"{newX},{newY}" == $"{mouseX},{mouseY}")
                        {
                            tbl[newX, newY] = $"{actionIcon} M";
                        }
                        else if ($"{newX},{newY}" == $"{dogX},{dogY}")
                        {
                            tbl[newX, newY] = $"{actionIcon} D";
                        }
                        else
                        {
                            tbl[newX, newY] = $"{actionIcon}";
                        }
                    }

                    // Update the Q-table
                    double maxQNew = GetMaxQ(newX, newY, GameConfig.Q);
                    double oldQ = GameConfig.Q[currentX, currentY, (int)nextAction.Action];
                    double newQ = oldQ + GameConfig.LEARNING_RATE * (currentReward + GameConfig.DISCOUNT_FACTOR * maxQNew - oldQ);
                    GameConfig.Q[currentX, currentY, (int)nextAction.Action] = newQ;

                    // Update the current state
                    currentX = newX;
                    currentY = newY;

                    // Check if the game is over
                    isGameOver = GameHelper.IsGameOver(env, newX, newY);
                }

                steps++;
            }

            if (enableLog)
            {
                if (caughtMouse)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                ConsoleTableBuilder
                    .From(GameHelper.ConvertToListOfLists(tbl))
                    .ExportAndWriteLine();

                Console.ResetColor();

                Console.WriteLine($"Game {episode} DONE!\t(Reward = {reward})");
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        // Find the maximum Q-value for the new state
        double GetMaxQ(int x, int y, double[,,] Q)
        {
            double maxQ = double.MinValue;

            for (int a = 0; a < GameConfig.NUMBER_OF_ACTION; a++)
            {
                maxQ = Math.Max(maxQ, Q[x, y, a]);
            }

            return maxQ;
        }
    }
}