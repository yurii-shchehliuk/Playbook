using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Domain.Entities
{
    public class MyLegue
    {
        public int Id { get; set; }
        public MatchLegue Legue { get; set; }

    }
    public enum MatchLegue
    {
        PremierLegue,
        Legue1,
        Bundesilga,
        SerieA,
        Eredivise,
        LaLiga,
        Euro,
        ChampionsLegue,
        EuropaLegue,
        EuropaConferenceLeague,
        UEFANationsLeague,
        WorldChampionship
    }
}
