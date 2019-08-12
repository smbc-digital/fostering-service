using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering.Application;

namespace fostering_service.Services.Application
{
    public interface IApplicationService
    {
        Task UpdateStatus(string caseId, ETaskStatus status, EFosteringApplicationForm form);

        Task<ETaskStatus> UpdateGpDetails(FosteringCaseGpDetailsUpdateModel model);

        Task<ETaskStatus> UpdateReferences(FosteringCaseReferenceUpdateModel model);

        Task<ETaskStatus> UpdateAddressHistory(FosteringCaseAddressHistoryUpdateModel model);
    }
}
