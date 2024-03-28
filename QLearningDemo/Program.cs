using ConsoleTableExt;
using System.Diagnostics;
using System.Threading;

namespace QLearningDemo
{
    class Program
    {
        const string QTABLE_MODEL_FILE = "..\\..\\..\\qtable.txt";

        // Define the game environment
        static int SIZE = 4;

        static int NUMBER_OF_ACTION = Enum.GetNames(typeof(AgentAction)).Length;

        // Initialize the Q-table
        static double[,,] Q = new double[SIZE, SIZE, NUMBER_OF_ACTION];

        // Define the Q-learning parameters
        static double LEARNING_RATE = 0.1;
        static double DISCOUNT_FACTOR = 0.9;

        // Epsilon-greedy strategy (explore or exploit)
        const double EPSILON = 0.1;

        const int NUMBER_OF_TRAIN_INSTANCE = 10;
        const int NUMBER_OF_EVALUATE = 50;

        static int MAX_STEPS = 10; // Maximum number of steps of a game

        const bool ENABLE_LOG_LEARNING = false;

        static bool GREEDLY_ONLY_MODE_LEARNING = false;
        static bool GREEDLY_ONLY_MODE_EVALUATE = true;
        
        const bool TRAIN_ONE_CASE = false; // for testing purpose
       
        private static int gamesCount = 0;
        private static Stopwatch stopwatch = new Stopwatch();

