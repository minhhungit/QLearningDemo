using System.Diagnostics;

namespace QLearningDemo
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // 2024/03/29 22:15 

            //// test decay
            //for (long i = 0; i < long.MaxValue; i++)
            //{
            //    if (i % 1000001 == 0)
            //    {
            //        Console.WriteLine($"{i} > {GameHelper.GetDecayedEpsilon(i)}");
            //    }
            //}
            //Console.ReadKey();

            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Load the saved Q-table (if it exists)
            GameHelper.LoadQTable();

            Console.WriteLine("Traning...");
            var game = new GameInstance();

            var currentEpisode = GameConfig.TOTAL_NBR_OF_PLAYED_GAMES;
            int tmpGameCountGamePerMinute = 0;
            int logSecondFlag = -1;
            int evaluteSecFlag = -1;

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                if (currentEpisode == long.MaxValue)
                {
                    break;
                }

                currentEpisode++;

                bool enableLog = false;

                var currentSecond = DateTime.Now.Second;
                if (currentSecond != logSecondFlag && currentSecond % 5 == 0)
                {
                    logSecondFlag = currentSecond;
                    enableLog = true;
                }

                game.TrainAgent(currentEpisode, enableLog: enableLog);
                GameConfig.TOTAL_NBR_OF_PLAYED_GAMES++;

                tmpGameCountGamePerMinute++;

                if (currentSecond != evaluteSecFlag && currentSecond % 20 == 0)
                {
                    // stop stopwatch
                    stopwatch.Stop();

                    evaluteSecFlag = currentSecond;

                    // save Q-Table
                    GameHelper.SaveQTable();

                    var elapsedTotalSeconds = stopwatch.Elapsed.TotalSeconds;

                    Console.WriteLine("\n\n============================================\n\n");
                    Console.WriteLine("Evaluating...");
                    new Evaluator().Run(GameConfig.NUMBER_OF_EVALUATE);
                    Console.WriteLine("Done Evaluation");

                    // show GpM
                    GameHelper.DisplayGamesPerMinute(tmpGameCountGamePerMinute, elapsedTotalSeconds);

                    // reset GpM game counter
                    tmpGameCountGamePerMinute = 0;
                    
                    // reset stopwatch
                    stopwatch.Restart();
                }
            }

            Console.WriteLine("Trained to max episode");
        }        
    }
}