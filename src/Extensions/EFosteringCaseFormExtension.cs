using StockportGovUK.NetStandard.Models.Enums;

namespace fostering_service.Extensions
{
    public static class EFosteringCaseFormExtension
    {
        public static string GetFormStatusFieldName(this EFosteringCaseForm value)
        {
            switch (value)
            {
                case EFosteringCaseForm.ChildrenLivingAwayFromYourHome:
                    return "childrenlivingawayfromyourhomestatus";
                case EFosteringCaseForm.LanguageSpokenInYourHome:
                    return "languagespokeninyourhomestatus";
                case EFosteringCaseForm.TellUsAboutYourInterestInFostering:
                    return "tellusaboutyourinterestinfosteringstatus";
                case EFosteringCaseForm.TellUsAboutYourself:
                    return "tellusaboutyourselfstatus";
                case EFosteringCaseForm.YourEmploymentDetails:
                    return "youremploymentdetailsstatus";
                case EFosteringCaseForm.YourFosteringHistory:
                    return "yourfosteringhistorystatus";
                case EFosteringCaseForm.YourHealth:
                    return "yourhealthstatus";
                case EFosteringCaseForm.YourHousehold:
                    return "yourhouseholdstatus";
                case EFosteringCaseForm.YourPartnership:
                    return "yourpartnershipstatus";
                case EFosteringCaseForm.References:
                    return "yourreferencesstatus";
                case EFosteringCaseForm.GpDetails:
                    return "gpdetailsstatus";
                default:
                    return null;
            }
        }
    }
}
