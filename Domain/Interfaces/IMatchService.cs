using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IMatchService
    {
        Task<Match> GetMatchById(string id);
        Task SaveCollectionToDb(List<Match> matches);
        Task SaveItemToDb(Match match);
    }
}
