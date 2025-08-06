using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class SQLWalkReository : IWalkRepository
    {
        private readonly NZWalksDbContext nZWalksDbContext;

        public SQLWalkReository(NZWalksDbContext nZWalksDbContext)
        {
            this.nZWalksDbContext = nZWalksDbContext;
        }
        public async Task<Walk> CreateAsync(Walk walk)
        {
            await nZWalksDbContext.Walks.AddAsync(walk);
            await nZWalksDbContext.SaveChangesAsync();
            return walk;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exisitngwalk = await nZWalksDbContext.Walks.FindAsync(id);
            if (exisitngwalk == null) { 
                return false;
            }
            nZWalksDbContext.Walks.Remove(exisitngwalk);
            await nZWalksDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Walk>> GetAllAsync(string? filterOn = null, string? filterQuery =null,
            string? sortBy = null, bool isAscending = true,int pageNumber =1, int pageSize=1000)
        {
            var walks = nZWalksDbContext.Walks.Include("Difficulty").Include("Region").AsQueryable();
            //Filtering
            if(!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("name", StringComparison.OrdinalIgnoreCase)) {
                    walks = walks.Where(x => x.Name.Contains(filterQuery));
                }    
            }
            //Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    walks = isAscending ? walks.OrderBy(x => x.Name) : walks.OrderByDescending(x => x.Name);
                }
                else if (sortBy.Equals("Length", StringComparison.OrdinalIgnoreCase))
                {
                    walks = isAscending ? walks.OrderBy(x => x.LengthInKm) : walks.OrderByDescending(x => x.LengthInKm);
                }
            }

            //Pagination
            var skipResults = (pageNumber - 1) * pageSize;

            return await walks.Skip(skipResults).Take(pageSize).ToListAsync();
            //return await nZWalksDbContext.Walks.Include("Difficulty").Include("Region").ToListAsync();
        }

        public async Task<Walk?> GetByIdAsync(Guid id)
        {
            return await nZWalksDbContext.Walks
                .Include(w => w.Difficulty)
                .Include(w => w.Region)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Walk?> UpdateAsync(Guid id, Walk walk)
        {
            var exisitngWalk = await nZWalksDbContext.Walks.FindAsync(id);
            if(exisitngWalk == null)
            {
                return null;
            }
            exisitngWalk.Name = walk.Name;
            exisitngWalk.Description = walk.Description;
            exisitngWalk.LengthInKm = walk.LengthInKm;
            exisitngWalk.WalkImageUrl = walk.WalkImageUrl;
            exisitngWalk.DifficultyId = walk.DifficultyId;
            exisitngWalk.RegionId = walk.RegionId;
            await nZWalksDbContext.SaveChangesAsync();
            return exisitngWalk;

        }
    }
}
