using System;
using fostering_service.Services.Application;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Application;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using Xunit;

namespace fostering_service_tests.Service.Application
{
    public class ApplicationServiceTests
    {
        private readonly Mock<IVerintServiceGateway> _verintServiceGatewayMock = new Mock<IVerintServiceGateway>();
        private readonly Mock<ILogger<ApplicationService>> _mockLogger = new Mock<ILogger<ApplicationService>>();
        private readonly ApplicationService _applicationService;

        public ApplicationServiceTests()
    {
    _applicationService = new ApplicationService(_verintServiceGatewayMock.Object, _mockLogger.Object);

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
            await _applicationService.UpdateGpDetails(model);

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
            await Assert.ThrowsAsync<Exception>(() => _applicationService.UpdateGpDetails(model));
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
            await _applicationService.UpdateGpDetails(model);

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
            await _applicationService.UpdateGpDetails(model);

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
            await _applicationService.UpdateReferences(model);

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
            await Assert.ThrowsAsync<Exception>(() => _applicationService.UpdateReferences(model));
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
            await _applicationService.UpdateStatus("1234", ETaskStatus.Completed, EFosteringApplicationForm.GpDetails);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ShouldThrowException()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _applicationService.UpdateStatus("1234", ETaskStatus.Completed, EFosteringApplicationForm.GpDetails));
        }
    }
}
