using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Match : BaseEntity
    {
        public string Title { get; set; }
        public string Result { get; set; }
        public string Date { get; set; }
        public string THomeId { get; set; }
        public string TGuestId { get; set; }
        public string RoundNr { get; set; }
        public List<string> Incidents { get; set; } = new List<string>();
        public List<string> Summary { get; set; } = new List<string>();
        public List<string> Stats0 { get; set; } = new List<string>();
        public List<string> Stats1 { get; set; } = new List<string>();
        public List<string> Stats2 { get; set; } = new List<string>();
        public PositionalReport AttackSides { get; set; }
        public PositionalReport ShotDirections { get; set; }
        public PositionalReport ShotZones { get; set; }
        public PositionalReport ActionZones { get; set; }
    }

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
    public class PositionalReport
    {
        public int Id { get; set; }
        public string Left { get; set; }
        public string Middle { get; set; }
        public string Right { get; set; }
    }

    public class Incidents
    {
        public string FirstHalf { get; set; }
        public string SecondHalf { get; set; }
        public Stats Participants { get; set; }
    }
}
