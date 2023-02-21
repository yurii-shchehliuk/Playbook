namespace Web.Domain.Entities
{
    public class Stats
    {
        public int Id { get; set; }
        public string[] Data { get; set; }
        public string BallPossession { get; set; }
        public string GoalAttempts { get; set; }
        public string ShotsOnGoal { get; set; }
        public string ShotsOffGoal { get; set; }
        public string BlockedShots { get; set; }
        public string FreeKicks { get; set; }
        public string CornerKicks { get; set; }
        public string Offsides { get; set; }
        public string ThrowIn { get; set; }
        public string GoalkeeperSaves { get; set; }
        public string Fouls { get; set; }
        public string YellowCards { get; set; }
        public string TotalPasses { get; set; }
        public string CompletedPasses { get; set; }
        public string Tackles { get; set; }
        public string Attacks { get; set; }
        public string DangerousAttacks { get; set; }
    }
}
