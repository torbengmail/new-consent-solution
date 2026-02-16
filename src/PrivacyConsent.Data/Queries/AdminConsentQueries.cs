using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class AdminConsentQueries : IAdminConsentQueries
{
    private readonly PrivacyDbContext _db;

    public AdminConsentQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<Consent>> GetConsents(IEnumerable<int> ownerIds)
    {
        return await _db.Consents
            .Where(c => ownerIds.Contains(c.OwnerId ?? 0))
            .Include(c => c.Expressions.Where(e => e.StatusId == 2)) // Published
            .OrderByDescending(c => c.ModifiedDate)
            .ToListAsync();
    }

    public async Task<Consent?> GetConsentById(int consentId)
    {
        return await _db.Consents
            .Where(c => c.Id == consentId)
            .FirstOrDefaultAsync();
    }

    public async Task<Consent> CreateConsent(Consent consent)
    {
        consent.CreatedDate = DateTime.UtcNow;
        consent.ModifiedDate = DateTime.UtcNow;
        _db.Consents.Add(consent);
        await _db.SaveChangesAsync();
        return consent;
    }

    public async Task<Consent?> UpdateConsent(int consentId, Consent updated)
    {
        var existing = await _db.Consents.FindAsync(consentId);
        if (existing == null) return null;

        existing.Name = updated.Name;
        existing.Description = updated.Description;
        existing.OwnerId = updated.OwnerId;
        existing.PurposeId = updated.PurposeId;
        existing.ConsentTypeId = updated.ConsentTypeId;
        existing.SpecialDataCategoryId = updated.SpecialDataCategoryId;
        existing.DataSourceId = updated.DataSourceId;
        existing.ProcessingTypeId = updated.ProcessingTypeId;
        existing.ProductId = updated.ProductId;
        existing.HideByDefault = updated.HideByDefault;
        existing.ParentConsentId = updated.ParentConsentId;
        existing.IsGroup = updated.IsGroup;
        existing.ExpirationDate = updated.ExpirationDate;
        existing.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> SoftDeleteConsent(int consentId)
    {
        var count = await _db.Consents
            .Where(c => c.Id == consentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.DeleteAt, DateTime.UtcNow)
                .SetProperty(c => c.ModifiedDate, DateTime.UtcNow));
        return count > 0;
    }

    public async Task<List<ConsentExpression>> GetExpressionsByConsentId(int consentId)
    {
        return await _db.ConsentExpressions
            .Where(ce => ce.ConsentId == consentId)
            .Include(ce => ce.Texts)
            .Include(ce => ce.TagList).ThenInclude(tl => tl.Tag)
            .OrderByDescending(ce => ce.ModifiedDate)
            .ToListAsync();
    }

    public async Task<ConsentExpression?> GetExpressionById(int expressionId)
    {
        return await _db.ConsentExpressions
            .Include(ce => ce.Texts)
            .Include(ce => ce.TagList).ThenInclude(tl => tl.Tag)
            .FirstOrDefaultAsync(ce => ce.Id == expressionId);
    }

    public async Task<ConsentExpression> CreateExpression(ConsentExpression expression)
    {
        expression.CreatedDate = DateTime.UtcNow;
        expression.ModifiedDate = DateTime.UtcNow;
        _db.ConsentExpressions.Add(expression);
        await _db.SaveChangesAsync();
        return expression;
    }

    public async Task<ConsentExpression?> UpdateExpression(int expressionId, ConsentExpression updated)
    {
        var existing = await _db.ConsentExpressions
            .Include(ce => ce.TagList)
            .FirstOrDefaultAsync(ce => ce.Id == expressionId);
        if (existing == null) return null;

        existing.Name = updated.Name;
        existing.Description = updated.Description;
        existing.StatusId = updated.StatusId;
        existing.IsDefault = updated.IsDefault;
        existing.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task SetExpressionTags(int expressionId, List<int> tagIds)
    {
        // Remove existing
        var existing = await _db.ConsentExpressionTagLists
            .Where(tl => tl.ConsentExpressionId == expressionId)
            .ToListAsync();
        _db.ConsentExpressionTagLists.RemoveRange(existing);

        // Add new
        foreach (var tagId in tagIds)
        {
            _db.ConsentExpressionTagLists.Add(new ConsentExpressionTagList
            {
                ConsentExpressionId = expressionId,
                ConsentExpressionTagId = tagId
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpsertExpressionText(int expressionId, string language,
        string title, string shortText, string longText)
    {
        var existing = await _db.ConsentExpressionTexts
            .FirstOrDefaultAsync(t => t.ConsentExpressionId == expressionId && t.Language == language);

        if (existing != null)
        {
            existing.Title = title;
            existing.ShortText = shortText;
            existing.LongText = longText;
        }
        else
        {
            _db.ConsentExpressionTexts.Add(new ConsentExpressionText
            {
                ConsentExpressionId = expressionId,
                Language = language,
                Title = title,
                ShortText = shortText,
                LongText = longText
            });
        }

        await _db.SaveChangesAsync();
    }

    // Tags
    public async Task<List<ConsentExpressionTag>> GetTags(IEnumerable<int> ownerIds)
    {
        return await _db.ConsentExpressionTags
            .Where(t => ownerIds.Contains(t.OwnerId ?? 0))
            .ToListAsync();
    }

    public async Task<ConsentExpressionTag?> GetTagById(int tagId) =>
        await _db.ConsentExpressionTags.FindAsync(tagId);

    public async Task<ConsentExpressionTag> CreateTag(string name, int ownerId)
    {
        var tag = new ConsentExpressionTag { Name = name, OwnerId = ownerId };
        _db.ConsentExpressionTags.Add(tag);
        await _db.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteTag(int tagId)
    {
        var count = await _db.ConsentExpressionTags
            .Where(t => t.Id == tagId)
            .ExecuteDeleteAsync();
        return count > 0;
    }
}
