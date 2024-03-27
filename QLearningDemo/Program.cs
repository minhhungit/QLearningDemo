using ConsoleTableExt;

namespace QLearningDemo
{
    enum AgentAction
    {
        LEFT = 0,
        RIGHT = 1,
        UP = 2,
        DOWN = 3
    }
    
    enum Animal
    {
        CAT = 1,
        MOUSE = 2,
        DOG = 3
    }

    class Program
    {
        // Define the game environment
        static int SIZE = 4;

        static int NbrOfActions = Enum.GetNames(typeof(AgentAction)).Length;

        // Initialize the Q-table
        static double[,,] Q = new double[SIZE, SIZE, NbrOfActions];

        // Define the Q-learning parameters
        static double LEARNING_RATE = 0.1;
        static double DISCOUNT_FACTOR = 0.9;
        static int MAX_STEPS = 15; // Maximum number of steps per evaluation
        static int NBR_OF_TRAIN_INSTANCE = 0;
        static double REWARD_CAN_NOT_DO_ACTION = -10.0;
        static double REWARD_LAST_STEP_BUT_NOT_SEE_MOUSE = -10.0;

        static bool GREEDLY_ONLY_MODE = false;
        
        // Epsilon-greedy strategy (explore or exploit)
        const double EPSILON = 0.1;

        const string QTABLE_MODEL_FILE = "..\\..\\..\\qtable.txt";
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

