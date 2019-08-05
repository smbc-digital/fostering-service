using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;

namespace fostering_service.Services.Application
{
    public interface IApplicationService
    {
        Task<ETaskStatus> UpdateGpDetails(FosteringCaseGpDetailsUpdateModel model);

        Task<ETaskStatus> UpdateReferences(FosteringCaseReferenceUpdateModel model);
    }
}
