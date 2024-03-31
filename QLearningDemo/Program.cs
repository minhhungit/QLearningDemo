using System.Diagnostics;
using System.Text.Json;
using System.Xml;

namespace QLearningDemo
{
    partial class Program
    {
        static Random rand = new Random();

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

            GameInstance? game = new GameInstance();

            while (true)
            {
                env.Reset();

                // random remove a dog
                if (env.DogPositions.Count() > 4)
                {
                    var canRemove = rand.Next(2);

                    if (canRemove == 1)
                    {
                        var ranIndex = rand.Next(env.DogPositions.Count());

                        var dogToRemove = env.DogPositions[ranIndex];

                        env.RemoveDog(dogToRemove.X, dogToRemove.Y);

                        Console.WriteLine($"Removed DOG at [{dogToRemove.X},{dogToRemove.Y}]");
                    }
                }
                

                var newDog = env.AddDog();
                if (newDog != null)
                {
                    Console.WriteLine($"Added DOG [{newDog.Value.x},{newDog.Value.y}] into game");
                }
                else
                {
                    Console.WriteLine("CAN NOT ADD MORE DOG DUE TO NO FREE SLOT");
                }

                //var json = JsonSerializer.Serialize(env);
                //Console.WriteLine(json);
                Console.WriteLine("Please press any key again to train");
                Console.ReadKey();

                for (int episode = 0; episode < GameConfig.NUM_OF_EPISODES; episode++)
                {
                    game.Run(env, episode,isEvaluate: false, enableLog: false);
                }

                // evalute
                var evaluteNumber = 1;
                double totalReward = 0;
                for (int i = 0; i < evaluteNumber; i++)
                {
                    var episodeReward = game.Run(env, episode: 1, isEvaluate: true, enableLog: true);
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
