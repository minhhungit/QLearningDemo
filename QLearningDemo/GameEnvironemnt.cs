namespace QLearningDemo
{
    public class CharacterPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class GameEnvironemnt
    {
        public int[,] Env;

        public CharacterPosition CatPosition = new CharacterPosition();
        public CharacterPosition MousePosition = new CharacterPosition();
        public List<CharacterPosition> DogPositions = new List<CharacterPosition>();

        Random rand;

        public GameEnvironemnt()
        {
            rand = new Random();
            Reset();
        }

        public void Reset()
        {
            if (Env == null)
            {
                Env = new int[GameConfig.ENV_SIZE, GameConfig.ENV_SIZE];
            }
            else
            {
                for (int i = 0; i < GameConfig.ENV_SIZE; i++)
                {
                    for (int j = 0; j < GameConfig.ENV_SIZE; j++)
                    {
                        if (Env[i,j] != (int)Animal.DOG)
                        {
                            Env[i, j] = 0;
                        }
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
        
        public CharacterPosition? AddDog()
        {
            // check if enviroment still has free slot for dog
            if (DogPositions.Count() >= ((GameConfig.ENV_SIZE * GameConfig.ENV_SIZE) / 2)) // max(number dogs) = a half of grid
            {
                return null;
            }

            int dogX;
            int dogY;
            do
            {
                dogX = rand.Next(GameConfig.ENV_SIZE);
                dogY = rand.Next(GameConfig.ENV_SIZE);
            } while ((CatPosition.X == dogX && CatPosition.Y == dogY) || MousePosition.X == dogX && MousePosition.Y == dogY);

            var dog = new CharacterPosition();

            dog.X = dogX;
            dog.Y = dogY;

            DogPositions.Add(dog);

            Env[dogX, dogY] = (int)Animal.DOG;

            return new CharacterPosition
            {
                X = dogX,
                Y = dogY
            };       
        }
    }
}