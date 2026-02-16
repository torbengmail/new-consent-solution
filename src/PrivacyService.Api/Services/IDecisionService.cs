namespace PrivacyService.Api.Services;

public interface IDecisionService
{
    Task<List<long>> SaveDecisions(List<DecisionInput> decisions, string userId, int idTypeId);
}
