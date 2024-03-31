using System.Xml.Linq;

namespace QLearningDemo
{
    public class CharacterPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class GameEnvironemnt
    {
        public int[,] Env { get; set; }

        public CharacterPosition CatPosition { get; set; }
        public CharacterPosition MousePosition { get; set; }
        public List<CharacterPosition> DogPositions { get; set; }

        Random rand;

        public GameEnvironemnt()
        {
            rand = new Random();

            Env = new int[GameConfig.ENV_SIZE, GameConfig.ENV_SIZE];

            CatPosition = new CharacterPosition();
            MousePosition = new CharacterPosition();
            DogPositions = new List<CharacterPosition>();

            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < GameConfig.ENV_SIZE; i++)
            {
                for (int j = 0; j < GameConfig.ENV_SIZE; j++)
                {
                    if (Env[i, j] != (int)Animal.DOG)
                    {
                        Env[i, j] = 0;
                    }
                }
            }

            var blockedPositions = new List<string>();
            foreach (var dog in DogPositions)
            {
                blockedPositions.Add($"{dog.X},{dog.Y}");
            }

            // pick a slot for cat
            while (true)
            {
                var x = rand.Next(GameConfig.ENV_SIZE);
                var y = rand.Next(GameConfig.ENV_SIZE);

                if (blockedPositions.Contains($"{x},{y}"))
                {
                    continue;
                }
                else
                {
                    CatPosition.X = x;
                    CatPosition.Y = y;

                    blockedPositions.Add($"{x},{y}");
                    Env[CatPosition.X, CatPosition.Y] = (int)Animal.CAT;

                    break;
                }
            }

            // pick a slot for mouse
            while (true)
            {
                var x = rand.Next(GameConfig.ENV_SIZE);
                var y = rand.Next(GameConfig.ENV_SIZE);

                if (blockedPositions.Contains($"{x},{y}"))
                {
                    continue;
                }
                else
                {
                    MousePosition.X = x;
                    MousePosition.Y = y;

                    blockedPositions.Add($"{x},{y}");
                    Env[MousePosition.X, MousePosition.Y] = (int)Animal.MOUSE;
                    break;
                }
            }
        }

        public int CountNumberOfAnimals()
        {
            return DogPositions.Count() + 2;
        }
        
        public (int x, int y)? AddDog()
        {
            // check if enviroment still has free slot for dog
            if (DogPositions.Count() >= ((GameConfig.ENV_SIZE * GameConfig.ENV_SIZE) - 2))  // -2 is cat + mouse
            {
                return null;
            }

            int dogX;
            int dogY;

            do
            {
                dogX = rand.Next(GameConfig.ENV_SIZE);
                dogY = rand.Next(GameConfig.ENV_SIZE);
            } while ((CatPosition.X == dogX && CatPosition.Y == dogY) || (MousePosition.X == dogX && MousePosition.Y == dogY) || DogPositions.Any(d => d.X == dogX && d.Y == dogY));

            DogPositions.Add(new CharacterPosition
            {
                X = dogX,
                Y = dogY
            });

            Env[dogX, dogY] = (int)Animal.DOG;

            return (dogX, dogY);
        }

        public void RemoveDog(int x, int y)
        {
            var dogs = DogPositions.Where(d => d.X == x && d.Y == y).ToList();
            foreach (var d in dogs)
            {
                DogPositions.Remove(d);
                Env[d.X, d.Y] = 0;
            }
        }
    }
}