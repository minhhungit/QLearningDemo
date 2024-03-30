using System.Diagnostics;
using System.Xml;

namespace QLearningDemo
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Load the saved Q-table (if it exists)
            //GameHelper.LoadQTable();

            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        Console.WriteLine($"HIGHEST SCORE {GameConfig.MAX_REWARD} AT {GameConfig.MAX_REWARD_AT}");
            //        Thread.Sleep(TimeSpan.FromSeconds(2));
            //    }
            //});

            GameEnvironemnt env = new GameEnvironemnt();
            
            Console.WriteLine("Traning...");
            GameInstance? game = new GameInstance();

            while (true)
            {
                env.Reset();

                var newDog = env.AddDog();
                if (newDog != null)
                {
                    Console.WriteLine($"Added DOG [{newDog.X},{newDog.Y}] into game");
                }
                else
                {
                    Console.WriteLine("CAN NOT ADD MORE DOG DUE TO NO FREE SLOT");
                }

                for (int episode = 0; episode < GameConfig.NUM_OF_EPISODES; episode++)
                {
                    game.Run(env, episode, false, enableLog: false);
                }

                // evalute
                var evaluteNumber = 100;
                double totalReward = 0;
                for (int i = 0; i < evaluteNumber; i++)
                {
                    var episodeReward = game.Run(env, 100, true, true);
                    totalReward += episodeReward;
                }

                Console.WriteLine("---------------------------------------------");
                Console.WriteLine($"Avg reward {totalReward / evaluteNumber} after {evaluteNumber} game");
                Console.WriteLine("---------------------------------------------");

                Console.WriteLine("\n\nPlease press any key to add more dog for higher game level ^^");
                Console.ReadKey();
            }

            Console.ReadKey();
        }        
    }
}