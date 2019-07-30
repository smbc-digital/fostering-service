using System;
using System.Collections.Generic;
using System.Text;
using fostering_service.Mappers;
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
            var integrationFormFields = new List<CustomField>

            {
                new CustomField
                {
                    Name = "prfpostcode",
                    Value = "SK1 3XE"
                },
                new CustomField
                {
                    Name = "prfaddress",
                    Value = "Address line 1|Address line 2|Town"
                },
                new CustomField
                {
                    Name = "prfplaceref",
                    Value = "1234"
                },

                new CustomField
                {
                Name = "prffirstname",
                Value = "name"
                },

                new CustomField
                {
                    Name = "prflastname",
                    Value = "lastname"
                },

                new CustomField
                {
                    Name = "prfrelation",
                    Value = "relation"
                },

                new CustomField
                {
                    Name = "prfyears",
                    Value = "years"
                },

                new CustomField
                {
                    Name = "prfemail",
                    Value = "email"
                },

                new CustomField
                {
                    Name = "prfcontact",
                    Value = "contact"
                },
            };

            //Act
            var result = ReferenceDetailsMapper.MapToReferenceDetails(integrationFormFields,
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
            Assert.Equal(string.Empty, result.FirstName);
            Assert.Equal(string.Empty, result.LastName);
            Assert.Equal(string.Empty, result.RelationshipToYou);
            Assert.Equal(string.Empty, result.NumberOfYearsKnown);
            Assert.Equal(string.Empty, result.EmailAddress);
            Assert.Equal(string.Empty, result.PhoneNumber);
        }
    }
}
