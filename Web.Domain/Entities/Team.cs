namespace Domain.Entities
{
    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Match> Matches { get; set; }
    }
}