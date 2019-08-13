using System;
using System.Linq;
using fostering_service.Helpers;
using fostering_service_tests.Builders;
using Xunit;

namespace fostering_service_tests.Helpers
{
    public class CaseHelperTests
    {
        private readonly CaseHelper _helper = new CaseHelper();

        [Fact]
        public void CreateAddressHistoryList_ShouldMapSingleAddressWhenFieldsAreNotNull()
        {
            // Arrange
            var addressLine1 = "line1";
            var addressLine2 = "line2";
            var addressLineTown = "town";
            var addressLineCounty = "county";
            var addressLineCountry = "country";
            var addressLinePostcode = "pstcode";
            var year = "2010";
            var month = "01";

            var date = new DateTime(int.Parse(year), int.Parse(month), 01);

            var builder = new CaseBuilder()
                .WithIntegrationFormField("pa1applicant1", $"{addressLine1}|{addressLine2}|{addressLineTown}|{addressLineCounty}|{addressLineCountry}")
                .WithIntegrationFormField("pa1postcodeapplicant1", addressLinePostcode)
                .WithIntegrationFormField("pa1datefrommonthapplicant1", month)
                .WithIntegrationFormField("pa1datefromyearapplicant1", year)
                .Build();

            // Act
            var result = _helper.CreateAddressHistoryList(builder.IntegrationFormFields);

            // Assert
            Assert.Single(result);
            Assert.Equal(addressLine1, result.FirstOrDefault().Address.AddressLine1);
            Assert.Equal(addressLine2, result.FirstOrDefault().Address.AddressLine2);
            Assert.Equal(addressLineTown, result.FirstOrDefault().Address.Town);
            Assert.Equal(addressLineCounty, result.FirstOrDefault().Address.County);
            Assert.Equal(addressLineCountry, result.FirstOrDefault().Address.Country);
            Assert.Equal(addressLinePostcode, result.FirstOrDefault().Address.Postcode);
            Assert.Equal(addressLinePostcode, result.FirstOrDefault().Address.Postcode);
            Assert.Equal(date, result.FirstOrDefault().DateFrom);
        }

        [Fact]
        public void CreateAddressHistoryList_ShouldMapMultipleAddressWhenFieldsAreNotNull()
        {
            // Arrange
            var address1Line1 = "add1-line1";
            var address1Line2 = "add1-line2";
            var address1LineTown = "add1-town";
            var address1LineCounty = "add1-county";
            var address1LineCountry = "add1-country";
            var address1LinePostcode = "add1-postcode";
            var year1 = "2015";
            var month1 = "08";

            var address2Line1 = "add2-line1";
            var address2Line2 = "add2-line2";
            var address2LineTown = "add2-town";
            var address2LineCounty = "add2-county";
            var address2LineCountry = "add2-country";
            var address2LinePostcode = "add2-pstcode";
            var year2 = "2010";
            var month2 = "01";

            var date1 = new DateTime(int.Parse(year1), int.Parse(month1), 01);
            var date2 = new DateTime(int.Parse(year2), int.Parse(month2), 01);

            var builder = new CaseBuilder()
                .WithIntegrationFormField("pa1applicant1", $"{address1Line1}|{address1Line2}|{address1LineTown}|{address1LineCounty}|{address1LineCountry}")
                .WithIntegrationFormField("pa1postcodeapplicant1", address1LinePostcode)
                .WithIntegrationFormField("pa1datefrommonthapplicant1", month1)
                .WithIntegrationFormField("pa1datefromyearapplicant1", year1)
                .WithIntegrationFormField("pa2applicant1", $"{address2Line1}|{address2Line2}|{address2LineTown}|{address2LineCounty}|{address2LineCountry}")
                .WithIntegrationFormField("pa2postcodeapplicant1", address2LinePostcode)
                .WithIntegrationFormField("pa2datefrommonthapplicant1", month2)
                .WithIntegrationFormField("pa2datefromyearapplicant1", year2)
                .Build();

            // Act
            var result = _helper.CreateAddressHistoryList(builder.IntegrationFormFields);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(address1Line1, result.FirstOrDefault().Address.AddressLine1);
            Assert.Equal(address1Line2, result.FirstOrDefault().Address.AddressLine2);
            Assert.Equal(address1LineTown, result.FirstOrDefault().Address.Town);
            Assert.Equal(address1LineCounty, result.FirstOrDefault().Address.County);
            Assert.Equal(address1LineCountry, result.FirstOrDefault().Address.Country);
            Assert.Equal(address1LinePostcode, result.FirstOrDefault().Address.Postcode);
            Assert.Equal(address1LinePostcode, result.FirstOrDefault().Address.Postcode);
            Assert.Equal(date1, result.FirstOrDefault().DateFrom);

            Assert.Equal(address2Line1, result[1].Address.AddressLine1);
            Assert.Equal(address2Line2, result[1].Address.AddressLine2);
            Assert.Equal(address2LineTown, result[1].Address.Town);
            Assert.Equal(address2LineCounty, result[1].Address.County);
            Assert.Equal(address2LineCountry, result[1].Address.Country);
            Assert.Equal(address2LinePostcode, result[1].Address.Postcode);
            Assert.Equal(address2LinePostcode, result[1].Address.Postcode);
            Assert.Equal(date2, result[1].DateFrom);
        }

