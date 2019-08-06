using fostering_service.Extensions;
using StockportGovUK.NetStandard.Models.Enums;
using Xunit;

namespace fostering_service_tests.Extensions
{
    public class EFosteringFormExtensionTests
    {

        [Theory]
        [InlineData("childrenlivingawayfromyourhomestatus", EFosteringHomeVisitForm.ChildrenLivingAwayFromYourHome)]
        [InlineData("languagespokeninyourhomestatus", EFosteringHomeVisitForm.LanguageSpokenInYourHome)]
        [InlineData("tellusaboutyourinterestinfosteringstatus", EFosteringHomeVisitForm.TellUsAboutYourInterestInFostering)]
        [InlineData("tellusaboutyourselfstatus", EFosteringHomeVisitForm.TellUsAboutYourself)]
        [InlineData("youremploymentdetailsstatus", EFosteringHomeVisitForm.YourEmploymentDetails)]
        [InlineData("yourfosteringhistorystatus", EFosteringHomeVisitForm.YourFosteringHistory)]
        [InlineData("yourhealthstatus", EFosteringHomeVisitForm.YourHealth)]
        [InlineData("yourhouseholdstatus", EFosteringHomeVisitForm.YourHousehold)]
        [InlineData("yourpartnershipstatus", EFosteringHomeVisitForm.YourPartnership)]
        public void GetFormStatusFieldName_ShouldReturnCorrectFieldNames(string expected, EFosteringHomeVisitForm fosteringHomeVisitForm)
        {
            Assert.Equal(expected, fosteringHomeVisitForm.GetFormStatusFieldName());
        }

        [Theory]
        [InlineData("yourreferencesstatus", EFosteringApplicationForm.References)]
        [InlineData("gpdetailsstatus", EFosteringApplicationForm.GpDetails)]
        public void GetFormStatusFieldName_ShouldReturnCorectFieldNames(string expected,
            EFosteringApplicationForm fosteringApplicationForm)
        {
            Assert.Equal(expected, fosteringApplicationForm.GetFormStatusFieldName());
        }
    }
}
