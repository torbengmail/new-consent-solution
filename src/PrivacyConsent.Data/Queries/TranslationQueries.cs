using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class TranslationQueries : ITranslationQueries
{
    private readonly PrivacyDbContext _db;

    public TranslationQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, string>> GetLanguageTranslations(
        string langCode, int ownerId, int? productId)
    {
        var query = _db.AdminTranslations
            .Where(at => at.LangCode == langCode && at.OwnerId == ownerId);

        if (productId.HasValue)
            query = query.Where(at => at.ProductId == productId.Value);
        else
            query = query.Where(at => at.ProductId == null);

        var translation = await query.FirstOrDefaultAsync();
        if (translation?.AugmentedTranslations == null)
            return new Dictionary<string, string>();

        return FlattenTranslationJson(translation.AugmentedTranslations);
    }

    public async Task<List<TranslationWithOwnerRow>> GetLanguageTranslationsMultiOwners(
        string langCode, IEnumerable<int> ownerIds, string textType)
    {
        var ownerList = ownerIds.ToList();

        return await (
            from at in _db.AdminTranslations
            join o in _db.Owners on at.OwnerId equals o.Id
            where at.LangCode == langCode
                  && ownerList.Contains(at.OwnerId ?? 0)
                  && at.ProductId == null
            select new TranslationWithOwnerRow
            {
                OwnerId = o.Id,
                OwnerName = o.Name,
                Translations = at.AugmentedTranslations
            }
        ).ToListAsync();
    }

    public async Task<List<AdminTranslation>> GetTranslations(int ownerId, int? productId)
    {
        var query = _db.AdminTranslations
            .Where(at => at.OwnerId == ownerId);

        if (productId.HasValue)
            query = query.Where(at => at.ProductId == productId.Value);
        else
            query = query.Where(at => at.ProductId == null);

        return await query.ToListAsync();
    }

    public async Task UpsertAdminTranslations(int ownerId, int? productId, string langCode,
        Dictionary<string, Dictionary<string, Dictionary<string, string>>> texts)
    {
        var existing = await _db.AdminTranslations
            .FirstOrDefaultAsync(at =>
                at.OwnerId == ownerId
                && at.ProductId == productId
                && at.LangCode == langCode);

        var translationsJson = JsonSerializer.Serialize(texts);

        // Build augmented (flattened) translations
        var augmented = new Dictionary<string, string>();
        foreach (var (page, fields) in texts)
        {
            foreach (var (field, translations) in fields)
            {
                foreach (var (key, value) in translations)
                {
                    augmented[$"{page}.{field}.{key}"] = value;
                }
            }
        }
        var augmentedJson = JsonSerializer.Serialize(augmented);

        if (existing != null)
        {
            existing.Translations = translationsJson;
            existing.AugmentedTranslations = augmentedJson;
        }
        else
        {
            _db.AdminTranslations.Add(new AdminTranslation
            {
                LangCode = langCode,
                OwnerId = ownerId,
                ProductId = productId,
                Translations = translationsJson,
                AugmentedTranslations = augmentedJson
            });
        }

        await _db.SaveChangesAsync();
    }

    public static Dictionary<string, string> FlattenTranslationJson(string json)
    {
        var result = new Dictionary<string, string>();
        var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (doc == null) return result;

        foreach (var (key, value) in doc)
        {
            if (value.ValueKind == JsonValueKind.String)
            {
                result[key] = value.GetString() ?? "";
            }
            else if (value.ValueKind == JsonValueKind.Object)
            {
                foreach (var inner in value.EnumerateObject())
                {
                    result[inner.Name] = inner.Value.ValueKind == JsonValueKind.String
                        ? inner.Value.GetString() ?? ""
                        : inner.Value.GetRawText();
                }
            }
        }

        return result;
    }

}

public record TranslationWithOwnerRow
{
    public int OwnerId { get; init; }
    public string OwnerName { get; init; } = "";
    public string? Translations { get; init; }
}
