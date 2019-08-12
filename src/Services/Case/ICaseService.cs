using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service.Services.Case
{
    public interface ICaseService
    {
        Task<FosteringCase> GetCase(string caseId);

        List<CouncillorRelationshipDetails> CreateCouncillorRelationshipDetailsList(List<CustomField> formFields);
    }
}
