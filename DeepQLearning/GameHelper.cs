using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepQLearning
{
    public static class GameHelper
    {
        private static Random _rand = new Random();

        ////public static NextAction NextAction(List<Tuple<int, int>> prevPositions, int x, int y, bool greedyOnly = false)
        ////{
        ////    var blockAction = new List<AgentAction>();
        ////    if (x == 0)
        ////    {
        ////        blockAction.Add(AgentAction.LEFT);
        ////    }
        ////    if (y == 0)
        ////    {
        ////        blockAction.Add(AgentAction.UP);
        ////    }

        ////    if (x == GameConfig.ENV_SIZE - 1)
        ////    {
        ////        blockAction.Add(AgentAction.RIGHT);
        ////    }

        ////    if (y == GameConfig.ENV_SIZE - 1)
        ////    {
        ////        blockAction.Add(AgentAction.DOWN);
        ////    }

        ////    if (x == 0 && y == 0)
        ////    {
        ////        blockAction.Add(AgentAction.LEFT);
        ////        blockAction.Add(AgentAction.UP);
        ////    }

        ////    if (x == GameConfig.ENV_SIZE - 1 && y == 0)
        ////    {
        ////        blockAction.Add(AgentAction.UP);
        ////        blockAction.Add(AgentAction.RIGHT);
        ////    }

        ////    if (x == GameConfig.ENV_SIZE - 1 && y == GameConfig.ENV_SIZE - 1)
        ////    {
        ////        blockAction.Add(AgentAction.RIGHT);
        ////        blockAction.Add(AgentAction.DOWN);
        ////    }

        ////    if (x == 0 && y == GameConfig.ENV_SIZE - 1)
        ////    {
        ////        blockAction.Add(AgentAction.DOWN);
        ////        blockAction.Add(AgentAction.LEFT);
        ////    }

        ////    blockAction = blockAction.Distinct().ToList();
        ////    return TryChooseAction(blockAction, prevPositions, x, y, greedyOnly);
        ////}

        ////private static NextAction? TryChooseAction(List<AgentAction> blockedActions, List<Tuple<int, int>> prevPositions, int x, int y, bool greedyOnly = false)
        ////{
        ////    var availableAction = new List<AgentAction>();
        ////    foreach (AgentAction a in (AgentAction[])Enum.GetValues(typeof(AgentAction)))
        ////    {

        ////        if (blockedActions.Contains(a))
        ////        {
        ////            continue;
        ////        }
        ////        else
        ////        {
        ////            NextAction positionByMove = MoveAgent(x, y, a);
        ////            if (!prevPositions.Contains(new Tuple<int, int>(positionByMove.X, positionByMove.Y)))
        ////            {
        ////                availableAction.Add(a);
        ////            }
        ////        }
        ////    }

        ////    if (availableAction.Count() == 0)
        ////    {
        ////        return null;
        ////    }

        ////    if (greedyOnly)
        ////    {
        ////        // Choose the action with the highest Q-value
        ////        double maxQ = double.MinValue;
        ////        AgentAction? bestAction = null;
        ////        int newX = x, newY = y;

        ////        foreach (var a in availableAction)
        ////        {
        ////            NextAction futureMove = MoveAgent(x, y, a);
        ////            if (Q[x, y, (int)a] > maxQ)
        ////            {
        ////                maxQ = Q[x, y, (int)a];
        ////                bestAction = a;
        ////                newX = futureMove.X;
        ////                newY = futureMove.Y;
        ////            }
        ////        }

        ////        if (bestAction == null)
        ////        {
        ////            return null;
        ////        }

        ////        return new NextAction
        ////        {
        ////            Action = (AgentAction)bestAction,
        ////            X = newX,
        ////            Y = newY
        ////        };
        ////    }
        ////    else
        ////    {
        ////        // Epsilon-greedy strategy (explore or exploit)
        ////        var ranEpsilon = _rand.NextDouble();

        ////        //Calculate epsilon value based on decay rate
        ////        var epsilon = GetDecayedEpsilon(GameConfig.TOTAL_NBR_OF_PLAYED_GAMES);

        ////        if (ranEpsilon < epsilon)
        ////        {
        ////            int randomIndex = _rand.Next(0, availableAction.Count);
        ////            AgentAction randomAction = availableAction[randomIndex];
        ////            NextAction futureMove = MoveAgent(x, y, randomAction);

        ////            return new NextAction
        ////            {
        ////                Action = randomAction,
        ////                X = futureMove.X,
        ////                Y = futureMove.Y,
        ////                RandomEpsilon = ranEpsilon,
        ////                DecayedEpsilon = epsilon
        ////            };
        ////        }
        ////        else
        ////        {
        ////            // Exploit: Choose the action with the highest Q-value
        ////            double maxQ = double.MinValue;
        ////            AgentAction? bestAction = null;

        ////            var newX = x;
        ////            var newY = y;

        ////            foreach (var a in availableAction)
        ////            {
        ////                NextAction futureMove = MoveAgent(x, y, a);

        ////                if (Q[x, y, (int)a] > maxQ)
        ////                {
        ////                    maxQ = Q[x, y, (int)a];
        ////                    bestAction = a;
        ////                    newX = futureMove.X;
        ////                    newY = futureMove.Y;
        ////                }
        ////            }

        ////            if (bestAction == null)
        ////            {
        ////                return null;
        ////            }

        ////            return new NextAction
        ////            {
        ////                Action = (AgentAction)bestAction,
        ////                X = newX,
        ////                Y = newY,
        ////                RandomEpsilon = ranEpsilon,
        ////                DecayedEpsilon = epsilon
        ////            };
        ////        }
        ////    }
        ////}

        //public static double GetDecayedEpsilon(long totalPlayedGames)
        //{
        //    return Math.Max(GameConfig.MIN_EPSILON, (GameConfig.START_EPSILON - GameConfig.MIN_EPSILON) * Math.Exp(-GameConfig.DECAY_RATE * totalPlayedGames));
        //}

        public static NextAction MoveAgent(int x, int y, AgentAction action)
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

        //public static double GetReward(int[,] environment, int x, int y, ref bool caughtMouse)
        //{
        //    if (environment[x, y] == (int)Animal.MOUSE)
        //    {
        //        //Console.WriteLine("caught");
        //        // Cat caught the mouse
        //        caughtMouse = true;
        //        return 10.0;
        //    }
        //    else if (environment[x, y] == (int)Animal.DOG)
        //    {
        //        //Console.WriteLine("Cat caught by the dog");
        //        // Cat caught by the dog
        //        return -10.0;
        //    }
        //    else
        //    {
        //        //Console.WriteLine("Small negative reward for every step");
        //        // Small negative reward for every step
        //        return -0.1;
        //    }
        //}
    }
}
