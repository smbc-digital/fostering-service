using fostering_service.Builder;
using fostering_service.Controllers.Case.Models;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models;
using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Fostering.HomeVisit;

namespace fostering_service.Services.HomeVisit
{
    public interface IHomeVisitService
    {

        Task UpdateStatus(string caseId, ETaskStatus status, EFosteringHomeVisitForm form);

        Task<ETaskStatus> UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model);

        Task<ETaskStatus> UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model);

        Task<ETaskStatus> UpdateLanguagesSpokenInYourHome(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model);
        
        Task<ETaskStatus> UpdatePartnershipStatus(FosteringCasePartnershipStatusUpdateModel model);

        Task<ETaskStatus> UpdateYourFosteringHistory(FosteringCaseYourFosteringHistoryUpdateModel model);

        Task<ETaskStatus> UpdateHealthStatus(FosteringCaseHealthUpdateModel model);
        
        Task<ETaskStatus> UpdateInterestInFostering(FosteringCaseInterestInFosteringUpdateModel model);

        Task<ETaskStatus> UpdateHousehold(FosteringCaseHouseholdUpdateModel model);

        Task<ETaskStatus> UpdateChildrenLivingAwayFromHome(FosteringCaseChildrenLivingAwayFromHomeUpdateModel model);

        FormFieldBuilder CreateOtherPersonBuilder(OtherPeopleConfigurationModel config, List<OtherPerson> otherPeople, int capacity = 8);
    }
}
