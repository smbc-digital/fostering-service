using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;

namespace fostering_service.Services
{
    public interface IFosteringService
    {
        Task<FosteringCase> GetCase(string caseId);

        Task UpdateStatus(string caseId, ETaskStatus status, EFosteringCaseForm form);

        Task<ETaskStatus> UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model);

        Task UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model);

        Task<ETaskStatus> UpdateLanguagesSpokenInYourHome(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model);
        
        Task<ETaskStatus> UpdatePartnershipStatus(FosteringCasePartnershipStatusUpdateModel model);

        Task<ETaskStatus> UpdateYourFosteringHistory(FosteringCaseYourFosteringHistoryUpdateModel model);
    }
}
