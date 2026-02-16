using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class DictionaryQueries : IDictionaryQueries
{
    private readonly PrivacyDbContext _db;

    public DictionaryQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<ConsentType>> GetConsentTypes() =>
        await _db.ConsentTypes.ToListAsync();

    public async Task<List<PurposeCategory>> GetPurposeCategories() =>
        await _db.PurposeCategories.ToListAsync();

    public async Task<List<ConsentExpressionTag>> GetExpressionTags() =>
        await _db.ConsentExpressionTags.ToListAsync();

    public async Task<List<ConsentExpressionStatus>> GetExpressionStatuses() =>
        await _db.ConsentExpressionStatuses.ToListAsync();

    public async Task<List<Language>> GetLanguages() =>
        await _db.Languages.ToListAsync();

    public async Task<List<IdType>> GetIdTypes() =>
        await _db.IdTypes.ToListAsync();

    public async Task<List<Owner>> GetOwners() =>
        await _db.Owners.ToListAsync();

    public async Task<List<Product>> GetProducts() =>
        await _db.Products.ToListAsync();

    public async Task<List<Owner>> GetOwnersWithProducts() =>
        await _db.Owners.Include(o => o.Products).ToListAsync();
}
