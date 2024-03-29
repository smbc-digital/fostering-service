﻿using fostering_service.Mappers;
using fostering_service_tests.Builders;
using StockportGovUK.NetStandard.Gateways.Models.Verint;
using Xunit;
using Address = StockportGovUK.NetStandard.Gateways.Models.Fostering.Address;

namespace fostering_service_tests.Mappers
{
    public class AddressMapperTests
    {

        [Fact]
        public void MapToFosteringAddress_ShouldReturnCompleteAddress()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("postcode", "SK1 3XE")
                .WithIntegrationFormField("address", "Address line 1, Line 2, Town")
                .WithIntegrationFormField("placeref", "1234")
                .Build();

            // Act
            var result = AddressMapper.MapToFosteringAddress(entity.IntegrationFormFields, "address", "placeref", "postcode");

            // Assert
            Assert.Equal(string.Empty, result.AddressLine1);
            Assert.Equal(string.Empty, result.AddressLine2);
            Assert.Equal(string.Empty, result.Town);
            Assert.Equal("SK1 3XE", result.Postcode);
            Assert.Equal("1234", result.PlaceRef);
            Assert.Equal("Address line 1, Line 2, Town", result.SelectedAddress);
        }

        [Fact]
        public void MapToFosteringAddress_ShouldReturnAddressWithLine1()
        {
            //Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("postcode", "SK1 3XE")
                .WithIntegrationFormField("address", "Address line 1||")
                .WithIntegrationFormField("placeref", "")
                .Build();

            //Act
            var result = AddressMapper.MapToFosteringAddress(entity.IntegrationFormFields, "address", "placeref", "postcode");

            //Assert
            Assert.Equal("Address line 1", result.AddressLine1);
            Assert.Equal(string.Empty, result.AddressLine2);
            Assert.Equal(string.Empty, result.Town);
            Assert.Equal("SK1 3XE", result.Postcode);
            Assert.Equal(string.Empty, result.PlaceRef);
        }

        [Fact]
        public void MapToFosteringAddress_ShouldReturnTwoAddressLines()
        {
            // Arrange
            var integrationFormFields = new List<CustomField>
            {
                new CustomField("postcode", "SK1 3XE"),
                new CustomField("address", "Address line 1|Address line 2|"),
                new CustomField("placeref", "")
            };

            // Act
            var result = AddressMapper.MapToFosteringAddress(integrationFormFields, "address", "placeref", "postcode");

            //Assert
            Assert.Equal("Address line 1", result.AddressLine1);
            Assert.Equal("Address line 2", result.AddressLine2);
            Assert.Equal(string.Empty, result.Town);
            Assert.Equal("SK1 3XE", result.Postcode);
            Assert.Equal(string.Empty, result.PlaceRef);
        }
        
        [Fact]
        public void MapToFosteringAddress_ShouldReturnEmptyAddress()
        {
            //Act
            var result =
                AddressMapper.MapToFosteringAddress(new List<CustomField>(), "address", "placeref", "postcode");

            //Assert
            Assert.Equal(string.Empty, result.AddressLine1);
            Assert.Equal(string.Empty, result.AddressLine2);
            Assert.Equal(string.Empty, result.Town);
            Assert.Equal(string.Empty, result.Postcode);
            Assert.Equal(string.Empty, result.PlaceRef);
        }

        [Fact]
        public void MapToVerintAddress_ShouldReturnAddress()
        {
            // Arrange
            var address = new Address
            {
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                Postcode = "SK13XE",
                Town = "Town"
            };

            // Act
            var result = AddressMapper.MapToVerintAddress(address, "address", "placeRef", "postcode");

            // Assert
            Assert.True(result.Exists(_ => _.FormFieldName == "address" && _.FormFieldValue == "Line 1|Line 2|Town"));
            Assert.True(result.Exists(_ => _.FormFieldName == "postcode" && _.FormFieldValue == "SK13XE"));
            Assert.True(result.Exists(_ => _.FormFieldName == "placeRef" && _.FormFieldValue == null));
        }

        [Fact]
        public void MapToVerintAddress_ShouldMapAutomaticallyPickedAddress()
        {
            //Arrange
            var address = new Address
            {
                PlaceRef = "123",
                Postcode = "SK13EX",
                SelectedAddress = "address"
            };

            //Act
            var result = AddressMapper.MapToVerintAddress(address, "address", "placeRef", "postcode");

            //Assert
            Assert.True(result.Exists(_ => _.FormFieldName == "address" && _.FormFieldValue == "address"));
            Assert.True(result.Exists(_ => _.FormFieldName == "placeRef" && _.FormFieldValue == "123"));
            Assert.True(result.Exists(_ => _.FormFieldName == "postcode" && _.FormFieldValue == "SK13EX"));
        }

        [Fact]
        public void Validate_ShouldReturnManuallyPickedAddress()
        {
            // Arrange
            var address = new Address
            {
                AddressLine1 = "Line 1",
                Town = "Town",
                Postcode = "SK13XE"
            };

            // Act
            var validatedAddress = address.Validate();

            // Assert
            Assert.NotNull(validatedAddress);
        }

        [Fact]
        public void Validate_ShouldReturnAutomaticallyPickedAddress()
        {
            // Arrange
            var address = new Address
            {
                Postcode = "SK13EX",
                PlaceRef = "123"
            };

            // Act
            var validatedAddress = address.Validate();

            // Assert
            Assert.NotNull(validatedAddress);
        }

        [Fact]
        public void Validate_ShouldReturnNull()
        {
            // Arrange
            var address = new Address();

            // Act
            var validatedAddress = address.Validate();

            // Assert
            Assert.Null(validatedAddress);
        }
    }
}
