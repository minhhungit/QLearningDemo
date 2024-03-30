using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace QLearningDemo
{
    public class GameHelper
    {
        static Random _rand = new Random();

        public static void InitPositions(ref int catX, ref int catY, ref int mouseX, ref int mouseY, ref int dogX, ref int dogY)
        {
            if (!GameConfig.TRAIN_ONE_CASE)
            {
                var rand = new Random();

                do
                {
                    catX = rand.Next(GameConfig.ENV_SIZE);
                    catY = rand.Next(GameConfig.ENV_SIZE);
                    mouseX = rand.Next(GameConfig.ENV_SIZE);
                    mouseY = rand.Next(GameConfig.ENV_SIZE);
                    dogX = rand.Next(GameConfig.ENV_SIZE);
                    dogY = rand.Next(GameConfig.ENV_SIZE);
                } while ((catX == mouseX && catY == mouseY) || (catX == dogX && catY == dogY) || (mouseX == dogX && mouseY == dogY));
            }
            else
            {
                catX = 0;
                catY = 0;
                mouseX = GameConfig.ENV_SIZE - 1;
                mouseY = GameConfig.ENV_SIZE - 1;
                dogX = GameConfig.ENV_SIZE - 2;
                dogY = GameConfig.ENV_SIZE - 2;
            }
        }

        public static List<List<object>> ConvertToListOfLists(string[,] array2D)
        {
            List<List<object>> listOfLists = new List<List<object>>();

            for (int i = 0; i < array2D.GetLength(0); i++)
            {
                List<object> innerList = new List<object>();
                for (int j = 0; j < array2D.GetLength(1); j++)
                {
                    innerList.Add(array2D[j, i]);
                }
                listOfLists.Add(innerList);
            }

            return listOfLists;
        }

        public static void DisplayGamesPerMinute(long nbrOfGames, double totalSeconds)
        {
            Console.WriteLine($"Completed {nbrOfGames} games in {totalSeconds} seconds");

            double gamesPerMinute = (nbrOfGames / (totalSeconds / 60));
            if (gamesPerMinute < 0 )
            {

            }
            Console.WriteLine("\n\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.WriteLine($"Games per minute: {gamesPerMinute}");
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n\n");
        }

        public static void SaveGameHistory(int catX, int catY, int mouseX, int mouseY, int dogX, int dogY, double reward)
        {
            return;
            var folder = $"..\\..\\..\\Histories";
            var fileName = $"{catX}{catY}-{mouseX}{mouseY}-{dogX}{dogY}.txt";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.AppendAllText(Path.Combine(folder, fileName), $"{reward}\n");
        }

        public static void SaveHighScoreLog(StringBuilder str)
        {
            var file = $"..\\..\\..\\high-score-log.txt";
            File.AppendAllText(file, str.ToString());
        }

        static object fileLock = new object(); // Object used for locking
        public static void SaveQTable()
        {
            lock (fileLock)
            {
                // backup
                if (File.Exists(GameConfig.QTABLE_MODEL_FILE) && !string.IsNullOrWhiteSpace(File.ReadAllText(GameConfig.QTABLE_MODEL_FILE)))
                {
                    File.Copy(GameConfig.QTABLE_MODEL_FILE, $"{GameConfig.QTABLE_MODEL_FILE}.bak", true);
                }

                // save
                using (StreamWriter writer = new StreamWriter(GameConfig.QTABLE_MODEL_FILE))
                {
                    writer.WriteLine(GameConfig.TOTAL_NBR_OF_PLAYED_GAMES);
                    writer.WriteLine($"{GameConfig.MAX_REWARD},{GameConfig.MAX_REWARD_AT}");

                    writer.WriteLine("---");

                    for (int x = 0; x < GameConfig.ENV_SIZE; x++)
                    {
                        for (int y = 0; y < GameConfig.ENV_SIZE; y++)
                        {
                            for (int a = 0; a < GameConfig.NUMBER_OF_ACTION; a++)
                            {
                                writer.WriteLine(GameConfig.Q[x, y, a]);
                            }
                        }
                    }
                }
                Console.WriteLine("\n\n==========================");
                Console.WriteLine("Saved Q table");
                Console.WriteLine("==========================\n\n");
            }
        }

        public static void LoadQTable()
        {
            if (File.Exists(GameConfig.QTABLE_MODEL_FILE))
            {
                using (StreamReader reader = new StreamReader(GameConfig.QTABLE_MODEL_FILE))
                {
                    string line = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        return;
                    }

                    string[] values = line.Split('\n');

                    // first line is number of played games
                    GameConfig.TOTAL_NBR_OF_PLAYED_GAMES = long.Parse(values[0]);

                    GameConfig.MAX_REWARD = double.Parse(values[1].Split(",")[0]);
                    GameConfig.MAX_REWARD_AT = long.Parse(values[1].Split(",")[1]);

                    // values[2] is "---"// below are Q-table values

                    int index = 3;
                    for (int x = 0; x < GameConfig.ENV_SIZE; x++)
                    {
                        for (int y = 0; y < GameConfig.ENV_SIZE; y++)
                        {
                            for (int a = 0; a < GameConfig.NUMBER_OF_ACTION; a++)
                            {
                                GameConfig.Q[x, y, a] = double.Parse(values[index++]);
                            }
                        }
                    }
                }

                Console.WriteLine("\n\n==========================");
                Console.WriteLine("Loaded Q table");
                Console.WriteLine("==========================\n\n");
            }
        }

        static object avgRewardFileLock = new object(); // Object used for locking
        public static void SaveAvgRewardPoints(AvgRewardPoint point)
        {
            return;
            lock (avgRewardFileLock)
            {
                // Check if the file exists
                if (File.Exists(GameConfig.AVG_REWARD_FILE))
                {
                    // Open the file in read mode
                    using (StreamReader reader = new StreamReader(GameConfig.AVG_REWARD_FILE))
                    {
                        // Check if the file has content
                        if (reader.Peek() >= 0)
                        {
                            // Read the first character of the file
                            //char firstChar = (char)reader.Read();
                            File.Copy(GameConfig.AVG_REWARD_FILE, $"{GameConfig.AVG_REWARD_FILE}.bak", true);
                        }
                        else
                        {
                            //Console.WriteLine("File is empty.");
                            File.AppendAllText(GameConfig.AVG_REWARD_FILE, $"{nameof(AvgRewardPoint.AvgReward)},{nameof(AvgRewardPoint.Episode)}\n");
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("File does not exist.");
                    File.AppendAllText(GameConfig.AVG_REWARD_FILE, $"{nameof(AvgRewardPoint.AvgReward)},{nameof(AvgRewardPoint.Episode)}\n");
                }


                // save
                File.AppendAllText(GameConfig.AVG_REWARD_FILE, $"{point.AvgReward},{point.Episode}\n");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("!!! Got converged and saved file");
                Console.ResetColor();
            }
        }

        public static bool IsGameOver(int[,] env, int x, int y)
        {
            return env[x, y] == (int)Animal.MOUSE || env[x, y] == (int)Animal.DOG;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prevPositions"></param>
        /// <param name="x">current X</param>
        /// <param name="y">current Y</param>
        /// <param name="Q"></param>
        /// <returns></returns>
        public static NextAction? NextAction(List<Tuple<int, int>> prevPositions, int x, int y, bool greedyOnly = false)
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
            return TryChooseAction(blockAction, prevPositions, x, y, GameConfig.Q, greedyOnly);
        }

        private static NextAction? TryChooseAction(List<AgentAction> blockedActions, List<Tuple<int, int>> prevPositions, int x, int y, double[,,] Q, bool greedyOnly = false)
        {
            var availableActions = new List<AgentAction>();
            foreach (AgentAction a in (AgentAction[])Enum.GetValues(typeof(AgentAction)))
            {

                if (blockedActions.Contains(a))
                {
                    continue;
                }
                else
                {
                    NextAction positionByMove = MoveAgent(x, y, a);
                    if (!prevPositions.Contains(new Tuple<int, int>(positionByMove.X, positionByMove.Y)))
                    {
                        availableActions.Add(a);
                    }
                }
            }

            if (availableActions.Count() == 0)
            {
                return null;
            }

            if (greedyOnly)
            {
                // Choose the action with the highest Q-value
                double maxQ = double.MinValue;
                AgentAction? bestAction = null;
                int newX = x, newY = y;

                foreach (var a in availableActions)
                {
                    NextAction futureMove = MoveAgent(x, y, a);
                    if (Q[x, y, (int)a] > maxQ)
                    {
                        maxQ = Q[x, y, (int)a];
                        bestAction = a;
                        newX = futureMove.X;
                        newY = futureMove.Y;
                    }
                }

                if (bestAction == null)
                {
                    return null;
                }

                return new NextAction
                {
                    Action = (AgentAction)bestAction,
                    X = newX,
                    Y = newY
                };
            }
            else
            {
                // Epsilon-greedy strategy (explore or exploit)
                var ranEpsilon = _rand.NextDouble();

                //Calculate epsilon value based on decay rate
                var epsilon = GetDecayedEpsilon(GameConfig.TOTAL_NBR_OF_PLAYED_GAMES);

                if (ranEpsilon < epsilon)
                {
                    int randomIndex = _rand.Next(0, availableActions.Count);
                    AgentAction randomAction = availableActions[randomIndex];
                    NextAction futureMove = MoveAgent(x, y, randomAction);

                    return new NextAction
                    {
                        Action = randomAction,
                        X = futureMove.X,
                        Y = futureMove.Y,
                        RandomEpsilon = ranEpsilon,
                        DecayedEpsilon = epsilon
                    };
                }
                else
                {
                    // Exploit: Choose the action with the highest Q-value
                    double maxQ = double.MinValue;
                    AgentAction? bestAction = null;

                    var newX = x;
                    var newY = y;

                    foreach (var a in availableActions)
                    {
                        NextAction futureMove = MoveAgent(x, y, a);

                        if (Q[x, y, (int)a] > maxQ)
                        {
                            maxQ = Q[x, y, (int)a];
                            bestAction = a;
                            newX = futureMove.X;
                            newY = futureMove.Y;
                        }
                    }

                    if (bestAction == null)
                    {
                        return null;
                    }

                    return new NextAction
                    {
                        Action = (AgentAction)bestAction,
                        X = newX,
                        Y = newY,
                        RandomEpsilon = ranEpsilon,
                        DecayedEpsilon = epsilon
                    };
                }
            }
        }

        private static NextAction MoveAgent(int x, int y, AgentAction action)
        {
            switch (action)
            {
                case AgentAction.LEFT: // Left
                    x = Math.Max(x - 1, 0);
                    break;
                case AgentAction.RIGHT: // Right
                    x = Math.Min(x + 1, GameConfig.ENV_SIZE - 1);
                    break;
                case AgentAction.UP: // Up
                    y = Math.Max(y - 1, 0);
                    break;
                case AgentAction.DOWN: // Down
                    y = Math.Min(y + 1, GameConfig.ENV_SIZE - 1);
                    break;
            }

            return new NextAction
            {
                Action = action,
                X = x,
                Y = y
            };
        }

        // Calculate the reward for the new state
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">next X</param>
        /// <param name="y">next Y</param>
        /// <param name="environment"></param>
        /// <param name="gotStuck"></param>
        /// <returns></returns>
        public static double GetReward(int [,] environment, int x, int y, ref bool caughtMouse)
        {
            if (environment[x, y] == (int)Animal.MOUSE)
            {
                //Console.WriteLine("caught");
                // Cat caught the mouse
                caughtMouse = true;
                return 10.0;
            }
            else if (environment[x, y] == (int)Animal.DOG)
            {
                //Console.WriteLine("Cat caught by the dog");
                // Cat caught by the dog
                return -10.0;
            }
            else
            {
                //Console.WriteLine("Small negative reward for every step");
                // Small negative reward for every step
                return -0.1;
            }
        }

        public static double GetDecayedEpsilon(long totalPlayedGames)
        {
            return Math.Max(GameConfig.MIN_EPSILON, (GameConfig.START_EPSILON - GameConfig.MIN_EPSILON) * Math.Exp(-GameConfig.DECAY_RATE * totalPlayedGames));
        }

        public static double CalculateMaxQValue()
        {
            double maxChange = 0;

            // Calculate maximum change in Q-values
            for (int i = 0; i < GameConfig.ENV_SIZE; i++)
            {
                for (int j = 0; j < GameConfig.ENV_SIZE; j++)
                {
                    for (int a = 0; a < GameConfig.NUMBER_OF_ACTION; a++)
                    {
                        // Calculate the change in Q-value for this state-action pair
                        double qValueChange = Math.Abs(GameConfig.Q[i, j, a]);

                        // Update maxChange if necessary
                        if (qValueChange > maxChange)
                        {
                            maxChange = qValueChange;
                        }
                    }
                }
            }

            return maxChange;
        }

        /// <summary>
        /// preMaxValue = GameConfig.Q[i, j, a]
        /// </summary>
        /// <param name="preMaxValue"></param>
        /// <returns></returns>
        public static double CalculateMaxQValueChange(double [,,] prevQValues)
        {
            double maxChange = 0;

            // Calculate maximum change in Q-values
            for (int i = 0; i < GameConfig.ENV_SIZE; i++)
            {
                for (int j = 0; j < GameConfig.ENV_SIZE; j++)
                {
                    for (int a = 0; a < GameConfig.NUMBER_OF_ACTION; a++)
                    {
                        // Store the previous Q-value for comparison
                        double prevQValue = prevQValues[i, j, a];


                        // Calculate the change in Q-value for this state-action pair
                        double qValueChange = Math.Abs(GameConfig.Q[i, j, a] - prevQValue);

                        // Update maxChange if necessary
                        if (qValueChange > maxChange)
                        {
                            maxChange = qValueChange;
                        }
                    }
                }
            }

            return maxChange;
        }
    }
}