        /*
        {
            "Thuật ngữ": [
                {
                    "Tên": "Q-Value",
                    "Mô tả": "Số liệu ước lượng cho hành động tại môi trường hiện tại",
                    "Ví dụ": "Q(s,a) là giá trị dự đoán khi thực hiện hành động a tại trạng thái s"
                },
                {
                    "Tên": "Learning Rate (alpha)",
                    "Mô tả": "Điều chỉnh mức độ cập nhật giá trị Q sau mỗi bước học",
                    "Ví dụ": "Nếu alpha = 0.1, tác nhân chỉ học 10% từ thông tin mới nhận được"
                },
                {
                    "Tên": "Discount Factor (gamma)",
                    "Mô tả": "Quyết định tầm quan trọng của phần thưởng trong tương lai so với phần thưởng hiện tại khi tính toán giá trị Q-Value cho một trạng thái hay hành động nào đó",
                    "Ví dụ": "Nếu gamma = 0.9, tác nhân sẽ ưu tiên 90% phần thưởng tương lai và 10% phần thưởng hiện tại khi tính toán Q-Value. Gamma càng lớn (gần 1) thì các phần thưởng tương lai càng được đánh giá cao, tác nhân sẽ chú trọng nhiều hơn vào các lợi ích dài hạn."
                },
                {
                    "Tên": "Exploration-Exploitation",
                    "Mô tả": "Là một vấn đề cốt lõi trong Reinforcement Learning, liên quan đến việc cân bằng giữa khám phá (exploration) môi trường mới để thu thập thông tin và khai thác (exploitation) những kiến thức đã học được.",
                    "Ví dụ": "Sử dụng Epsilon-Greedy Policy - nếu Epsilon nhỏ, tác nhân sẽ ít khám phá môi trường mới và chủ yếu khai thác những hành động có giá trị Q-Value cao đã biết. Ngược lại, nếu Epsilon lớn, tác nhân sẽ thường xuyên thăm dò ngẫu nhiên các hành động khác thay vì khai thác theo Q-Value."
                },
                {
                    "Tên": "Discount Factor (gamma)",
                    "Mô tả": "Quyết định tầm quan trọng của phần thưởng trong tương lai so với phần thưởng hiện tại khi tính toán giá trị Q-Value cho một trạng thái hay hành động nào đó",
                    "Ví dụ": "Nếu gamma = 0.9, tác nhân sẽ ưu tiên 90% phần thưởng tương lai và 10% phần thưởng hiện tại khi tính toán Q-Value. Gamma càng lớn (gần 1) thì các phần thưởng tương lai càng được đánh giá cao, tác nhân sẽ chú trọng nhiều hơn vào các lợi ích dài hạn."
                },
                {
                    "Tên": "Policy",
                    "Mô tả": "Cách tác nhân chọn hành động trong mỗi trạng thái",
                    "Ví dụ": "Sử dụng Q(s,a) để chọn hành động tối ưu a cho trạng thái s"
                }
            ]
        }
        */
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Load the saved Q-table (if it exists)
            if (File.Exists(QTABLE_MODEL_FILE))
            {
                LoadQTable(QTABLE_MODEL_FILE);
            }

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (NUMBER_OF_EVALUATE > 0)
                        {
                            Console.WriteLine("\n=======================================================\n");
                            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
                            EvaluateAgent(NUMBER_OF_EVALUATE);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(15));
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    DisplayGamesPerMinute();
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            });

            stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < NUMBER_OF_TRAIN_INSTANCE; i++)
            {
                var t = Task.Run(() =>
                {
                    int checkpoint = 0;
                    while (true)
                    {
                        checkpoint++;

                        if (checkpoint % 1_000_000 == 0)
                        {
                            SaveQTable(QTABLE_MODEL_FILE);
                            checkpoint = 0;
                        }

                        var gameId = Guid.NewGuid();

                        gamesCount++;

                        //Console.WriteLine($"New game {DateTime.Now:HH:mm:ss fff}");

                        int[,] env = new int[SIZE, SIZE];

                        // Train the Q-learning agent if no saved Q-table is found
                        // Initialize the game environment
                        int catX = -1, catY = -1, mouseX = -1, mouseY = -1, dogX = -1, dogY = -1;

                        InitPositions(ref catX, ref catY, ref mouseX, ref mouseY, ref dogX, ref dogY);

                        // Reset the game environment
                        Array.Clear(env, 0, env.Length);

                        env[catX, catY] = (int)Animal.CAT;
                        env[mouseX, mouseY] = (int)Animal.MOUSE;
                        env[dogX, dogY] = (int)Animal.DOG;

                        if (ENABLE_LOG_LEARNING)
                        {
                            Console.WriteLine($"Initial CAT [{catX},{catY}] - MOUSE [{mouseX},{mouseY}] - DOG [{dogX},{dogY}] - GreedyOnly ({(GREEDLY_ONLY_MODE_EVALUATE ? "true" : "false")})");
                        }

                        int currentX = catX, currentY = catY;
                        bool isGameOver = false;
                        int steps = 0; // Keep track of the number of steps

                        string[,] tbl = new string[SIZE, SIZE];

                        if (ENABLE_LOG_LEARNING)
                        {
                            Console.WriteLine();

                            for (int i = 0; i < SIZE; i++)
                            {
                                for (int j = 0; j < SIZE; j++)
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
                        while (!isGameOver && steps < MAX_STEPS)
                        {
                            listPreviousPosition.Add(new Tuple<int, int>(currentX, currentY));

                            // Choose an action based on the current Q-values
                            MovedActionAndPositionEntry? movedAction = ChooseAction(listPreviousPosition, currentX, currentY, Q, GREEDLY_ONLY_MODE_LEARNING);

                            if (movedAction == null)
                            {
                                if (ENABLE_LOG_LEARNING)
                                {
                                    tbl[currentX, currentY] = "⭙";
                                    Console.WriteLine($"Game {gameId.ToString().Substring(0, 7)}\t\tStep {steps + 1}\t\tGOT STUCK");
                                }

                                break;
                            }
                            else
                            {
                                // Perform the action and get the new state
                                int newX = movedAction.X, newY = movedAction.Y;

                                // Calculate the reward for the new state
                                double reward = GetReward(newX, newY, env, ref caughtMouse);

                                if (ENABLE_LOG_LEARNING)
                                {
                                    var epsilonInfo = string.Empty;
                                    if (!GREEDLY_ONLY_MODE_LEARNING)
                                    {
                                        epsilonInfo = $"- Epsilon {movedAction.RandomEpsilon} {(movedAction.RandomEpsilon < EPSILON ? "RANDOM" : "BEST")}";
                                    }

                                    Console.WriteLine($"Game {gameId.ToString().Substring(0, 7)}\t\tStep {steps + 1}\t\t{currentX},{currentY} {movedAction.Action} {newX},{newY}\t(Reward = {reward}) {epsilonInfo}");

                                    var actionIcon = string.Empty;
                                    if (movedAction == null)
                                    {
                                        actionIcon = "⭙";
                                    }
                                    else
                                    {
                                        switch (movedAction.Action)
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
                                double maxQNew = GetMaxQ(newX, newY, Q);
                                double oldQ = Q[currentX, currentY, (int)movedAction.Action];
                                double newQ = oldQ + LEARNING_RATE * (reward + DISCOUNT_FACTOR * maxQNew - oldQ);
                                Q[currentX, currentX, (int)movedAction.Action] = newQ;

                                // Update the current state
                                currentX = newX;
                                currentY = newY;

                                // Check if the game is over
                                isGameOver = IsGameOver(newX, newY, env);
                            }

                            steps++;
                        }

                        if (ENABLE_LOG_LEARNING)
                        {
                            if (caughtMouse)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }

                            ConsoleTableBuilder
                                .From(ConvertToListOfLists(tbl))
                                .ExportAndWriteLine();

                            Console.ResetColor();

                            Console.WriteLine("DONE");
                        }

                    }
                });

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            Console.ReadLine();
        }

        static void DisplayGamesPerMinute()
        {
            stopwatch.Stop();
            double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;
            double gamesPerMinute = gamesCount / (elapsedTimeInSeconds / 60);

            Console.WriteLine();
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.WriteLine($"Games per minute: {gamesPerMinute}");
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.WriteLine();
            Console.WriteLine();

            gamesCount = 0;

            stopwatch.Reset();
            stopwatch.Start();
        }

        static Random rand = new Random();

        class MovedActionAndPositionEntry
        {
            public AgentAction Action { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public double RandomEpsilon { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prevPositions"></param>
        /// <param name="x">current X</param>
        /// <param name="y">current Y</param>
        /// <param name="Q"></param>
        /// <returns></returns>
        static MovedActionAndPositionEntry? ChooseAction(List<Tuple<int, int>> prevPositions, int x, int y, double[,,] Q, bool greedyOnly = false)
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

            if (x == SIZE - 1)
            {
                blockAction.Add(AgentAction.RIGHT);
            }

            if (y == SIZE - 1)
            {
                blockAction.Add(AgentAction.DOWN);
            }

            if (x == 0 && y == 0)
            {
                blockAction.Add(AgentAction.LEFT);
                blockAction.Add(AgentAction.UP);
            }

            if (x == SIZE - 1 && y == 0)
            {
                blockAction.Add(AgentAction.UP);
                blockAction.Add(AgentAction.RIGHT);
            }

            if (x == SIZE - 1 && y == SIZE - 1)
            {
                blockAction.Add(AgentAction.RIGHT);
                blockAction.Add(AgentAction.DOWN);
            }

            if (x == 0 && y == SIZE - 1)
            {
                blockAction.Add(AgentAction.DOWN);
                blockAction.Add(AgentAction.LEFT);
            }

            blockAction = blockAction.Distinct().ToList() ;
            var tmpAction = TryChooseAction(blockAction, prevPositions, x, y, Q, greedyOnly);

            if (tmpAction == null)
            {
                return null;
            }
            else
            {
                //prevPositions.Add(new Tuple<int, int>(tmpAction.X, tmpAction.Y));
                return new MovedActionAndPositionEntry
                {
                    Action = tmpAction.Action,
                    X = tmpAction.X,
                    Y = tmpAction.Y,
                    RandomEpsilon = tmpAction.RandomEpsilon
                };
            }
        }

        // Choose an action based on the current Q-values

        class ActionAndPositionEntry
        {
            public AgentAction Action { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public double RandomEpsilon { get; set; }
        }

        static ActionAndPositionEntry? TryChooseAction(List<AgentAction> blockedActions, List<Tuple<int, int>> prevPositions, int x, int y, double[,,] Q, bool greedyOnly = false)
        {
            var availableAction = new List<AgentAction>();
            foreach (AgentAction a in (AgentAction[])Enum.GetValues(typeof(AgentAction)))            
            {

                if (blockedActions.Contains(a))
                {
                    continue;

                }
                else
                {
                    FutureMove positionByMove = MoveAgent(x, y, a);
                    if (!prevPositions.Contains(new Tuple<int, int>(positionByMove.X, positionByMove.Y)))
                    {
                        availableAction.Add(a);
                    }
                }
            }

            if (availableAction.Count() == 0)
            {
                return null;
            }

            if (greedyOnly)
            {
                // Choose the action with the highest Q-value
                double maxQ = double.MinValue;
                AgentAction? bestAction = null;
                int newX = x, newY = y;

                foreach (var a in availableAction)
                {
                    FutureMove futureMove = MoveAgent(x, y, a);
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

                return new ActionAndPositionEntry
                {
                    Action = (AgentAction)bestAction,
                    X = newX,
                    Y = newY
                };
            }
            else
            {
                // Epsilon-greedy strategy (explore or exploit)
                var ranEpsilon = rand.NextDouble();
                if (ranEpsilon < EPSILON)
                {
                    int randomIndex = rand.Next(0, availableAction.Count);
                    AgentAction randomAction = availableAction[randomIndex];
                    FutureMove futureMove = MoveAgent(x, y, randomAction);
                    
                    return new ActionAndPositionEntry
                    {
                        Action = randomAction,
                        X = futureMove.X,
                        Y = futureMove.Y,
                        RandomEpsilon = ranEpsilon
                    };
                }
                else
                {
                    // Exploit: Choose the action with the highest Q-value
                    double maxQ = double.MinValue;
                    AgentAction? bestAction = null;

                    var newX = x;
                    var newY = y;

                    foreach (var a in availableAction)
                    {
                        FutureMove futureMove = MoveAgent(x, y, a);

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

                    return new ActionAndPositionEntry
                    {
                        Action = (AgentAction)bestAction,
                        X = newX,
                        Y = newY,
                        RandomEpsilon = ranEpsilon
                    };
                }
            }
        }

        // Perform the action and get the new state

        class FutureMove
        {
            public int X { get; set; }
            public int Y { get; set; }
            public AgentAction Action { get; set; }
        }

        static FutureMove MoveAgent(int x, int y, AgentAction action)
        {
            switch (action)
            {
                case AgentAction.LEFT: // Left
                    x = Math.Max(x - 1, 0);
                    break;
                case AgentAction.RIGHT: // Right
                    x = Math.Min(x + 1, SIZE - 1);
                    break;
                case AgentAction.UP: // Up
                    y = Math.Max(y - 1, 0);
                    break;
                case AgentAction.DOWN: // Down
                    y = Math.Min(y + 1, SIZE - 1);
                    break;
            }

            return new FutureMove
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
        /// <param name="env"></param>
        /// <param name="gotStuck"></param>
        /// <returns></returns>
        static double GetReward(int x, int y, int[,] env, ref bool caughtMouse)
        {
            if (env[x, y] == (int)Animal.MOUSE)
            {
                //Console.WriteLine("caught");
                // Cat caught the mouse
                caughtMouse = true;
                return 10.0;
            }
            else if (env[x, y] == (int)Animal.DOG)
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

        // Find the maximum Q-value for the new state
        static double GetMaxQ(int x, int y, double[,,] Q)
        {
            double maxQ = double.MinValue;

            for (int a = 0; a < NUMBER_OF_ACTION; a++)
            {
                maxQ = Math.Max(maxQ, Q[x, y, a]);
            }

            return maxQ;

        }
        // Check if the game is over
        static bool IsGameOver(int x, int y, int[,] env)
        {
            return env[x, y] == 2 || env[x, y] == 3;
        }

        // Evaluate the trained agent

        static void EvaluateAgent(int NUM_EVALUATIONS = 100)
        {
            int[,] env = new int[SIZE, SIZE];

            double totalReward = 0;

            for (int eval = 0; eval < NUM_EVALUATIONS; eval++)
            {
                Console.WriteLine("----------------------------");
                // Reset the game environment
                Array.Clear(env, 0, env.Length);

                int catX = -1, catY = -1, mouseX = -1, mouseY = -1, dogX = -1, dogY = -1;
                InitPositions(ref catX, ref catY, ref mouseX, ref mouseY, ref dogX, ref dogY);
                
                env[catX, catY] = (int)Animal.CAT;
                env[mouseX, mouseY] = (int)Animal.MOUSE;
                env[dogX, dogY] = (int)Animal.DOG;

                Console.WriteLine($"Initial CAT [{catX},{catY}] - MOUSE [{mouseX},{mouseY}] - DOG [{dogX},{dogY}] - GreedyOnly ({(GREEDLY_ONLY_MODE_EVALUATE ? "true" : "false")})");

                int currentX = catX, currentY = catY;
                bool isGameOver = false;
                double reward = 0;
                int steps = 0; // Keep track of the number of steps

                List<Tuple<int, int>> listPreviousPosition = new List<Tuple<int, int>>();

                Console.WriteLine();

                string[,] tbl = new string[SIZE, SIZE];

                for (int i = 0; i < SIZE; i++)
                {
                    for (int j = 0; j < SIZE; j++)
                    {
                        tbl[i, j] = $"{i},{j}";
                    }
                }

                tbl[catX, catY] = "C";
                tbl[mouseX, mouseY] = "M";
                tbl[dogX, dogY] = "D";

                bool caughtMouse = false;

                while (!isGameOver && steps < MAX_STEPS)
                {
                    listPreviousPosition.Add(new Tuple<int, int>(currentX, currentY));
                    // Choose the action with the highest Q-value (no exploration)
                    MovedActionAndPositionEntry? movedAction = ChooseAction(listPreviousPosition, currentX, currentY, Q, GREEDLY_ONLY_MODE_EVALUATE);

                    if (movedAction == null)
                    {
                        tbl[currentX, currentY] = "⭙";
                        Console.WriteLine($"Evaluation {eval + 1}\t\tStep {steps + 1}\t\tGOT STUCK");
                        break;
                    }
                    else
                    {
                        // Perform the action and get the new state
                        int newX = movedAction.X, newY = movedAction.Y;

                        // Calculate the reward for the new state
                        double currentReward = GetReward(newX, newY, env, ref caughtMouse);

                        reward += currentReward;

                        var epsilonInfo = string.Empty;
                        if (!GREEDLY_ONLY_MODE_EVALUATE)
                        {
                            epsilonInfo = $"- Epsilon {movedAction.RandomEpsilon} {(movedAction.RandomEpsilon < EPSILON ? "RANDOM" : "BEST")}";
                        }

                        Console.WriteLine($"Evaluation {eval + 1}\t\tStep {steps + 1}\t\t{currentX},{currentY} {movedAction.Action} {newX},{newY}\t(Reward = {currentReward}) {epsilonInfo}");
                        
                        var actionIcon = string.Empty;
                        if (movedAction == null)
                        {
                            actionIcon = "⭙";
                        }
                        else
                        {
                            switch (movedAction.Action)
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
                        isGameOver = IsGameOver(newX, newY, env);

                    }

                    steps++;
                }

                if (caughtMouse)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                
                ConsoleTableBuilder
                    .From(ConvertToListOfLists(tbl))
                    .ExportAndWriteLine();

                Console.WriteLine($"Evaluation {eval + 1}\t\t(Reward = {reward})");

                Console.ResetColor();

                totalReward += reward;
                Console.WriteLine("DONE------------------------------------");
            }

            double averageReward = (double)totalReward / NUM_EVALUATIONS;

            Console.WriteLine("=============================================================================================");
            Console.WriteLine($"Average reward over {NUM_EVALUATIONS} evaluations: {averageReward}");
            Console.WriteLine("=============================================================================================");
        }

        private static readonly Random rnd = new Random();
        private static ulong Get64BitRandom(ulong minValue, ulong maxValue)
        {
            // Get a random array of 8 bytes. 
            // As an option, you could also use the cryptography namespace stuff to generate a random byte[8]
            byte[] buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0) % (maxValue - minValue + 1) + minValue;
        }

        static object fileLock = new object(); // Object used for locking
        static void SaveQTable(string fileName)
        {
            lock (fileLock)
            {
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    for (int x = 0; x < SIZE; x++)
                    {
                        for (int y = 0; y < SIZE; y++)
                        {
                            for (int a = 0; a < NUMBER_OF_ACTION; a++)
                            {
                                writer.WriteLine(Q[x, y, a]);
                            }
                        }
                    }
                }
                Console.WriteLine("Saved Q table");
            }
        }

        static void LoadQTable(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string line = reader.ReadToEnd();
                    string[] values = line.Split('\n');

                    int index = 0;
                    for (int x = 0; x < SIZE; x++)
                    {
                        for (int y = 0; y < SIZE; y++)
                        {
                            for (int a = 0; a < NUMBER_OF_ACTION; a++)
                            {
                                Q[x, y, a] = double.Parse(values[index++]);
                            }
                        }
                    }
                }

                Console.WriteLine("Loaded Q table");
            }
        }

        static List<List<object>> ConvertToListOfLists(string[,] array2D)
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

        private static void InitPositions(ref int catX, ref int catY, ref int mouseX, ref int mouseY, ref int dogX, ref int dogY)
        {
            if (TRAIN_ONE_CASE)
            {
                // test case
                catX = 0;
                catY = 0;
                mouseX = SIZE - 1;
                mouseY = SIZE - 1;
                dogX = SIZE - 2;
                dogY = SIZE - 2;
            }
            else
            {
                do
                {
                    catX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    catY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    mouseX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    mouseY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    dogX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    dogY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                } while ((catX == mouseX && catY == mouseY) || (catX == dogX && catY == dogY) || (mouseX == dogX && mouseY == dogY));
            }
        }
    }
}