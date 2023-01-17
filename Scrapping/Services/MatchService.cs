using Domain.Entities;
using Domain.Interfaces;
using Persistance;

namespace Scrapping.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>MSSQL</remarks>
    public class MatchService : IMatchService
    {
        private readonly AppDbContext _context;
        public MatchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Match> GetMatchById(string id)
        {
            return await _context.Matches.FindAsync(id);
        }

        public async Task SaveCollectionToDb(List<Match> matches)
        {
            await _context.Matches.AddRangeAsync(matches);
            _context.SaveChanges();
        }

        public async Task SaveItemToDb(Match match)
        {
            await _context.Matches.AddAsync(match);
            _context.SaveChanges();
        }
    }
}
