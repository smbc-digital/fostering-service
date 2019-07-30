using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using fostering_service.Models;
using fostering_service.Services;
using fostering_service_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.AspNetCore.Gateways.Response;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;
using Xunit;
using Model = StockportGovUK.NetStandard.Models.Models.Fostering;
using Address = StockportGovUK.NetStandard.Models.Models.Fostering.Address;

namespace fostering_service_tests.Service
{
    public class FosteringServiceTests
    {
        private readonly Mock<IVerintServiceGateway> _verintServiceGatewayMock = new Mock<IVerintServiceGateway>();
        private readonly Mock<ILogger<FosteringService>> _mockLogger = new Mock<ILogger<FosteringService>>();
        private readonly FosteringService _service;

        public FosteringServiceTests()
        {
            _service = new FosteringService(_verintServiceGatewayMock.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCase_ShouldCall_VerintService()
        {
            // Arrange 
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            await _service.GetCase("1234");

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.GetCase("1234"));
        }

        [Fact]
        public async Task GetCase_ShouldThrowException_OnNon200Response()
        {
            // Arrange 
            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetCase("1234"));
        }

        [Fact]
        public async Task GetCase_ShouldReturn_FosteringCase()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.IsType<FosteringCase>(result);
        }

        [Fact]
        public async Task GetCase_ShouldReturn_FosteringCaseWithParsedDateTime()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("dateofthehomevisit", "26/06/2019")
                .WithIntegrationFormField("timeofhomevisit", "13:30")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.IsType<FosteringCase>(result);
            Assert.Equal(DateTime.Parse("26/06/2019 13:30"), result.HomeVisitDateTime);

        }

        [Fact]
        public async Task GetCase_ShouldReturn_FosteringCaseWithAReference()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("prffirstname", "name")
                .WithIntegrationFormField("prflastname", "last name")
                .WithIntegrationFormField("prfrelation", "relation")
                .WithIntegrationFormField("prfyears", "years")
                .WithIntegrationFormField("prfemail", "test@test.com")
                .WithIntegrationFormField("prfcontact", "contact")
                .WithIntegrationFormField("prfplaceref", "123")
                .WithIntegrationFormField("prfpostcode", "sk13xe")
                .WithIntegrationFormField("prfaddress", "address")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.IsType<FosteringCase>(result);
            Assert.Equal("name", result.FamilyReference.FirstName);
            Assert.Equal("last name", result.FamilyReference.LastName);
            Assert.Equal("relation", result.FamilyReference.RelationshipToYou);
            Assert.Equal("years", result.FamilyReference.NumberOfYearsKnown);
            Assert.Equal("test@test.com", result.FamilyReference.EmailAddress);
            Assert.Equal("contact", result.FamilyReference.PhoneNumber);
            Assert.Equal("123", result.FamilyReference.Address.PlaceRef);
            Assert.Equal("sk13xe", result.FamilyReference.Address.Postcode);
            Assert.Null(result.FamilyReference.Address.SelectedAddress);
            Assert.Equal("", result.FamilyReference.Address.AddressLine1);
            Assert.Equal("", result.FamilyReference.Address.AddressLine2);
            Assert.Equal("", result.FamilyReference.Address.Town);
        }

        [Theory]
        [InlineData("withpartner")]
        [InlineData("")]
        public async Task GetCase_ShouldOnlyHaveOneApplicant(string formField)
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField(formField, "No")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.NotNull(result.FirstApplicant);
            Assert.Null(result.SecondApplicant);
        }

        [Fact]
        public async Task GetCase_ShouldMapFirstApplicant_ToFosteringCase()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("hasanothername", "True")
                .WithIntegrationFormField("under16address11", "test|test|test")
                .WithIntegrationFormField("under16postcode11", "test")
                .WithIntegrationFormField("over16address11", "test|test|test")
                .WithIntegrationFormField("over16postcode11", "test")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.Equal("First Name", result.FirstApplicant.FirstName);
            Assert.Equal("Last Name", result.FirstApplicant.LastName);
            Assert.True(result.FirstApplicant.EverBeenKnownByAnotherName);
            Assert.Equal("Previous name", result.FirstApplicant.AnotherName);
            Assert.Equal("Nationality", result.FirstApplicant.Nationality);
            Assert.Equal("Ethnicity", result.FirstApplicant.Ethnicity);
            Assert.Equal("Gender", result.FirstApplicant.Gender);
            Assert.Equal("Religion", result.FirstApplicant.Religion);
            Assert.NotNull(result.FirstApplicant);
            Assert.Null(result.SecondApplicant);
        }

        [Fact]
        public async Task GetCase_ShouldMapSecondApplicant_ToFosteringCase()
        {
            // Arrange
            var entity = new CaseBuilder()
                            .WithIntegrationFormField("religionorfaithgroup", "Religion")
                            .WithIntegrationFormField("gender", "Gender")
                            .WithIntegrationFormField("ethnicity", "Ethnicity")
                            .WithIntegrationFormField("nationality", "Nationality")
                            .WithIntegrationFormField("previousname", "Previous name")
                            .WithIntegrationFormField("surname", "Last Name")
                            .WithIntegrationFormField("firstname", "First Name")
                            .WithIntegrationFormField("withpartner", "Yes")
                            .WithIntegrationFormField("firstname_2", "First Name")
                            .WithIntegrationFormField("surname_2", "Last Name")
                            .WithIntegrationFormField("previousname_2", "Previous name")
                            .WithIntegrationFormField("nationality2", "Nationality")
                            .WithIntegrationFormField("ethnicity2", "Ethnicity")
                            .WithIntegrationFormField("gender2", "Gender")
                            .WithIntegrationFormField("religionorfaithgroup2", "Religion")
                            .WithIntegrationFormField("hasanothername2", "True")
                            .WithIntegrationFormField("haschildrenundersixteen2", "yes")
                            .WithIntegrationFormField("haschildrenoversixteen2", "yes")
                            .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.NotNull(result.FirstApplicant);
            Assert.NotNull(result.SecondApplicant);
            Assert.Equal("First Name", result.SecondApplicant.FirstName);
            Assert.Equal("Last Name", result.SecondApplicant.LastName);
            Assert.True(result.SecondApplicant.EverBeenKnownByAnotherName);
            Assert.Equal("Previous name", result.SecondApplicant.AnotherName);
            Assert.Equal("Nationality", result.SecondApplicant.Nationality);
            Assert.Equal("Ethnicity", result.SecondApplicant.Ethnicity);
            Assert.Equal("Gender", result.SecondApplicant.Gender);
            Assert.Equal("Religion", result.SecondApplicant.Religion);
        }

        [Theory]
        [InlineData("None", ETaskStatus.None)]
        [InlineData("CantStart", ETaskStatus.CantStart)]
        [InlineData("Completed", ETaskStatus.Completed)]
        [InlineData("NotCompleted", ETaskStatus.NotCompleted)]
        public async Task GetCase_ShouldMapStatus(string status, ETaskStatus expectedStatus)
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("tellusaboutyourselfstatus", status)
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("");

            // Assert
            Assert.Equal(expectedStatus, result.Statuses.TellUsAboutYourselfStatus);
        }

        [Fact]
        public async Task GetCase_ShouldMapMarriageStatus()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .WithIntegrationFormField("marriedorinacivilpartnership", "Yes")
                .WithIntegrationFormField("dateofreg", "26/06/2019")
                .WithIntegrationFormField("datesetuphousehold", "26/06/2019")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.True(result.MarriedOrInACivilPartnership);
            Assert.Equal(DateTime.Parse("26/06/2019"), result.DateOfMarriage);
            Assert.Equal(DateTime.Parse("26/06/2019"), result.DateMovedInTogether);
        }

        [Fact]
        public async Task GetCase_ShouldMapPreviouslyApplied()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .WithIntegrationFormField("previouslyappliedapplicant1", "yes")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.True(result.FirstApplicant.PreviouslyApplied);
        }

        [Fact]
        public async Task GetCase_ShouldMapPreviouslyApplied_ForBothApplicants()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .WithIntegrationFormField("withpartner", "Yes")
                .WithIntegrationFormField("firstname_2", "First name")
                .WithIntegrationFormField("surname_2", "Surname")
                .WithIntegrationFormField("previouslyappliedapplicant1", "yes")
                .WithIntegrationFormField("previouslyappliedapplicant2", "no")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.True(result.FirstApplicant.PreviouslyApplied);
            Assert.False(result.SecondApplicant.PreviouslyApplied);
        }

        [Fact]
        public async Task GetCase_ShouldMapTypesOfFostering()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("fiichildrenwithdisability", "ChildrenWithDisability")
                .WithIntegrationFormField("fiirespite", "Respite")
                .WithIntegrationFormField("fiishortterm", "ShortTerm")
                .WithIntegrationFormField("fiilongterm", "LongTerm")
                .WithIntegrationFormField("fiiunsure", "Unsure")
                .WithIntegrationFormField("fiishortbreaks", "ShortBreaks")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("");

            // Assert
            Assert.True(result.TypesOfFostering.Exists(_ => _.Equals("childrenWithDisability")));
            Assert.True(result.TypesOfFostering.Exists(_ => _.Equals("respite")));
            Assert.True(result.TypesOfFostering.Exists(_ => _.Equals("shortTerm")));
            Assert.True(result.TypesOfFostering.Exists(_ => _.Equals("longTerm")));
            Assert.True(result.TypesOfFostering.Exists(_ => _.Equals("unsure")));
            Assert.True(result.TypesOfFostering.Exists(_ => _.Equals("shortBreaks")));
        }

        [Fact]
        public async Task GetCase_ShouldMapReasonsForFostering()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("reasonsforfosteringapplicant1", "test")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("");

            // Assert
            Assert.Equal("test", result.ReasonsForFostering);
        }

        [Fact]
        public async Task GetCase_ShouldMapGpDetails()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("nameofgp", "Name of gp")
                .WithIntegrationFormField("nameofpractice", "Name of practice")
                .WithIntegrationFormField("gpphonenumber", "01234567890")
                .WithIntegrationFormField("addressofpractice", "First Line 1|Second Line 2|Town")
                .WithIntegrationFormField("postcodeofpractice", "SK1 3XE")
                .WithIntegrationFormField("placerefofpractice", "12345")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.Equal("Name of gp", result.FirstApplicant.NameOfGp);
            Assert.Equal("Name of practice", result.FirstApplicant.NameOfGpPractice);
            Assert.Equal("01234567890", result.FirstApplicant.GpPhoneNumber);
            Assert.Equal("", result.FirstApplicant.GpAddress.AddressLine1);
            Assert.Equal("", result.FirstApplicant.GpAddress.AddressLine2);
            Assert.Equal("", result.FirstApplicant.GpAddress.Town);
            Assert.Equal("12345", result.FirstApplicant.GpAddress.PlaceRef);
            Assert.Equal("SK1 3XE", result.FirstApplicant.GpAddress.Postcode);
        }

        [Fact]
        public async Task GetCase_ShouldMapGpDetails_SecondApplicant()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("withpartner", "Yes")
                .WithIntegrationFormField("firstname_2", "First name")
                .WithIntegrationFormField("surname_2", "Surname")
                .WithIntegrationFormField("nameofgp2", "Name of gp")
                .WithIntegrationFormField("nameofpractice2", "Name of practice")
                .WithIntegrationFormField("gpphonenumber2", "01234567890")
                .WithIntegrationFormField("addressofpractice2", "First Line 1|Second Line 2|Town")
                .WithIntegrationFormField("postcodeofpractice2", "SK1 3XE")
                .WithIntegrationFormField("placerefofpractice2", "")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.Equal("Name of gp", result.SecondApplicant.NameOfGp);
            Assert.Equal("Name of practice", result.SecondApplicant.NameOfGpPractice);
            Assert.Equal("01234567890", result.SecondApplicant.GpPhoneNumber);
            Assert.Equal("First Line 1", result.SecondApplicant.GpAddress.AddressLine1);
            Assert.Equal("Second Line 2", result.SecondApplicant.GpAddress.AddressLine2);
            Assert.Equal("Town", result.SecondApplicant.GpAddress.Town);
            Assert.Equal("", result.SecondApplicant.GpAddress.PlaceRef);
            Assert.Equal("SK1 3XE", result.SecondApplicant.GpAddress.Postcode);
        }

        [Fact]
        public async Task UpdateStatus_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _service.UpdateStatus("", ETaskStatus.None, EFosteringCaseForm.ChildrenLivingAwayFromYourHome);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateStatus("", ETaskStatus.None, EFosteringCaseForm.ChildrenLivingAwayFromYourHome));
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    AnotherName = "another-name",
                    Nationality = "nationality",
                    EverBeenKnownByAnotherName = true,
                    PlaceOfBirth = "place-of-birth"
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    AnotherName = "another-name",
                    Nationality = "nationality",
                    EverBeenKnownByAnotherName = true,
                    PlaceOfBirth = "place-of-birth"
                }
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateAboutYourself(model));
        }

        [Theory]
        [InlineData(true, ETaskStatus.NotCompleted)]
        [InlineData(false, ETaskStatus.Completed)]
        public async Task UpdateAboutYourself_ShouldReturnETaskStatus(bool hasAnotherName, ETaskStatus expectedStatus)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                }
            };

            // Act
            var result = await _service.UpdateAboutYourself(model);

            // Assert
            Assert.Equal(expectedStatus, result);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == expectedStatus.ToString())
                )), Times.Once);
        }

        [Theory]
        [InlineData(false, "false")]
        [InlineData(null, "")]
        public async Task UpdateAboutYourself_ShouldMapApplicantToIntegratedFormFields(bool? hasAnotherName, string expectedAnotherName)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == "Completed")
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "previousname" && match.FormFieldValue == "")
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "ethnicity" && match.FormFieldValue == model.FirstApplicant.Ethnicity)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "gender" && match.FormFieldValue == model.FirstApplicant.Gender)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "nationality" && match.FormFieldValue == model.FirstApplicant.Nationality)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "placeofbirth" && match.FormFieldValue == model.FirstApplicant.PlaceOfBirth)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "religionorfaithgroup" && match.FormFieldValue == model.FirstApplicant.Religion)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "hasanothername" && match.FormFieldValue == expectedAnotherName)
            )), Times.Once);
        }

        [Theory]
        [InlineData(false, "false")]
        [InlineData(null, "")]
        public async Task UpdateAboutYourself_ShouldMapSecondApplicant(bool? hasAnotherName, string expectedAnotherName)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                SecondApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                },
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = false
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == "Completed")
           )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "previousname_2" && match.FormFieldValue == "")
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "ethnicity2" && match.FormFieldValue == model.SecondApplicant.Ethnicity)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "gender2" && match.FormFieldValue == model.SecondApplicant.Gender)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "nationality2" && match.FormFieldValue == model.SecondApplicant.Nationality)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "placeofbirth_2" && match.FormFieldValue == model.SecondApplicant.PlaceOfBirth)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "religionorfaithgroup2" && match.FormFieldValue == model.SecondApplicant.Religion)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "hasanothername2" && match.FormFieldValue == expectedAnotherName)
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseLanguagesSpokenInYourHomeUpdateModel()
            {
                CaseReference = "0121DO1",
                PrimaryLanguage = "English",
                OtherLanguages = "Dutch"
            };

            // Act
            await _service.UpdateLanguagesSpokenInYourHome(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            var model = new FosteringCaseLanguagesSpokenInYourHomeUpdateModel()
            {
                CaseReference = "0121DO1",
                PrimaryLanguage = "English",
                OtherLanguages = "Dutch"
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateLanguagesSpokenInYourHome(model));
        }

        [Theory]
        [InlineData(null, ETaskStatus.NotCompleted)]
        [InlineData("English", ETaskStatus.Completed)]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldReturnETaskStatus(string primaryLanguage, ETaskStatus expectedStatus)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseLanguagesSpokenInYourHomeUpdateModel()
            {
                CaseReference = "0121DO1",
                PrimaryLanguage = primaryLanguage,
                OtherLanguages = "Dutch"
            };

            // Act
            var result = await _service.UpdateLanguagesSpokenInYourHome(model);

            // Assert
            Assert.Equal(expectedStatus, result);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "languagespokeninyourhomestatus" && match.FormFieldValue == expectedStatus.ToString())
                )), Times.Once);
        }

        [Theory]
        [InlineData("English", "Dutch")]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldMapToIntegratedFormFields(string primaryLanguage, string otherLanguages)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseLanguagesSpokenInYourHomeUpdateModel()
            {
                CaseReference = "0121DO1",
                PrimaryLanguage = primaryLanguage,
                OtherLanguages = otherLanguages
            };

            // Act
            await _service.UpdateLanguagesSpokenInYourHome(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "languagespokeninyourhomestatus" && match.FormFieldValue == "Completed")
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "primarylanguage" && match.FormFieldValue == "English")
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "otherlanguages" && match.FormFieldValue == "Dutch")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldCallVerintService()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test"
            };

            // Act
            await _service.UpdatePartnershipStatus(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldCreateIntegratedFormFields()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test",
                DateMovedInTogether = DateTime.Parse("01/09/1996"),
                DateOfMarriage = DateTime.Parse("01/09/1997"),
                MarriedOrInACivilPartnership = true
            };

            // Act
            await _service.UpdatePartnershipStatus(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.CaseReference == "test"
            )), Times.Once);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "datesetuphousehold" && field.FormFieldValue == "01/09/1996")
                )), Times.Once);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "dateofreg" && field.FormFieldValue == "01/09/1997")
            )), Times.Once);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "marriedorinacivilpartnership" && field.FormFieldValue == "Yes")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldCreateEmptyIntegratedFormFields()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test",
                DateMovedInTogether = null,
                DateOfMarriage = null,
                MarriedOrInACivilPartnership = null
            };

            // Act
            await _service.UpdatePartnershipStatus(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.CaseReference == "test"
            )), Times.Once);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "datesetuphousehold" && field.FormFieldValue == string.Empty)
            )), Times.Once);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "dateofreg" && field.FormFieldValue == string.Empty)
            )), Times.Once);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "marriedorinacivilpartnership" && field.FormFieldValue == string.Empty)
            )), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldReturnNotCompleted_WhenMarried()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test",
                DateMovedInTogether = null,
                DateOfMarriage = null,
                MarriedOrInACivilPartnership = true
            };

            // Act
            var result = await _service.UpdatePartnershipStatus(model);

            // Assert
            Assert.Equal(ETaskStatus.NotCompleted, result);
            _verintServiceGatewayMock.Verify(_ =>_.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "yourpartnershipstatus" && field.FormFieldValue == "NotCompleted")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldReturnCompleted_WhenMarried()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test",
                DateMovedInTogether = null,
                DateOfMarriage = DateTime.Parse("26/06/2019"),
                MarriedOrInACivilPartnership = true
            };

            // Act
            var result = await _service.UpdatePartnershipStatus(model);

            // Assert
            Assert.Equal(ETaskStatus.Completed, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "yourpartnershipstatus" && field.FormFieldValue == "Completed")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldReturnCompleted_WhenNotMarried()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test",
                DateMovedInTogether = DateTime.Parse("26/06/2019"),
                DateOfMarriage = null,
                MarriedOrInACivilPartnership = false
            };

            // Act
            var result = await _service.UpdatePartnershipStatus(model);

            // Assert
            Assert.Equal(ETaskStatus.Completed, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "yourpartnershipstatus" && field.FormFieldValue == "Completed")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldReturnNotCompleted_WhenNotMarried()
        {
            // Arrange
            var model = new FosteringCasePartnershipStatusUpdateModel
            {
                CaseReference = "test",
                DateMovedInTogether = null,
                DateOfMarriage = null,
                MarriedOrInACivilPartnership = false
            };

            // Act
            var result = await _service.UpdatePartnershipStatus(model);

            // Assert
            Assert.Equal(ETaskStatus.NotCompleted, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "yourpartnershipstatus" && field.FormFieldValue == "NotCompleted")
            )), Times.Once);
        }

        [Theory]
        [InlineData(true, "Yes")]
        [InlineData(false, "No")]
        public async Task UpdateYourFosteringHistory_ShouldAddFormFieldsForFirstApplicant(bool previouslyAppliedValue, string expectedFormValue)
        {
            // Arrange
            var callbackModel = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseYourFosteringHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseYourFosteringHistoryApplicationUpdateModel
                {
                    PreviouslyApplied = previouslyAppliedValue
                },
                CaseReference = "1234"
            };

            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(() => new HttpResponseMessage())
                .Callback<IntegrationFormFieldsUpdateModel>(a => callbackModel = a);

            // Act
            await _service.UpdateYourFosteringHistory(model);

            // Assert
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "previouslyappliedapplicant1" && _.FormFieldValue == expectedFormValue);
        }

        [Theory]
        [InlineData(true, "Yes")]
        [InlineData(false, "No")]
        public async Task UpdateYourFosteringHistory_ShouldAddFormFieldsForBothApplicants(bool previouslyAppliedValue, string expectedFormValue)
        {
            // Arrange
            var callbackModel = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseYourFosteringHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseYourFosteringHistoryApplicationUpdateModel
                {
                    PreviouslyApplied = previouslyAppliedValue
                },
                SecondApplicant = new FosteringCaseYourFosteringHistoryApplicationUpdateModel
                {
                    PreviouslyApplied = previouslyAppliedValue
                },
                CaseReference = "1234"
            };

            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(() => new HttpResponseMessage())
                .Callback<IntegrationFormFieldsUpdateModel>(a => callbackModel = a);

            // Act
            await _service.UpdateYourFosteringHistory(model);

            // Assert
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "previouslyappliedapplicant1" && _.FormFieldValue == expectedFormValue);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "previouslyappliedapplicant2" && _.FormFieldValue == expectedFormValue);
        }

        [Theory]
        [InlineData(ETaskStatus.NotCompleted, null)]
        [InlineData(ETaskStatus.Completed, true)]
        [InlineData(ETaskStatus.Completed, false)]
        public async Task UpdateYourFosteringHistory_ShouldCorrectlyAddFormStatus(ETaskStatus expectedETaskStatus, bool? previouslyApplied)
        {
            // Arrange
            var callbackModel = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseYourFosteringHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseYourFosteringHistoryApplicationUpdateModel
                {
                    PreviouslyApplied = previouslyApplied
                },
                CaseReference = "1234"
            };

            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(() => new HttpResponseMessage())
                .Callback<IntegrationFormFieldsUpdateModel>(a => callbackModel = a);

            // Act
            var result = await _service.UpdateYourFosteringHistory(model);

            // Assert
            Assert.Equal(expectedETaskStatus, result);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "yourfosteringhistorystatus" && _.FormFieldValue == expectedETaskStatus.ToString());
        }

        [Theory]
        [InlineData(true, true, "Yes", "Yes")]
        [InlineData(false, false, "No", "No")]
        public async Task UpdateYourHealthStatus_ShouldAddFormFieldsForFirstApplicant(bool registeredDisabled, bool practitioner, string firstExpectedFormValue, string secondExpectedFormValue)
        {
            // Arrange
            var callbackModel = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseHealthUpdateModel
            {
                FirstApplicant = new FosteringCaseHealthApplicantUpdateModel
                {
                   RegisteredDisabled = registeredDisabled,
                   Practitioner = practitioner,
                },
                CaseReference = "1234"
            };

            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(() => new HttpResponseMessage())
                .Callback<IntegrationFormFieldsUpdateModel>(a => callbackModel = a);

            // Act
            await _service.UpdateHealthStatus(model);

            // Assert
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "registereddisabled" && _.FormFieldValue == firstExpectedFormValue);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "practitioner" && _.FormFieldValue == secondExpectedFormValue);
        }

        [Theory]
        [InlineData(true, true, true, true, "Yes", "Yes", "Yes", "Yes")]
        [InlineData(false, false, false, false, "No", "No", "No", "No")]
        public async Task UpdateHealthSt5tatus_ShouldAddFormFieldsForBothApplicants(bool registeredDisabled, bool practitioner, bool registeredDisabled2, bool practitioner2, string firstExpectedFormValue, string secondExpectedFormValue, string thirdExpectedFormValue, string fourthExpectedFormValue)
        {
            // Arrange
            var callbackModel = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseHealthUpdateModel
            {
                FirstApplicant = new FosteringCaseHealthApplicantUpdateModel
                {
                    RegisteredDisabled = registeredDisabled,
                    Practitioner = practitioner,
                },
                SecondApplicant = new FosteringCaseHealthApplicantUpdateModel
                {
                    RegisteredDisabled = registeredDisabled2,
                    Practitioner = practitioner2,
                },
                CaseReference = "1234"
            };

            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(() => new HttpResponseMessage())
                .Callback<IntegrationFormFieldsUpdateModel>(a => callbackModel = a);

            // Act
            await _service.UpdateHealthStatus(model);

            // Assert
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "registereddisabled" && _.FormFieldValue == firstExpectedFormValue);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "practitioner" && _.FormFieldValue == secondExpectedFormValue);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "registereddisabled2" && _.FormFieldValue == thirdExpectedFormValue);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "practitioner2" && _.FormFieldValue == fourthExpectedFormValue);
        }

        [Theory]
        [InlineData(ETaskStatus.NotCompleted, null, null)]
        [InlineData(ETaskStatus.NotCompleted, null, true)]
        [InlineData(ETaskStatus.Completed, true, true)]
        [InlineData(ETaskStatus.Completed, false, true)]
        public async Task UpdateHealthStatus_ShouldCorrectlyAddFormStatus(ETaskStatus expectedETaskStatus, bool? practitioner, bool? registeredDisabled)
        {
            // Arrange
            var callbackModel = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseHealthUpdateModel
            {
                FirstApplicant = new FosteringCaseHealthApplicantUpdateModel()
                {
                    Practitioner = practitioner,
                    RegisteredDisabled = registeredDisabled
                },
                CaseReference = "1234"
            };

            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(() => new HttpResponseMessage())
                .Callback<IntegrationFormFieldsUpdateModel>(a => callbackModel = a);

            // Act
            var result = await _service.UpdateHealthStatus(model);

            // Assert
            Assert.Equal(expectedETaskStatus, result);
            Assert.Contains(callbackModel.IntegrationFormFields, _ => _.FormFieldName == "yourhealthstatus" && _.FormFieldValue == expectedETaskStatus.ToString());
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldCreateTypesOfFosteringIntegrationFormFields()
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                ReasonsForFostering = "test",
                TypesOfFostering = new List<string>
                {
                    "childrenWithDisability",
                    "respite",
                    "shortTerm",
                    "longTerm",
                    "unsure",
                    "shortBreaks"
                }
            };

            // Act
            await _service.UpdateInterestInFostering(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiichildrenwithdisability" && field.FormFieldValue == "ChildrenWithDisability")
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiirespite" && field.FormFieldValue == "Respite")
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiishortterm" && field.FormFieldValue == "ShortTerm")
            )), Times.Once);
             _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiilongterm" && field.FormFieldValue == "LongTerm")
            )), Times.Once);
             _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiiunsure" && field.FormFieldValue == "Unsure")
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiishortbreaks" && field.FormFieldValue == "ShortBreaks")
            )), Times.Once);

        }


        [Fact]
        public async Task UpdateInterestInFostering_ShouldCreateTypesOfFosteringEmptyIntegrationFormFields()
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                ReasonsForFostering = "test",
                TypesOfFostering = new List<string>()
            };

            // Act
            await _service.UpdateInterestInFostering(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiichildrenwithdisability" && field.FormFieldValue == string.Empty)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiirespite" && field.FormFieldValue == string.Empty)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiishortterm" && field.FormFieldValue == string.Empty)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
               updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiilongterm" && field.FormFieldValue == string.Empty)
           )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
               updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiiunsure" && field.FormFieldValue == string.Empty)
           )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiishortbreaks" && field.FormFieldValue == string.Empty)
            )), Times.Once);
        }

        [Theory]
        [InlineData("reasons", "reasons")]
        [InlineData(null, "")]
        public async Task UpdateInterestInFostering_ShouldCreateEmptyReasonsForFosteringIntegrationFormFields(string reasons, string expectedReasons)
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                TypesOfFostering = new List<string>(),
                ReasonsForFostering = reasons
            };

            // Act
            await _service.UpdateInterestInFostering(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "reasonsforfosteringapplicant1" && field.FormFieldValue == expectedReasons)
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldCall_VerintService()
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                TypesOfFostering = new List<string>()
            };

            // Act
            await _service.UpdateInterestInFostering(model);

            // Assert
            _verintServiceGatewayMock.Verify(
                _ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturnCompleted()
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                TypesOfFostering = new List<string>
                {
                    "unsure"
                },
                ReasonsForFostering = "test"
            };

            // Act
            var result = await _service.UpdateInterestInFostering(model);

            // Assert
            Assert.Equal(ETaskStatus.Completed, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "tellusaboutyourinterestinfosteringstatus" && field.FormFieldValue == "Completed")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturnNotCompleted()
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                TypesOfFostering = new List<string>
                {
                    "unsure"
                },
                ReasonsForFostering = string.Empty
            };

            // Act
            var result = await _service.UpdateInterestInFostering(model);

            // Assert
            Assert.Equal(ETaskStatus.NotCompleted, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "tellusaboutyourinterestinfosteringstatus" && field.FormFieldValue == "NotCompleted")
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturnNotCompleted_WhenTypesOfFosteringEmpty()
        {
            // Arrange
            var model = new FosteringCaseInterestInFosteringUpdateModel
            {
                CaseReference = "1234",
                TypesOfFostering = new List<string>(),
                ReasonsForFostering = "Test"
            };

            // Act
            var result = await _service.UpdateInterestInFostering(model);

            // Assert
            Assert.Equal(ETaskStatus.NotCompleted, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "tellusaboutyourinterestinfosteringstatus" && field.FormFieldValue == "NotCompleted")
            )), Times.Once);
        }

        [Fact]
        public void CreateOtherPersonList_ShouldMapCustomFormFieldToOtherPersonList()
        {
            // Arrange
            var config = new OtherPeopleConfigurationModel
            {
                DateOfBirth = "PREFIX_DOB",
                FirstName = "ANOTHER_PREFIX_FIRSTNAME",
                Gender = "PREFIX_EXAMPLE_GENDER",
                LastName = "PREFIX_EXAMPLE_LASTNAME"
            };

            var model = new List<CustomField>
            {
                new CustomField
                {
                    Name = $"{config.DateOfBirth}1",
                    Value =  "01/02/1996"
                },
                new CustomField
                {
                    Name = $"{config.FirstName}1",
                    Value =  "firstname1"
                },
                new CustomField
                {
                    Name = $"{config.DateOfBirth}2",
                    Value =  "01/02/1996"
                },
                new CustomField
                {
                    Name = $"{config.FirstName}2",
                    Value =  "firstname2"
                },
                new CustomField
                {
                    Name = $"{config.DateOfBirth}3",
                    Value =  "01/02/1996"
                },
                new CustomField
                {
                    Name = $"{config.FirstName}3",
                    Value =  "firstname3"
                }
            };

            // Act
            var result = _service.CreateOtherPersonList(config, model);

            // Assert
            Assert.Equal(model[0].Value, result[0].DateOfBirth.GetValueOrDefault().ToString("dd/MM/yyyy"));
            Assert.Equal(model[1].Value, result[0].FirstName);
            Assert.Equal(model[2].Value, result[1].DateOfBirth.GetValueOrDefault().ToString("dd/MM/yyyy"));
            Assert.Equal(model[3].Value, result[1].FirstName);
            Assert.Equal(model[4].Value, result[2].DateOfBirth.GetValueOrDefault().ToString("dd/MM/yyyy"));
            Assert.Equal(model[5].Value, result[2].FirstName);

            Assert.Equal(3, result.Count);
        }


        [Fact]
        public void CreateOtherPersonList_ShouldMapCustomFormFieldToOtherPersonList_When8thFieldPopulated()
        {
            // Arrange
            var config = new OtherPeopleConfigurationModel
            {
                DateOfBirth = "PREFIX_DOB",
                FirstName = "ANOTHER_PREFIX_FIRSTNAME",
                Gender = "PREFIX_EXAMPLE_GENDER",
                LastName = "PREFIX_EXAMPLE_LASTNAME"
            };

            var model = new List<CustomField>
            {
                new CustomField
                {
                    Name = $"{config.Gender}1",
                    Value =  "test"
                },
                new CustomField
                {
                    Name = $"{config.LastName}1",
                    Value =  "last name test"
                },
                new CustomField
                {
                    Name = $"{config.Gender}8",
                    Value =  "test 2"
                },
                new CustomField
                {
                    Name = $"{config.LastName}8",
                    Value =  "last name test 2"
                }
            };

            // Act
            var result = _service.CreateOtherPersonList(config, model);

            // Assert
            Assert.Equal(model[0].Value, result[0].Gender);
            Assert.Equal(model[1].Value, result[0].LastName);
            Assert.Equal(model[2].Value, result[1].Gender);
            Assert.Equal(model[3].Value, result[1].LastName);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void CreateOtherPersonBuilder_ShouldMapPersonListToIntegratedFormFields()
        {
            // Arrange
            var peopleList = new List<OtherPerson>
            {
                new OtherPerson
                {
                    FirstName = "test first name"
                }
            };

            var config = new OtherPeopleConfigurationModel
            {
                FirstName = "FIRSTNAME_TEST_PREFIX"
            };

            // Act
            var result = _service.CreateOtherPersonBuilder(config, peopleList).Build();

            // Assert
            Assert.Equal(peopleList[0].FirstName, result[0].FormFieldValue);
            Assert.Equal($"{config.FirstName}1", result[0].FormFieldName);

            Assert.Equal(32, result.Count);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseHouseholdUpdateModel
            {
                CaseReference = "0121DO1",
                DoYouHaveAnyPets = null,
                AnyOtherPeopleInYourHousehold = null,
                OtherPeopleInYourHousehold = new List<OtherPerson>(),
                PetsInformation = ""
            };

            // Act
            await _service.UpdateHousehold(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            var model = new FosteringCaseHouseholdUpdateModel
            {
                CaseReference = "0121DO1",
                DoYouHaveAnyPets = false,
                AnyOtherPeopleInYourHousehold = false,
                OtherPeopleInYourHousehold = new List<OtherPerson>(),
                PetsInformation = ""
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateHousehold(model));
        }

        [Theory]
        [InlineData(true, ETaskStatus.NotCompleted)]
        [InlineData(false, ETaskStatus.Completed)]
        public async Task UpdateHousehold_ShouldReturnETaskStatus(bool anyPets, ETaskStatus expectedStatus)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseHouseholdUpdateModel
            {
                CaseReference = "0121DO1",
                AnyOtherPeopleInYourHousehold = false,
                DoYouHaveAnyPets = anyPets,
                OtherPeopleInYourHousehold = new List<OtherPerson>(),
                PetsInformation = ""
            };

            // Act
            var result = await _service.UpdateHousehold(model);

            // Assert
            Assert.Equal(expectedStatus, result);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "yourhouseholdstatus" && match.FormFieldValue == expectedStatus.ToString())
                )), Times.Once);
        }

        [Theory]
        [InlineData(false, "false")]
        [InlineData(null, "")]
        public async Task UpdateHousehold_ShouldMapApplicantToIntegratedFormFields(bool? hasAnotherName, string expectedAnotherName)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == "Completed")
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "previousname" && match.FormFieldValue == "")
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "ethnicity" && match.FormFieldValue == model.FirstApplicant.Ethnicity)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "gender" && match.FormFieldValue == model.FirstApplicant.Gender)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "nationality" && match.FormFieldValue == model.FirstApplicant.Nationality)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "placeofbirth" && match.FormFieldValue == model.FirstApplicant.PlaceOfBirth)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "religionorfaithgroup" && match.FormFieldValue == model.FirstApplicant.Religion)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "hasanothername" && match.FormFieldValue == expectedAnotherName)
            )), Times.Once);
        }

        [Theory]
        [InlineData(false, "false")]
        [InlineData(null, "")]
        public async Task UpdateHousehold_ShouldMapSecondApplicant(bool? hasAnotherName, string expectedAnotherName)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                SecondApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                },
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = false
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == "Completed")
           )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "previousname_2" && match.FormFieldValue == "")
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "ethnicity2" && match.FormFieldValue == model.SecondApplicant.Ethnicity)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "gender2" && match.FormFieldValue == model.SecondApplicant.Gender)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "nationality2" && match.FormFieldValue == model.SecondApplicant.Nationality)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "placeofbirth_2" && match.FormFieldValue == model.SecondApplicant.PlaceOfBirth)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "religionorfaithgroup2" && match.FormFieldValue == model.SecondApplicant.Religion)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "hasanothername2" && match.FormFieldValue == expectedAnotherName)
            )), Times.Once);
        }


        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldCallVerintServiceGateway()
        {
            //Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseChildrenLivingAwayFromHomeUpdateModel
            {
                FirstApplicant = new FosteringCaseChildrenLivingAwayFromHomeApplicantUpdateModel
                {
                    ChildrenUnderSixteenLivingAwayFromHome = new List<OtherPerson>(),
                    ChildrenOverSixteenLivingAwayFromHome = new List<OtherPerson>()
                }
            };

            //Act
            await _service.UpdateChildrenLivingAwayFromHome(model);

            //Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            var model = new FosteringCaseChildrenLivingAwayFromHomeUpdateModel()
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseChildrenLivingAwayFromHomeApplicantUpdateModel
                {
                    ChildrenUnderSixteenLivingAwayFromHome = new List<OtherPerson>(),
                    ChildrenOverSixteenLivingAwayFromHome = new List<OtherPerson>()
                }
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateChildrenLivingAwayFromHome(model));
        }

        [Theory]
        [InlineData(true, null, ETaskStatus.NotCompleted)]
        [InlineData(true, true, ETaskStatus.NotCompleted)]
        [InlineData(false, false, ETaskStatus.Completed)]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldReturnETaskStatus(bool? hasUnderSixteenLivingAwayFromHome, bool? hasOverSixteenLivingAwayFromHome, ETaskStatus expectedStatus)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseChildrenLivingAwayFromHomeUpdateModel()
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseChildrenLivingAwayFromHomeApplicantUpdateModel
                {
                    AnyChildrenUnderSixteen = hasUnderSixteenLivingAwayFromHome,
                    AnyChildrenOverSixteen = hasOverSixteenLivingAwayFromHome,
                    ChildrenUnderSixteenLivingAwayFromHome = new List<OtherPerson>(),
                    ChildrenOverSixteenLivingAwayFromHome = new List<OtherPerson>()
                }
            };

            // Act
            var result = await _service.UpdateChildrenLivingAwayFromHome(model);

            // Assert
            Assert.Equal(expectedStatus, result);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "childrenlivingawayfromyourhomestatus" && match.FormFieldValue == expectedStatus.ToString())
                )), Times.Once);
        }

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldMapFirstApplicantToIntegratedFormFields()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var date = DateTime.Now;
            var expectedDate = date.ToString("dd/MM/yyyy");

            var expectedUnderSixteenAddress = "31 Street|Place|Town";
            var expectedOverSixteenAddress = "31 Road|Place|Town";

            var model = new FosteringCaseChildrenLivingAwayFromHomeUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseChildrenLivingAwayFromHomeApplicantUpdateModel
                {
                    AnyChildrenUnderSixteen = true,
                    AnyChildrenOverSixteen = true,
                    ChildrenUnderSixteenLivingAwayFromHome = new List<OtherPerson>
                    {
                        new OtherPerson
                        {
                            FirstName = "Under",
                            LastName = "Sixteen",
                            Gender = "Male",
                            DateOfBirth = date,
                            Address = new Model.Address
                            {
                                AddressLine1 = "31 Street",
                                AddressLine2 = "Place",
                                Town = "Town",
                                Postcode = "SK1 3XE"
                            }
                        }
                    },
                    ChildrenOverSixteenLivingAwayFromHome = new List<OtherPerson>
                    {
                        new OtherPerson
                        {
                            FirstName = "Over",
                            LastName = "Sixteen",
                            Gender = "Female",
                            DateOfBirth = date,
                            Address = new Model.Address
                            {
                                AddressLine1 = "31 Road",
                                AddressLine2 = "Place",
                                Town = "Town",
                                Postcode = "SK1 3XE"
                            }
                        }
                    }
                }
            };

            // Act
            await _service.UpdateChildrenLivingAwayFromHome(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "childrenlivingawayfromyourhomestatus" && match.FormFieldValue == "Completed")
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16firstname11" && match.FormFieldValue == model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].FirstName)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16lastname11" && match.FormFieldValue == model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].LastName)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16gender11" && match.FormFieldValue == model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].Gender)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16address11" && match.FormFieldValue == expectedUnderSixteenAddress)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16postcode11" && match.FormFieldValue == model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].Address.Postcode)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16dateofbirth11" && Equals(match.FormFieldValue, expectedDate))
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16firstname11" && match.FormFieldValue == model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome[0].FirstName)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16lastname11" && match.FormFieldValue == model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome[0].LastName)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16gender11" && match.FormFieldValue == model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome[0].Gender)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16dateofbirth11" && Equals(match.FormFieldValue, expectedDate))
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16address11" && match.FormFieldValue == expectedOverSixteenAddress)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16postcode11" && match.FormFieldValue == model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome[0].Address.Postcode)
                )), Times.Once);
        }

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldMapSecondApplicantToIntegratedFormFields()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var date = DateTime.Now;
            var expectedDate = date.ToString("dd/MM/yyyy");

            var expectedUnderSixteenAddress = "31 Street|Place|Town";
            var expectedOverSixteenAddress = "31 Road|Place|Town";

            var model = new FosteringCaseChildrenLivingAwayFromHomeUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseChildrenLivingAwayFromHomeApplicantUpdateModel
                {
                    AnyChildrenUnderSixteen = false,
                    AnyChildrenOverSixteen = false,
                    ChildrenUnderSixteenLivingAwayFromHome = new List<OtherPerson>(),
                    ChildrenOverSixteenLivingAwayFromHome = new List<OtherPerson>()
                },
                SecondApplicant = new FosteringCaseChildrenLivingAwayFromHomeApplicantUpdateModel
                {
                    AnyChildrenUnderSixteen = true,
                    AnyChildrenOverSixteen = true,
                    ChildrenUnderSixteenLivingAwayFromHome = new List<OtherPerson>
                    {
                        new OtherPerson
                        {
                            FirstName = "Under",
                            LastName = "Sixteen",
                            Gender = "Male",
                            DateOfBirth = date,
                            Address = new Model.Address
                            {
                                AddressLine1 = "31 Street",
                                AddressLine2 = "Place",
                                Town = "Town",
                                Postcode = "SK1 3XE"
                            }
                        }
                    },
                    ChildrenOverSixteenLivingAwayFromHome = new List<OtherPerson>
                    {
                        new OtherPerson
                        {
                            FirstName = "Over",
                            LastName = "Sixteen",
                            Gender = "Female",
                            DateOfBirth = date,
                            Address = new Model.Address
                            {
                                AddressLine1 = "31 Road",
                                AddressLine2 = "Place",
                                Town = "Town",
                                Postcode = "SK1 3XE"
                            }
                        }
                    }
                }
            };

            // Act
            await _service.UpdateChildrenLivingAwayFromHome(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "childrenlivingawayfromyourhomestatus" && match.FormFieldValue == "Completed")
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16firstname21" && match.FormFieldValue == model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].FirstName)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16lastname21" && match.FormFieldValue == model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].LastName)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16gender21" && match.FormFieldValue == model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].Gender)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16address21" && match.FormFieldValue == expectedUnderSixteenAddress)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16postcode21" && match.FormFieldValue == model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome[0].Address.Postcode)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "under16dateofbirth21" && Equals(match.FormFieldValue, expectedDate))
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16firstname21" && match.FormFieldValue == model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome[0].FirstName)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16lastname21" && match.FormFieldValue == model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome[0].LastName)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16gender21" && match.FormFieldValue == model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome[0].Gender)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16dateofbirth21" && Equals(match.FormFieldValue, expectedDate))
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16address21" && match.FormFieldValue == expectedOverSixteenAddress)
                )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "over16postcode21" && match.FormFieldValue == model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome[0].Address.Postcode)
                )), Times.Once);
        }

        [Theory]
        [InlineData("addressLine1|fghf|ttykuuky", "addressLine1", "fghf", "ttykuuky")]
        [InlineData("line1|line2||", "line1", "line2", "")]
        [InlineData("|", "", null, "")]
        [InlineData("||", "", "", "")]
        [InlineData("|||", "", "", "")]
        public void CreateOtherPersonList_ShouldReturnCorrectAddress(string address, string expectedLine1, string expectedLine2, string expectedTown)
        {
            // Arrange
             var config = new OtherPeopleConfigurationModel
             {
                 DateOfBirth = "PREFIX_DOB",
                 FirstName = "ANOTHER_PREFIX_FIRSTNAME",
                 Gender = "PREFIX_EXAMPLE_GENDER",
                 LastName = "PREFIX_EXAMPLE_LASTNAME",
                 Address = "over16address1"
             };

            var model = new List<CustomField>
            {
                new CustomField
                {
                    Name = $"{config.Address}1",
                    Value =  address
                }
            };

            // Act
            var result = _service.CreateOtherPersonList(config, model, 1);

            // Assert
            Assert.Equal(result[0].Address.AddressLine1, expectedLine1);
            Assert.Equal(result[0].Address.AddressLine2, expectedLine2);
            Assert.Equal(result[0].Address.Town, expectedTown);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldCallVerintServiceGateway()
        {
            //Arrange
            var model = new FosteringCaseGpDetailsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseGpDetailsApplicantUpdateModel
                {
                    GpAddress = new Address
                    {
                        AddressLine1 = "Line 1",
                        AddressLine2 = "Line 2",
                        Postcode = "SK1 3XE",
                        Town = "Stockport"
                    },
                    GpPhoneNumber = "0123456789",
                    NameOfGpPractice = "Practice name",
                    NameOfGp = "Gp name"
                }
            };
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            //Act
            await _service.UpdateGpDetails(model);

            //Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldThrowException()
        {
            // Arrange
            var model = new FosteringCaseGpDetailsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseGpDetailsApplicantUpdateModel
                {
                    GpAddress = new Address
                    {
                        AddressLine1 = "Line 1",
                        AddressLine2 = "Line 2",
                        Postcode = "SK1 3XE",
                        Town = "Stockport"
                    },
                    GpPhoneNumber = "0123456789",
                    NameOfGpPractice = "Practice name",
                    NameOfGp = "Gp name"
                }
            };
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateGpDetails(model));
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldCreateIntegrationFormFields()
        {
            // Arrange
            var model = new FosteringCaseGpDetailsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseGpDetailsApplicantUpdateModel
                {
                    GpAddress = new Address
                    {
                        AddressLine1 = "Line 1",
                        AddressLine2 = "Line 2",
                        Postcode = "SK1 3XE",
                        Town = "Stockport"
                    },
                    GpPhoneNumber = "0123456789",
                    NameOfGpPractice = "Practice name",
                    NameOfGp = "Gp name"
                }
            };
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _service.UpdateGpDetails(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "nameofgp" && field.FormFieldValue == model.FirstApplicant.NameOfGp)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "nameofpractice" && field.FormFieldValue == model.FirstApplicant.NameOfGpPractice)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "gpphonenumber" && field.FormFieldValue == model.FirstApplicant.GpPhoneNumber)
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldCreateIntegrationFormFields_ForSecondApplicant()
        {
            // Arrange
            var model = new FosteringCaseGpDetailsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseGpDetailsApplicantUpdateModel
                {
                    GpAddress = new Address
                    {
                        AddressLine1 = "Line 1",
                        AddressLine2 = "Line 2",
                        Postcode = "SK1 3XE",
                        Town = "Stockport"
                    },
                    GpPhoneNumber = "0123456789",
                    NameOfGpPractice = "Practice name",
                    NameOfGp = "Gp name"
                },
                SecondApplicant = new FosteringCaseGpDetailsApplicantUpdateModel
                {
                    GpAddress = new Address
                    {
                        AddressLine1 = "Line 1",
                        AddressLine2 = "Line 2",
                        Postcode = "SK1 3XE",
                        Town = "Stockport"
                    },
                    GpPhoneNumber = "0123456789",
                    NameOfGpPractice = "Practice name",
                    NameOfGp = "Gp name"
                }
            };
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            // Act
            await _service.UpdateGpDetails(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "nameofgp2" && field.FormFieldValue == model.SecondApplicant.NameOfGp)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "nameofpractice2" && field.FormFieldValue == model.SecondApplicant.NameOfGpPractice)
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "gpphonenumber2" && field.FormFieldValue == model.SecondApplicant.GpPhoneNumber)
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateReferences_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseReferenceUpdateModel()
            {
                CaseReference = "0121DO1",
                FamilyReference = new ReferenceDetails
                {
                    FirstName = "firstname",
                    LastName = "surname",
                    EmailAddress = "email",
                    PhoneNumber = "phone",
                    RelationshipToYou = "Relation",
                    NumberOfYearsKnown = "5",
                    Address = new Address
                    {
                        Postcode = "postcode",
                        PlaceRef = "123",
                        SelectedAddress = "Address",
                        AddressLine1 = "",
                        AddressLine2 = "",
                        Town = ""
                    }
                },
                FirstPersonalReference = new ReferenceDetails
                {
                    Address = new Address()
                },
                SecondPersonalReference = new ReferenceDetails
                {
                    Address = new Address()
                }
            };

            // Act
            await _service.UpdateReferences(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReferences_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            var model = new FosteringCaseReferenceUpdateModel()
            {
                CaseReference = "0121DO1",
                FamilyReference = new ReferenceDetails
                {
                    FirstName = "firstname",
                    LastName = "surname",
                    EmailAddress = "email",
                    PhoneNumber = "phone",
                    RelationshipToYou = "Relation",
                    NumberOfYearsKnown = "5",
                    Address = new Address
                    {
                        Postcode = "postcode",
                        PlaceRef = "123",
                        SelectedAddress = "Address",
                        AddressLine1 = "",
                        AddressLine2 = "",
                        Town = ""
                    }
                },
                FirstPersonalReference = new ReferenceDetails
                {
                    Address = new Address()
                },
                SecondPersonalReference = new ReferenceDetails
                {
                    Address = new Address()
                }
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateReferences(model));
        }

        [Fact]
        public async Task GetCase_ShouldSetEnableAdditionalInformationToTrueWhenDefinitionNameIsSetToFosteringApplication()
        {

            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .WithDefinitionName("Fostering_Application")
                .Build();

            //Arrange
            _verintServiceGatewayMock.Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK, 
                    ResponseContent = entity
                });


            //Act
            var result = await _service.GetCase("123");

            //Assert
            Assert.True(result.EnableAdditionalInformationSection);
        }

        [Fact]
        public async Task GetCase_ShouldSetEnableAdditionalInformationToFalseWhenDefinitionNameIsNotFosteringApplication()
        {

            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .WithDefinitionName("Not_Appliation")
                .Build();

            //Arrange
            _verintServiceGatewayMock.Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            //Act
            var result = await _service.GetCase("123");

            //Assert
            Assert.False(result.EnableAdditionalInformationSection);
        }
    }
}
