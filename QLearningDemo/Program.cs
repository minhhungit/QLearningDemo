using System.Diagnostics;

namespace QLearningDemo
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Load the saved Q-table (if it exists)
            //GameHelper.LoadQTable();

            Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine($"HIGHEST SCORE {GameConfig.MAX_REWARD} AT {GameConfig.MAX_REWARD_AT}");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            });

            Console.WriteLine("Traning...");
            var game = new GameInstance();

            for (int episode = 0; episode < GameConfig.NUM_OF_EPISODES; episode++)
            {
                game.TrainAgent(episode, enableLog: false);
            }

            // 7.299999999999999 is good reward and can be stop training

            // save Q-Table
            //GameHelper.SaveQTable();

            Console.WriteLine("...");
            Console.ReadKey();
        }        
    }
}