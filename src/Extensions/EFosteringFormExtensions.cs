using StockportGovUK.NetStandard.Models.Enums;

namespace fostering_service.Extensions
{
    public static class EFosteringFormExtensions
    {
        public static string GetFormStatusFieldName(this EFosteringHomeVisitForm value)
        {
            switch (value)
            {
                case EFosteringHomeVisitForm.ChildrenLivingAwayFromYourHome:
                    return "childrenlivingawayfromyourhomestatus";
                case EFosteringHomeVisitForm.LanguageSpokenInYourHome:
                    return "languagespokeninyourhomestatus";
                case EFosteringHomeVisitForm.TellUsAboutYourInterestInFostering:
                    return "tellusaboutyourinterestinfosteringstatus";
                case EFosteringHomeVisitForm.TellUsAboutYourself:
                    return "tellusaboutyourselfstatus";
                case EFosteringHomeVisitForm.YourEmploymentDetails:
                    return "youremploymentdetailsstatus";
                case EFosteringHomeVisitForm.YourFosteringHistory:
                    return "yourfosteringhistorystatus";
                case EFosteringHomeVisitForm.YourHealth:
                    return "yourhealthstatus";
                case EFosteringHomeVisitForm.YourHousehold:
                    return "yourhouseholdstatus";
                case EFosteringHomeVisitForm.YourPartnership:
                    return "yourpartnershipstatus";
                default:
                    return null;
            }
        }

        public static string GetFormStatusFieldName(this EFosteringApplicationForm value)
        {
            switch (value)
            {
                case EFosteringApplicationForm.References:
                    return "yourreferencesstatus";
                case EFosteringApplicationForm.GpDetails:
                    return "gpdetailsstatus";
                case EFosteringApplicationForm.CouncillorsOrEmployees:
                    return "councillorsoremployeesstatus";
                default:
                    return null;
            }
        }
    }
}
