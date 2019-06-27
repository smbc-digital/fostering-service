using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
    }
}
