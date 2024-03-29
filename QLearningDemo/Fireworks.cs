using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLearningDemo
{
    internal class Fireworks
    {
        public static void Congratulation()
        {
            Console.WriteLine("Congratulations!");
            Console.WriteLine("**************");

            Console.ForegroundColor = ConsoleColor.Red; // Change color each iteration
            Console.WriteLine(@"         \     |     /");
            Console.WriteLine(@"        __\    |    /__");
            Console.WriteLine(@"       |___\   |   /___|");
            Console.WriteLine(@"           _|__|_");
            Console.WriteLine(@"          /       \");
            Console.WriteLine(@"         | .     . |");
            Console.WriteLine(@"         |   ...   |");
            Console.WriteLine(@"         |_________|");
            Console.WriteLine();

            // Reset console color
            Console.ResetColor();
        }
    }
}
