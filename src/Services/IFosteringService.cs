using System.Collections.Generic;
using System.Threading.Tasks;
using fostering_service.Builder;
using fostering_service.Models;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service.Services
{
    public interface IFosteringService
    {
        Task<FosteringCase> GetCase(string caseId);

        Task UpdateStatus(string caseId, ETaskStatus status, EFosteringCaseForm form);

        Task<ETaskStatus> UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model);

        Task<ETaskStatus> UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model);

        Task<ETaskStatus> UpdateLanguagesSpokenInYourHome(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model);
        
        Task<ETaskStatus> UpdatePartnershipStatus(FosteringCasePartnershipStatusUpdateModel model);

        Task<ETaskStatus> UpdateYourFosteringHistory(FosteringCaseYourFosteringHistoryUpdateModel model);

        Task<ETaskStatus> UpdateHealthStatus(FosteringCaseHealthUpdateModel model);
        
        Task<ETaskStatus> UpdateInterestInFostering(FosteringCaseInterestInFosteringUpdateModel model);

        Task<ETaskStatus> UpdateHousehold(FosteringCaseHouseholdUpdateModel model);

        List<OtherPerson> CreateOtherPersonList(OtherPeopleConfigurationModel config, List<CustomField> formFields, int capacity = 8);

        FormFieldBuilder CreateOtherPersonBuilder(OtherPeopleConfigurationModel config, List<OtherPerson> otherPeople);
    }
}
