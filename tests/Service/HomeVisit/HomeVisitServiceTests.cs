using System.Net;
using fostering_service.Controllers.Case.Models;
using fostering_service.Services.HomeVisit;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models;
using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Fostering.HomeVisit;
using StockportGovUK.NetStandard.Gateways.Models.Verint.Update;
using StockportGovUK.NetStandard.Gateways.VerintService;
using Xunit;
using Model = StockportGovUK.NetStandard.Gateways.Models.Fostering;

namespace fostering_service_tests.Service.HomeVisit
{
    public class HomeVisitServiceTests
    {
        private readonly Mock<IVerintServiceGateway> _verintServiceGatewayMock = new Mock<IVerintServiceGateway>();
        private readonly Mock<ILogger<HomeVisitService>> _mockLogger = new Mock<ILogger<HomeVisitService>>();
        private readonly HomeVisitService _homeVisitService;

        public HomeVisitServiceTests()
        {
            _homeVisitService = new HomeVisitService(_verintServiceGatewayMock.Object, _mockLogger.Object);
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
            await _homeVisitService.UpdateStatus("", ETaskStatus.None, EFosteringHomeVisitForm.ChildrenLivingAwayFromYourHome);

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
            await Assert.ThrowsAsync<Exception>(() => _homeVisitService.UpdateStatus("", ETaskStatus.None, EFosteringHomeVisitForm.ChildrenLivingAwayFromYourHome));
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
            await _homeVisitService.UpdateAboutYourself(model);

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
            await Assert.ThrowsAsync<Exception>(() => _homeVisitService.UpdateAboutYourself(model));
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
            var result = await _homeVisitService.UpdateAboutYourself(model);

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
            await _homeVisitService.UpdateAboutYourself(model);

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
            await _homeVisitService.UpdateAboutYourself(model);

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
            await _homeVisitService.UpdateLanguagesSpokenInYourHome(model);

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
            await Assert.ThrowsAsync<Exception>(() => _homeVisitService.UpdateLanguagesSpokenInYourHome(model));
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
            var result = await _homeVisitService.UpdateLanguagesSpokenInYourHome(model);

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
            await _homeVisitService.UpdateLanguagesSpokenInYourHome(model);

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
            await _homeVisitService.UpdatePartnershipStatus(model);

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
            await _homeVisitService.UpdatePartnershipStatus(model);

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
            await _homeVisitService.UpdatePartnershipStatus(model);

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
            var result = await _homeVisitService.UpdatePartnershipStatus(model);

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
            var result = await _homeVisitService.UpdatePartnershipStatus(model);

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
            var result = await _homeVisitService.UpdatePartnershipStatus(model);

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
            var result = await _homeVisitService.UpdatePartnershipStatus(model);

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
            await _homeVisitService.UpdateYourFosteringHistory(model);

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
            await _homeVisitService.UpdateYourFosteringHistory(model);

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
            var result = await _homeVisitService.UpdateYourFosteringHistory(model);

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
            await _homeVisitService.UpdateHealthStatus(model);

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
            await _homeVisitService.UpdateHealthStatus(model);

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
            var result = await _homeVisitService.UpdateHealthStatus(model);

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
            await _homeVisitService.UpdateInterestInFostering(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "fiichildrenwithdisability" && field.FormFieldValue == "ChildrenWithDisabilities")
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
            await _homeVisitService.UpdateInterestInFostering(model);

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
            await _homeVisitService.UpdateInterestInFostering(model);

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
            await _homeVisitService.UpdateInterestInFostering(model);

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
            var result = await _homeVisitService.UpdateInterestInFostering(model);

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
            var result = await _homeVisitService.UpdateInterestInFostering(model);

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
            var result = await _homeVisitService.UpdateInterestInFostering(model);

            // Assert
            Assert.Equal(ETaskStatus.NotCompleted, result);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                updateModel => updateModel.IntegrationFormFields.Exists(field => field.FormFieldName == "tellusaboutyourinterestinfosteringstatus" && field.FormFieldValue == "NotCompleted")
            )), Times.Once);
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
            var result = _homeVisitService.CreateOtherPersonBuilder(config, peopleList).Build();

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
            await _homeVisitService.UpdateHousehold(model);

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
            await Assert.ThrowsAsync<Exception>(() => _homeVisitService.UpdateHousehold(model));
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
            var result = await _homeVisitService.UpdateHousehold(model);

            // Assert
            Assert.Equal(expectedStatus, result);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "yourhouseholdstatus" && match.FormFieldValue == expectedStatus.ToString())
                )), Times.Once);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldMapApplicantToIntegratedFormFields()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var dateOfBirth = new DateTime();
            var expectedDate = dateOfBirth.ToString("dd/MM/yyyy");

            var model = new FosteringCaseHouseholdUpdateModel()
            {
                CaseReference = "1234",
                OtherPeopleInYourHousehold = new List<OtherPerson>
                {
                    new OtherPerson
                    {
                        FirstName = "First",
                        LastName = "Last",
                        Gender = "Male",
                        DateOfBirth = dateOfBirth,
                        RelationshipToYou = "Relationship"
                    }
                },
                AnyOtherPeopleInYourHousehold = true,
                DoYouHaveAnyPets = true,
                PetsInformation = "Cat"
            };

            // Act
            await _homeVisitService.UpdateHousehold(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "yourhouseholdstatus" && match.FormFieldValue == "Completed")
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "opfirstname1" && match.FormFieldValue == model.OtherPeopleInYourHousehold[0].FirstName)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "oplastname1" && match.FormFieldValue == model.OtherPeopleInYourHousehold[0].LastName)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "opgender1" && match.FormFieldValue == model.OtherPeopleInYourHousehold[0].Gender)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "opdateofbirth1" && match.FormFieldValue == expectedDate)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "oprelationshiptoapplicant1" && match.FormFieldValue == model.OtherPeopleInYourHousehold[0].RelationshipToYou)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "otherpeopleinyourhousehold" && match.FormFieldValue == "Yes")
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "doyouhaveanypets" && match.FormFieldValue == "Yes")
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "listofpetsandanimals" && match.FormFieldValue == model.PetsInformation)
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
            await _homeVisitService.UpdateChildrenLivingAwayFromHome(model);

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
            await Assert.ThrowsAsync<Exception>(() => _homeVisitService.UpdateChildrenLivingAwayFromHome(model));
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
            var result = await _homeVisitService.UpdateChildrenLivingAwayFromHome(model);

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
            await _homeVisitService.UpdateChildrenLivingAwayFromHome(model);

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
            await _homeVisitService.UpdateChildrenLivingAwayFromHome(model);

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

    }
}