            for (int i = 0; i < NBR_OF_TRAIN_INSTANCE; i++)
            {
                var t = Task.Run(() =>
                {
                    while (true)
                    {
                        int[,] env = new int[SIZE, SIZE];

                        // Train the Q-learning agent if no saved Q-table is found
                        // Initialize the game environment
                        int catX, catY, mouseX, mouseY, dogX, dogY;

                        // Reset the game environment
                        Array.Clear(env, 0, env.Length);

                        do
                        {
                            catX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                            catY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                            mouseX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                            mouseY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                            dogX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                            dogY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                        } while ((catX == mouseX && catY == mouseY) || (catX == dogX && catY == dogY) || (mouseX == dogX && mouseY == dogY));

                        env[catX, catY] = (int)Animal.CAT;
                        env[mouseX, mouseY] = (int)Animal.MOUSE;
                        env[dogX, dogY] = (int)Animal.DOG;

                        int currentX = catX, currentY = catY;
                        bool isGameOver = false;
                        int steps = 0; // Keep track of the number of steps

                        List<Tuple<int, int>> listPreviousPosition = new List<Tuple<int, int>>();

                        AgentAction? lastAction = null;

                        while (!isGameOver && steps < MAX_STEPS)
                        {
                            listPreviousPosition.Add(new Tuple<int, int>(currentX, currentY));

                            // Choose an action based on the current Q-values
                            MovedActionAndPositionEntry? movedAction = ChooseAction(listPreviousPosition, currentX, currentY, Q, greedyOnly: GREEDLY_ONLY_MODE);
                                                        
                            // Perform the action and get the new state
                            int newX = currentX, newY = currentY;

                            // Check if the game is over
                            isGameOver = IsGameOver(newX, newY, env);

                            // last step but not end game
                            var lastStepsButNotSeeMouse = !isGameOver && (steps + 1 == MAX_STEPS);

                            // Calculate the reward for the new state
                            double reward = GetReward(newX, newY, env, lastStepsButNotSeeMouse, movedAction == null);

                            if (movedAction == null)
                            {
                                // Update the Q-table
                                double maxQNew = GetMaxQ(newX, newY, Q);
                                double oldQ = Q[currentX, currentY, (int)lastAction];
                                double newQ = oldQ + LEARNING_RATE * (reward + DISCOUNT_FACTOR * maxQNew - oldQ);
                                Q[currentX, currentY, (int)lastAction] = newQ;
                                break;
                            }
                            else
                            {
                                // Update the Q-table
                                double maxQNew = GetMaxQ(newX, newY, Q);
                                double oldQ = Q[currentX, currentY, (int)movedAction.Action];
                                double newQ = oldQ + LEARNING_RATE * (reward + DISCOUNT_FACTOR * maxQNew - oldQ);
                                Q[currentX, currentY, (int)movedAction.Action] = newQ;

                                lastAction = movedAction.Action;
                            }

                            // Update the current state
                            currentX = newX;
                            currentY = newY;

                            steps++;
                        }
                    }
                });
            }

            Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine("\n=======================================================\n");
                    Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
                    SaveQTable(QTABLE_MODEL_FILE);
                    EvaluateAgent(100);

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            });

            Console.ReadLine();
        }

        static Random rand = new Random();

        class MovedActionAndPositionEntry
        {
            public AgentAction Action { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public double RandomEpsilon { get; set; }
        }

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
                prevPositions.Add(new Tuple<int, int>(tmpAction.X, tmpAction.Y));
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
        static double GetReward(int x, int y, int[,] env, bool lastStepsButNotSeeMouse, bool canNotDoAction)
        {
            if (canNotDoAction)
            {
                return REWARD_CAN_NOT_DO_ACTION;
            }

            if (lastStepsButNotSeeMouse)
            {
                return REWARD_LAST_STEP_BUT_NOT_SEE_MOUSE;
            }

            if (env[x, y] == (int)Animal.MOUSE)
            {
                //Console.WriteLine("caught");
                // Cat caught the mouse
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

            for (int a = 0; a < NbrOfActions; a++)
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
                int catX, catY, mouseX, mouseY, dogX, dogY;

                do
                {
                    catX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    catY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    mouseX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    mouseY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    dogX = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                    dogY = (int)Get64BitRandom(0, (ulong)SIZE - 1);
                } while ((catX == mouseX && catY == mouseY) || (catX == dogX && catY == dogY) || (mouseX == dogX && mouseY == dogY));

                // test
                //catX = 3;
                //catY = 3;

                //mouseX = 3;
                //mouseY = 0;

                //dogX = 1;
                //dogY = 3;

                env[catX, catY] = (int)Animal.CAT;
                env[mouseX, mouseY] = (int)Animal.MOUSE;
                env[dogX, dogY] = (int)Animal.DOG;

                Console.WriteLine($"Initial CAT [{catX},{catY}] - MOUSE [{mouseX},{mouseY}] - DOG [{dogX},{dogY}] - GreedyOnly ({(GREEDLY_ONLY_MODE ? "true" : "false")})");

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
                        tbl[i, j] = $"{j},{i}";
                    }
                }

                tbl[catX, catY] = "C";
                tbl[mouseX, mouseY] = "M";
                tbl[dogX, dogY] = "D";

                while (!isGameOver && steps < MAX_STEPS)
                {
                    listPreviousPosition.Add(new Tuple<int, int>(currentX, currentY));
                    // Choose the action with the highest Q-value (no exploration)
                    MovedActionAndPositionEntry? movedAction = ChooseAction(listPreviousPosition, currentX, currentY, Q, greedyOnly: GREEDLY_ONLY_MODE);

                    if (movedAction == null)
                    {
                        var currentReward = REWARD_CAN_NOT_DO_ACTION;
                        reward += currentReward;
                        tbl[currentX, currentY] = "⭙";
                        Console.WriteLine($"Evaluation {eval + 1}\t\tStep {steps + 1}\t\tGET STUCK\t\t-> (Reward = {REWARD_CAN_NOT_DO_ACTION})");
                        break;
                    }
                    else
                    {
                        // Perform the action and get the new state
                        int newX = movedAction.X, newY = movedAction.Y;

                        // Check if the game is over
                        isGameOver = IsGameOver(newX, newY, env);

                        // last step but not end game
                        var lastStepsButNotFoundMouse = !isGameOver && (steps + 1 == MAX_STEPS);

                        // Calculate the reward for the new state
                        int currentReward = (int)GetReward(newX, newY, env, lastStepsButNotFoundMouse, movedAction == null);

                        reward += currentReward;

                        Console.WriteLine($"Evaluation {eval + 1}\t\tStep {steps + 1}\t\t{currentX},{currentY} {movedAction.Action} {newX},{newY}\t(Reward = {currentReward}) - Epsilon {movedAction.RandomEpsilon} {(movedAction.RandomEpsilon < EPSILON ? "RANDOM" : "BEST")}");
                        
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
                    }

                    steps++;
                }

                // If the maximum number of steps is reached, consider the episode as failed
                if (steps == MAX_STEPS)
                {
                    reward = -10; // Assign a negative reward for failing the episode
                }

                ConsoleTableBuilder
                    .From(ConvertToListOfLists(tbl))
                    .ExportAndWriteLine();

                Console.WriteLine($"Evaluation {eval + 1}\t\t(Reward = {reward})");

                totalReward += reward;
                Console.WriteLine("DONE------------------------------------");
            }

            double averageReward = (double)totalReward / NUM_EVALUATIONS;
            Console.WriteLine($"Average reward over {NUM_EVALUATIONS} evaluations: {averageReward}");
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

        static void SaveQTable(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        for (int a = 0; a < NbrOfActions; a++)
                        {
                            writer.WriteLine(Q[x, y, a]);
                        }
                    }
                }
            }

            Console.WriteLine("Saved Q table");
        }

        static void LoadQTable(string fileName)
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
                        for (int a = 0; a < NbrOfActions; a++)
                        {
                            Q[x, y, a] = double.Parse(values[index++]);
                        }
                    }
                }
            }
            Console.WriteLine("Loaded Q table");
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
    }
}