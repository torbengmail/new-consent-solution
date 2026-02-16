using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class MasterIdQueries : IMasterIdQueries
{
    private readonly PrivacyDbContext _db;

    public MasterIdQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<MasterId?> GetOrCreateMasterId(string userId, int idTypeId)
    {
        var existing = await _db.MasterIds
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IdTypeId == idTypeId);

        if (existing != null)
            return existing;

        var masterId = new MasterId
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IdTypeId = idTypeId
        };

        _db.MasterIds.Add(masterId);
        await _db.SaveChangesAsync();
        return masterId;
    }

    public async Task<MasterId?> GetMasterId(string userId, int idTypeId)
    {
        return await _db.MasterIds
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IdTypeId == idTypeId);
    }

    public async Task<IdType> CreateIdType(string name)
    {
        var idType = new IdType { Name = name };
        _db.IdTypes.Add(idType);
        await _db.SaveChangesAsync();
        return idType;
    }

    public async Task<IdMap> CreateIdMapping(int idTypeId, string name)
    {
        var idMap = new IdMap { IdTypeId = idTypeId, Name = name };
        _db.IdMaps.Add(idMap);
        await _db.SaveChangesAsync();
        return idMap;
    }

    public async Task<int> DeleteTestUser(string userId, int idTypeId)
    {
        var master = await _db.MasterIds
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IdTypeId == idTypeId);

        if (master == null)
            return 0;

        var raCount = await _db.RequestAttempts
            .Where(ra => ra.MasterId == master.Id)
            .ExecuteDeleteAsync();

        var ucCount = await _db.UserConsents
            .Where(uc => uc.MasterId == master.Id)
            .ExecuteDeleteAsync();

        return raCount + ucCount;
    }
}
