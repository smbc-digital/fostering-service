using System;
using System.Collections.Generic;
using System.Text;
using fostering_service.Mappers;
using fostering_service_tests.Builders;
using StockportGovUK.NetStandard.Models.Models.Verint;
using Xunit;

namespace fostering_service_tests.Mappers
{
    public class ReferenceDetailsMapperTests
    {
        [Fact]
        public void MapToReferenceDetails_ShouldReturnCompleteReferenceDetails()
        {
            //Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("prfpostcode", "SK1 3XE")
                .WithIntegrationFormField("prfaddress", "Address line 1|Address line 2|Town")
                .WithIntegrationFormField("prfplaceref", "1234")
                .WithIntegrationFormField("prffirstname", "name")
                .WithIntegrationFormField("prflastname", "lastname")
                .WithIntegrationFormField("prfrelation", "relation")
                .WithIntegrationFormField("prfyears", "years")
                .WithIntegrationFormField("prfemail", "email")
                .WithIntegrationFormField("prfcontact", "contact")
                .Build();

            //Act
            var result = ReferenceDetailsMapper.MapToReferenceDetails(entity.IntegrationFormFields,
                "prffirstname", "prflastname", "prfrelation", "prfyears",
                "prfemail", "prfcontact", "prfaddress",
                "prfplaceref", "prfpostcode");

            //Assert
            Assert.Equal(string.Empty, result.Address.AddressLine1);
            Assert.Equal(string.Empty, result.Address.AddressLine2);
            Assert.Equal(string.Empty, result.Address.Town);
            Assert.Equal("SK1 3XE", result.Address.Postcode);
            Assert.Equal("1234", result.Address.PlaceRef);
            Assert.Equal("name", result.FirstName);
            Assert.Equal("lastname", result.LastName);
            Assert.Equal("relation", result.RelationshipToYou);
            Assert.Equal("years", result.NumberOfYearsKnown);
            Assert.Equal("email", result.EmailAddress);
            Assert.Equal("contact", result.PhoneNumber);
        }

        [Fact]
        public void MapToReferenceDetails_ShouldReturnEmptyReferenceDetails()
        {
            //Arrange

            var integrationFormFields = new List<CustomField>();

            //Act
            var result = ReferenceDetailsMapper.MapToReferenceDetails(integrationFormFields,
                "prffirstname", "prflastname", "prfrelation", "prfyears",
                "prfemail", "prfcontact", "prfaddress",
                "prfplaceref", "prfpostcode");

            //Assert
            Assert.Equal(string.Empty, result.Address.AddressLine1);
            Assert.Equal(string.Empty, result.Address.AddressLine2);
            Assert.Equal(string.Empty, result.Address.Town);
            Assert.Equal(string.Empty, result.Address.Postcode);
            Assert.Equal(string.Empty, result.Address.PlaceRef);
            Assert.Equal(null, result.FirstName);
            Assert.Equal(null, result.LastName);
            Assert.Equal(null, result.RelationshipToYou);
            Assert.Equal(null, result.NumberOfYearsKnown);
            Assert.Equal(null, result.EmailAddress);
            Assert.Equal(null, result.PhoneNumber);
        }
    }
}
