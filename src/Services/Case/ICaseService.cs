using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Verint;

namespace fostering_service.Services.Case
{
    public interface ICaseService
    {
        Task<FosteringCase> GetCase(string caseId);

        List<CouncillorRelationshipDetails> CreateCouncillorRelationshipDetailsList(List<CustomField> formFields, bool isSecondApplicant);
    }
}
