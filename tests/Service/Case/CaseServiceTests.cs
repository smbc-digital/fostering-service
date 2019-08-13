using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Controllers.Case.Models;
using fostering_service.Services.Case;
using fostering_service_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.AspNetCore.Gateways.Response;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Verint;
using Xunit;


namespace fostering_service_tests.Service
{
    public class CaseServiceTests
    {
        private readonly Mock<IVerintServiceGateway> _verintServiceGatewayMock = new Mock<IVerintServiceGateway>();
        private readonly Mock<ILogger<CaseService>> _mockLogger = new Mock<ILogger<CaseService>>();
        private readonly CaseService _caseService;

        public CaseServiceTests()
        {
            _caseService = new CaseService(_verintServiceGatewayMock.Object, _mockLogger.Object);

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
            await _caseService.GetCase("1234");

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
            await Assert.ThrowsAsync<Exception>(() => _caseService.GetCase("1234"));
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
            var result = await _caseService.GetCase("1234");

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
            var result = await _caseService
.GetCase("1234");

            // Assert
            Assert.IsType<FosteringCase>(result);
            Assert.Equal(DateTime.Parse("26/06/2019 13:30"), result.HomeVisitDateTime);

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
            var result = await _caseService
.GetCase("1234");

            // Assert
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
            var result = await _caseService.GetCase("1234");

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
            var result = await _caseService.GetCase("");

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
            var result = await _caseService.GetCase("1234");

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
            var result = await _caseService.GetCase("1234");

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
            var result = await _caseService.GetCase("1234");

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
            var result = await _caseService.GetCase("");

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
            var result = await _caseService.GetCase("");

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
            var result = await _caseService.GetCase("1234");

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
            var result = await _caseService.GetCase("1234");

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
        public async Task GetCase_ShouldSetEnableAdditionalInformationToTrueWhenDefinitionNameIsSetToFosteringApplication()
        {

            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .WithEnquirySubject("Fostering")
                .WithEnquiryReason("Fostering Application")
                .WithEnquiryType("3. Application")
                .Build();

            //Arrange
            _verintServiceGatewayMock.Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });


            //Act
            var result = await _caseService.GetCase("123");

            //Assert
            Assert.True(result.EnableAdditionalInformationSection);
        }

        [Fact]
        public async Task GetCase_ShouldSetEnableAdditionalInformationToFalseWhenDefinitionNameIsNotFosteringApplication()
        {

            var entity = new CaseBuilder()
                .WithIntegrationFormField("firstname", "First name")
                .WithIntegrationFormField("surname", "Surname")
                .Build();

            //Arrange
            _verintServiceGatewayMock.Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            //Act
            var result = await _caseService.GetCase("123");

            //Assert
            Assert.False(result.EnableAdditionalInformationSection);
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
            var result = _caseService.CreateOtherPersonList(config, model);

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
            var result = _caseService.CreateOtherPersonList(config, model);

            // Assert
            Assert.Equal(model[0].Value, result[0].Gender);
            Assert.Equal(model[1].Value, result[0].LastName);
            Assert.Equal(model[2].Value, result[1].Gender);
            Assert.Equal(model[3].Value, result[1].LastName);

            Assert.Equal(2, result.Count);
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
            var result = _caseService.CreateOtherPersonList(config, model, 1);

            // Assert
            Assert.Equal(result[0].Address.AddressLine1, expectedLine1);
            Assert.Equal(result[0].Address.AddressLine2, expectedLine2);
            Assert.Equal(result[0].Address.Town, expectedTown);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateCouncillorRelationshipDetailsList_ShouldReturnDetails(bool isSecondApplicant)
        {
            // Arrange
            var formFields = new List<CustomField>();
            var applicantSuffix = isSecondApplicant ? "2" : "1";

            for (var i = 1; i <= 4; i++)
            {
                formFields.Add(new CustomField
                {
                    Name = $"councilloremployeename{applicantSuffix}{i}",
                    Value = $"Name{applicantSuffix}{i}"
                });
                formFields.Add(new CustomField
                {
                    Name = $"councillorrelationship{applicantSuffix}{i}",
                    Value = $"Relationship{applicantSuffix}{i}"
                });
            }

            // Act
            var result = _caseService.CreateCouncillorRelationshipDetailsList(formFields, isSecondApplicant);

            // Assert
            Assert.Equal(4, result.Count);
            for (var i = 1; i <= 4; i++)
            {
                Assert.True(result.Exists(_ => _.CouncillorName == $"Name{applicantSuffix}{i}" && _.Relationship == $"Relationship{applicantSuffix}{i}"));
            }
        }

        [Fact]
        public void CreateCouncillorRelationshipDetailsList_ShouldReturnEmptyList()
        {
            // Act
            var result = _caseService.CreateCouncillorRelationshipDetailsList(new List<CustomField>());

            // Assert
            Assert.Empty(result);
        }
    }
}

