using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using fostering_service.Models;
using fostering_service.Services;
using fostering_service_tests.Builders;
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

namespace fostering_service_tests.Service
{
    public class FosteringServiceTests
    {
        private readonly Mock<IVerintServiceGateway> _verintServiceGatewayMock = new Mock<IVerintServiceGateway>();
        private FosteringService _service;

        public FosteringServiceTests()
        {
            _service = new FosteringService(_verintServiceGatewayMock.Object);
        }

        [Fact]
        public async Task GetCase_ShouldCall_VerintService()
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
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
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

        [Theory]
        [InlineData("withpartner")]
        [InlineData("")]
        public async Task GetCase_ShouldOnlyHaveOneApplicant(string formField)
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
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("hasanothername", "True")
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
            Assert.Equal("Sexual orientation", result.FirstApplicant.SexualOrientation);
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
                            .WithIntegrationFormField("sexualorientation", "Sexual orientation")
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
                            .WithIntegrationFormField("sexualorientation2", "Sexual orientation")
                            .WithIntegrationFormField("religionorfaithgroup2", "Religion")
                            .WithIntegrationFormField("hasanothername2", "True")
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
            Assert.Equal("Sexual orientation", result.SecondApplicant.SexualOrientation);
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
                    SexualOrientation = "sexual-orientation",
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
                    SexualOrientation = "sexual-orientation",
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
                    SexualOrientation = "sexual-orientation",
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
                    SexualOrientation = "sexual-orientation",
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
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "sexualorientation" && match.FormFieldValue == model.FirstApplicant.SexualOrientation)
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
                    SexualOrientation = "sexual-orientation",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                },
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
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
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "sexualorientation2" && match.FormFieldValue == model.SecondApplicant.SexualOrientation)
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

            Assert.Equal(4, result.Count);
        }
    }
}
