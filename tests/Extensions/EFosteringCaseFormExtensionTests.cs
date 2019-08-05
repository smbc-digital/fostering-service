using fostering_service.Extensions;
using StockportGovUK.NetStandard.Models.Enums;
using Xunit;

namespace fostering_service_tests.Extensions
{
    public class EFosteringCaseFormExtensionTests
    {

        [Theory]
        [InlineData("childrenlivingawayfromyourhomestatus", EFosteringCaseForm.ChildrenLivingAwayFromYourHome)]
        [InlineData("languagespokeninyourhomestatus", EFosteringCaseForm.LanguageSpokenInYourHome)]
        [InlineData("tellusaboutyourinterestinfosteringstatus", EFosteringCaseForm.TellUsAboutYourInterestInFostering)]
        [InlineData("tellusaboutyourselfstatus", EFosteringCaseForm.TellUsAboutYourself)]
        [InlineData("youremploymentdetailsstatus", EFosteringCaseForm.YourEmploymentDetails)]
        [InlineData("yourfosteringhistorystatus", EFosteringCaseForm.YourFosteringHistory)]
        [InlineData("yourhealthstatus", EFosteringCaseForm.YourHealth)]
        [InlineData("yourhouseholdstatus", EFosteringCaseForm.YourHousehold)]
        [InlineData("yourpartnershipstatus", EFosteringCaseForm.YourPartnership)]
        [InlineData("yourreferencesstatus", EFosteringCaseForm.References)]
        [InlineData("gpdetailsstatus", EFosteringCaseForm.GpDetails)]
        public void GetFormStatusFieldName_ShouldReturnCorrectFieldNames(string expected, EFosteringCaseForm fosteringCaseForm )
        {
            Assert.Equal(expected, fosteringCaseForm.GetFormStatusFieldName());
        }
    }
}
