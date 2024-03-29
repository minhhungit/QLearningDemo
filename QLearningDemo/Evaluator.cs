using ConsoleTableExt;

namespace QLearningDemo
{
    public class Evaluator
    {
        int[,] env;

        public Evaluator()
        {
            env = new int[GameConfig.ENV_SIZE, GameConfig.ENV_SIZE];
        }

        public void Run()
        {
            EvaluateAgent(1);
        }

        public void Run(int numberOfEvalute)
        {
            EvaluateAgent(numberOfEvalute);
        }

        private void EvaluateAgent(int numberOfEvalute)
        {
            var isGreedyOnly = GameConfig.GREEDLY_ONLY_MODE_EVALUATE;

            double totalReward = 0;

            for (int episode = 0; episode < numberOfEvalute; episode++)
            {
                Console.WriteLine("----------------------------");
                // Reset the game environment
                Array.Clear(env, 0, env.Length);

                int catX = -1, catY = -1, mouseX = -1, mouseY = -1, dogX = -1, dogY = -1;
                GameHelper.InitPositions(ref catX, ref catY, ref mouseX, ref mouseY, ref dogX, ref dogY);

                env[catX, catY] = (int)Animal.CAT;
                env[mouseX, mouseY] = (int)Animal.MOUSE;
                env[dogX, dogY] = (int)Animal.DOG;

                Console.WriteLine($"Initial CAT [{catX},{catY}] - MOUSE [{mouseX},{mouseY}] - DOG [{dogX},{dogY}] - GreedyOnly ({(isGreedyOnly ? "true" : "false")})");

                int currentX = catX, currentY = catY;
                bool isGameOver = false;
                double reward = 0;
                int steps = 0; // Keep track of the number of steps

                List<Tuple<int, int>> listPreviousPosition = new List<Tuple<int, int>>();

                Console.WriteLine();
                string[,] tbl = new string[GameConfig.ENV_SIZE, GameConfig.ENV_SIZE];

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

                bool caughtMouse = false;

                while (!isGameOver && steps < GameConfig.MAX_GAME_STEPS)
                {
                    listPreviousPosition.Add(new Tuple<int, int>(currentX, currentY));

                    // Choose the action with the highest Q-value (no exploration)
                    NextAction? nextAction = GameHelper.NextAction( listPreviousPosition, currentX, currentY, isGreedyOnly);

                    if (nextAction == null)
                    {
                        tbl[currentX, currentY] = "⭙";
                        Console.WriteLine($"Evaluation {episode + 1}\tStep {steps + 1}\tGOT STUCK");
                        break;
                    }
                    else
                    {
                        // Perform the action and get the new state
                        int newX = nextAction.X, newY = nextAction.Y;

                        // Calculate the reward for the new state
                        double currentReward = GameHelper.GetReward(env, newX, newY, ref caughtMouse);

                        reward += currentReward;

                        string epsilonInfo = string.Empty;
                        if (!isGreedyOnly)
                        {
                            epsilonInfo = $" - Ran/Dec.Epsilon {nextAction.RandomEpsilon}/{nextAction.DecayedEpsilon} {(nextAction.RandomEpsilon < nextAction.DecayedEpsilon ? "RANDOM" : "BEST")}";
                        }

                        Console.WriteLine($"Evaluation {episode + 1}\tStep {steps + 1}\t{currentX},{currentY} {nextAction.Action,-5} {newX},{newY}\t(Reward = {currentReward}) {epsilonInfo}");

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

                        // Update the current state
                        currentX = newX;
                        currentY = newY;

                        // Check if the game is over
                        isGameOver = GameHelper.IsGameOver(env, newX, newY);

                    }

                    steps++;
                }

                if (caughtMouse)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                ConsoleTableBuilder
                    .From(GameHelper.ConvertToListOfLists(tbl))
                    .ExportAndWriteLine();

                Console.WriteLine($"Evaluation {episode + 1}\t(Reward = {reward})");

                Console.ResetColor();

                totalReward += reward;    
            }

            double averageReward = (double)totalReward / numberOfEvalute;
            Console.WriteLine("=============================================================================================");
            Console.WriteLine($"Average reward over {numberOfEvalute} evaluations: {averageReward}");
            Console.WriteLine("=============================================================================================");
        }
    }
}