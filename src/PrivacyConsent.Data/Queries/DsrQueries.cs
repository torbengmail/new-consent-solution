using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class DsrQueries : IDsrQueries
{
    private readonly PrivacyDbContext _db;

    public DsrQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<DsrTracking>> GetDsrRequests(string userId, int idTypeId, string type)
    {
        // Use raw SQL to cast enum columns to text for comparison
        return await _db.DsrTrackings
            .FromSqlInterpolated($"""
                SELECT ticket_id, user_id, id_type_id, type::text AS type, status::text AS status,
                       created_date, updated_date
                FROM consent.dsr_tracking
                WHERE user_id = {userId} AND id_type_id = {idTypeId} AND type::text = {type}
                ORDER BY created_date DESC
                """)
            .ToListAsync();
    }

    public async Task<DsrTracking?> GetDsrRequest(string ticketId, string userId, int idTypeId, string type)
    {
        var results = await _db.DsrTrackings
            .FromSqlInterpolated($"""
                SELECT ticket_id, user_id, id_type_id, type::text AS type, status::text AS status,
                       created_date, updated_date
                FROM consent.dsr_tracking
                WHERE ticket_id = {ticketId} AND user_id = {userId}
                      AND id_type_id = {idTypeId} AND type::text = {type}
                LIMIT 1
                """)
            .ToListAsync();
        return results.FirstOrDefault();
    }

    public async Task CreateDsrRequest(string ticketId, string userId, int idTypeId, string type, string status)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO consent.dsr_tracking (ticket_id, user_id, id_type_id, type, status, created_date, updated_date)
            VALUES ({ticketId}, {userId}, {idTypeId}, {type}::type_dsr_processing, {status}::status_dsr_processing, NOW(), NOW())
            """);
    }

    public async Task UpdateDsrRequest(string ticketId, string userId, int idTypeId, string type, string status)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE consent.dsr_tracking
            SET status = {status}::status_dsr_processing, updated_date = NOW()
            WHERE ticket_id = {ticketId} AND user_id = {userId}
                  AND id_type_id = {idTypeId} AND type::text = {type}
            """);
    }
}
