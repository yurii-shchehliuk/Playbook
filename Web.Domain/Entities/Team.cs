using PuppeteerSharp;
using System.Text.RegularExpressions;
using Web.Domain.Extentions;

namespace Web.Domain.Entities
{
    public class Team : BaseEntity
    {
        public Team()
        {

        }
        public Team(string id, string name, string nameFull)
        {
            Id = id;
            Name = name;
            NameFull = nameFull;
        }
        public string Name { get; set; }
        public string NameFull { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();

        public async Task<Team> ConfigTeam(IElementHandle element, string fullName = "")
        {
            var participantHomeId = (await element.GetPropertyAsync("href")).Convert().Split('/');
            var team = new Team(participantHomeId[5], participantHomeId[4], fullName);
            return team;
        }

    }
}