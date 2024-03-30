
namespace QLearningDemo
{
    public static class GameConfig
    {
        public static long TOTAL_NBR_OF_PLAYED_GAMES = 0;
        public const string QTABLE_MODEL_FILE = "..\\..\\..\\qtable.txt";
        public const string AVG_REWARD_FILE = "..\\..\\..\\avgReward.csv";

        // Define the game environment
        public const int ENV_SIZE = 6;

        public static int NUMBER_OF_ACTION = Enum.GetNames(typeof(AgentAction)).Length;

        // Initialize the Q-table
        public static double[,,] Q = new double[ENV_SIZE, ENV_SIZE, NUMBER_OF_ACTION];

        // Define the Q-learning parameters
        public const double LEARNING_RATE = 0.7;// Alpha
        public const double DISCOUNT_FACTOR = 0.95;// Gamma

        // Epsilon-greedy strategy (explore or exploit)
        public const double START_EPSILON = 1.0; // start training by selecting purely random actions
        public const double MIN_EPSILON = 0.05;   // the lowest epsilon allowed to decay to
        public const double DECAY_RATE = 0.00000001;   // epsilon will gradually decay so we do less exploring and more exploiting as Q-function improves

        //static int NUMBER_OF_TRAIN_INSTANCE = Environment.ProcessorCount * 5;
        public static long NUM_OF_EPISODES = 10_000;
        public const int NUMBER_OF_EVALUATE = 10;

        public const int MAX_GAME_STEPS = int.MaxValue; // Maximum number of steps of a game

        public const bool GREEDLY_ONLY_MODE_LEARNING = false;
        public const bool GREEDLY_ONLY_MODE_EVALUATE = true;

        //public const double CONVERGENCE_THRESHOLD = -0.000001; // Convergence threshold
        //public static double PREV_MAX_CHANGE = double.MaxValue;

        public const bool TRAIN_ONE_CASE = true; // for testing purpose
        public static double MAX_REWARD = double.MinValue;
        public static long MAX_REWARD_AT = 0;

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
                    "Tên": "Policy",
                    "Mô tả": "Cách tác nhân chọn hành động trong mỗi trạng thái",
                    "Ví dụ": "Sử dụng Q(s,a) để chọn hành động tối ưu a cho trạng thái s"
                }
            ]
        }
        */
    }
}