        [Fact]
        public void CreateAddressHistoryList_ShouldMapSingleAddressForSecondApplicantsWhenFieldsAreNotNull()
        {
            // Arrange
            var addressLine1 = "line1";
            var addressLine2 = "line2";
            var addressLineTown = "town";
            var addressLineCounty = "county";
            var addressLineCountry = "country";
            var addressLinePostcode = "pstcode";
            var year = "2010";
            var month = "01";

            var date = new DateTime(int.Parse(year), int.Parse(month), 01);

            var builder = new CaseBuilder()
                .WithIntegrationFormField("pa1applicant2", $"{addressLine1}|{addressLine2}|{addressLineTown}|{addressLineCounty}|{addressLineCountry}")
                .WithIntegrationFormField("pa1postcodeapplicant2", addressLinePostcode)
                .WithIntegrationFormField("pa1datefrommonthapplicant2", month)
                .WithIntegrationFormField("pa1datefromyearapplicant2", year)
                .Build();

            // Act
            var result = _helper.CreateAddressHistoryList(builder.IntegrationFormFields, true);

            // Assert
            Assert.Single(result);
            Assert.Equal(addressLine1, result.FirstOrDefault().Address.AddressLine1);
            Assert.Equal(addressLine2, result.FirstOrDefault().Address.AddressLine2);
            Assert.Equal(addressLineTown, result.FirstOrDefault().Address.Town);
            Assert.Equal(addressLineCounty, result.FirstOrDefault().Address.County);
            Assert.Equal(addressLineCountry, result.FirstOrDefault().Address.Country);
            Assert.Equal(addressLinePostcode, result.FirstOrDefault().Address.Postcode);
            Assert.Equal(addressLinePostcode, result.FirstOrDefault().Address.Postcode);
            Assert.Equal(date, result.FirstOrDefault().DateFrom);
        }

        [Fact]
        public void CreateAddressHistoryList_ShouldMapSingleAdditionalInformationWhenFieldsAreNotNull()
        {
            // Arrange
            var additional = "Line1|Line2|Town|County|Country|Postcode|3|2016";

            var date = new DateTime(int.Parse("2016"), int.Parse("3"), 01);

            var builder = new CaseBuilder()
                .WithIntegrationFormField("addressadditionalinformation1", $"{additional}")
                .Build();

            // Act
            var result = _helper.CreateAddressHistoryList(builder.IntegrationFormFields);

            // Assert
            Assert.Single(result);
            Assert.Equal("Line1", result.FirstOrDefault().Address.AddressLine1);
            Assert.Equal("Line2", result.FirstOrDefault().Address.AddressLine2);
            Assert.Equal("Town", result.FirstOrDefault().Address.Town);
            Assert.Equal("County", result.FirstOrDefault().Address.County);
            Assert.Equal("Country", result.FirstOrDefault().Address.Country);
            Assert.Equal("Postcode", result.FirstOrDefault().Address.Postcode);
            Assert.Equal(date, result.FirstOrDefault().DateFrom);
        }

        [Fact]
        public void CreateAddressHistoryList_ShouldMapMultipleAdditionalInformationWhenFieldsAreNotNull()
        {
            // Arrange
            var additional = "Line1|Line2|Town|County|Country|Postcode|3|2016%2Line1|2Line2|2Town|2County|2Country|2Postcode|6|2010";

            var date1 = new DateTime(int.Parse("2016"), int.Parse("3"), 01);
            var date2 = new DateTime(int.Parse("2010"), int.Parse("6"), 01);

            var builder = new CaseBuilder()
                .WithIntegrationFormField("addressadditionalinformation1", $"{additional}")
                .Build();

            // Act
            var result = _helper.CreateAddressHistoryList(builder.IntegrationFormFields);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Line1", result.FirstOrDefault().Address.AddressLine1);
            Assert.Equal("Line2", result.FirstOrDefault().Address.AddressLine2);
            Assert.Equal("Town", result.FirstOrDefault().Address.Town);
            Assert.Equal("County", result.FirstOrDefault().Address.County);
            Assert.Equal("Country", result.FirstOrDefault().Address.Country);
            Assert.Equal("Postcode", result.FirstOrDefault().Address.Postcode);
            Assert.Equal(date1, result.FirstOrDefault().DateFrom);
            Assert.Equal("2Line1", result[1].Address.AddressLine1);
            Assert.Equal("2Line2", result[1].Address.AddressLine2);
            Assert.Equal("2Town", result[1].Address.Town);
            Assert.Equal("2County", result[1].Address.County);
            Assert.Equal("2Country", result[1].Address.Country);
            Assert.Equal("2Postcode", result[1].Address.Postcode);
            Assert.Equal(date2, result[1].DateFrom);
        }
    }
}
