using Newtonsoft.Json;

namespace Web.Domain.Entities
{
    public class Stats
    {
        [JsonProperty("ExpectedGoals(xG)")]
        public float ExpectedGoals { get; set; } = 0;
        public int BallPossession { get; set; }
        public int GoalAttempts { get; set; }
        public int ShotsOnGoal { get; set; }
        public int ShotsOffGoal { get; set; }
        public int BlockedShots { get; set; }
        public int FreeKicks { get; set; }
        public int CornerKicks { get; set; }
        public int Offsides { get; set; }
        [JsonProperty("Throw-in")]
        public int ThrowIn { get; set; }
        public int GoalkeeperSaves { get; set; }
        public int Fouls { get; set; }
        public int YellowCards { get; set; }
        public int TotalPasses { get; set; }
        public int CompletedPasses { get; set; }
        public int Tackles { get; set; }
        public int Attacks { get; set; }
        public int DangerousAttacks { get; set; }
    }
}
