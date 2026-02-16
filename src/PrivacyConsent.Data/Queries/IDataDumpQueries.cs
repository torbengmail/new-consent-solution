namespace PrivacyConsent.Data.Queries;

public interface IDataDumpQueries
{
    Task<List<DecisionDataRecord>> GetUserDecisionDataRecords(string userId, int idTypeId);
    Task<List<RequestAttemptDataRecord>> GetUserRequestAttemptDataRecords(string userId, int idTypeId);
}
