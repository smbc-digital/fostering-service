using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;

namespace fostering_service.Services
{
    public interface IFosteringService
    {
        Task<FosteringCase> GetCase(string caseId);

        Task UpdateStatus(string caseId, ETaskStatus status, EFosteringCaseForm form);

        Task UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model);

        Task UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model);
    }
}
