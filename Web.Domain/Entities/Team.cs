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
        public Team(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public string Name { get; set; }
        public int GoalsPerFirst { get; set; }
        public int GoalsPerSecond{ get; set; }

        public async Task<Team> ConfigTeam(IElementHandle element)
        {
            var participantHomeId = (await element.GetPropertyAsync("href")).Convert().Split('/');
            var team = new Team(participantHomeId[5], participantHomeId[4]);
            return team;
        }

    }
}