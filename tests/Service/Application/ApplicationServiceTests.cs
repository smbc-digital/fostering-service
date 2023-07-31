using System.Net;
using fostering_service.Services.Application;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Fostering.Application;
using StockportGovUK.NetStandard.Gateways.Models.Verint.Update;
using StockportGovUK.NetStandard.Gateways.VerintService;
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

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var model = new FosteringCaseCouncillorsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = true,
                    CouncillorRelationshipDetails = new List<CouncillorRelationshipDetailsUpdateModel>
                    {
                        new CouncillorRelationshipDetailsUpdateModel
                        {
                            CouncillorName = "Name",
                            Relationship = "Relationship"
                        }
                    }
                }
            };

            // Act
            await _applicationService.UpdateCouncillorsDetails(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });
            var model = new FosteringCaseCouncillorsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = true,
                    CouncillorRelationshipDetails = new List<CouncillorRelationshipDetailsUpdateModel>
                    {
                        new CouncillorRelationshipDetailsUpdateModel
                        {
                            CouncillorName = "Name",
                            Relationship = "Relationship"
                        }
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _applicationService.UpdateCouncillorsDetails(model));
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldCreateIntegrationFormFields_BothApplicants()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var model = new FosteringCaseCouncillorsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = true,
                    CouncillorRelationshipDetails = new List<CouncillorRelationshipDetailsUpdateModel>
                    {
                        new CouncillorRelationshipDetailsUpdateModel
                        {
                            CouncillorName = "Name",
                            Relationship = "Relationship"
                        }
                    }
                },
                SecondApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = true,
                    CouncillorRelationshipDetails = new List<CouncillorRelationshipDetailsUpdateModel>
                    {
                        new CouncillorRelationshipDetailsUpdateModel
                        {
                            CouncillorName = "Name",
                            Relationship = "Relationship"
                        }
                    }
                }
            };

            // Act
            await _applicationService.UpdateCouncillorsDetails(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "contactwithcouncillor1" && field.FormFieldValue == "true")
                )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "contactwithcouncillor2" && field.FormFieldValue == "true")
                )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "councilloremployeename11" && field.FormFieldValue == "Name")
                )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "councilloremployeename21" && field.FormFieldValue == "Name")
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "councillorrelationship11" && field.FormFieldValue == "Relationship")
            )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "councillorrelationship21" && field.FormFieldValue == "Relationship")
            )), Times.Once);

            for (var i = 2; i < 5; i++)
            {
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councilloremployeename1{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councilloremployeename2{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councillorrelationship1{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councillorrelationship2{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
            }
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldCreateEmptyIntegrationFormFields()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var model = new FosteringCaseCouncillorsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = false
                },
                SecondApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = false
                }
            };

            // Act
            await _applicationService.UpdateCouncillorsDetails(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "contactwithcouncillor1" && field.FormFieldValue == "false")
                )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "contactwithcouncillor2" && field.FormFieldValue == "false")
                )), Times.Once);

            for (var i = 1; i < 5; i++)
            {
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councilloremployeename1{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councilloremployeename2{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councillorrelationship1{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councillorrelationship2{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
            }
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldCreateIntegrationFormFields()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var model = new FosteringCaseCouncillorsUpdateModel
            {
                CaseReference = "1234",
                FirstApplicant = new FosteringCaseCouncillorsApplicantUpdateModel
                {
                    HasContactWithCouncillor = false
                }
            };

            // Act
            await _applicationService.UpdateCouncillorsDetails(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "contactwithcouncillor1" && field.FormFieldValue == "false")
                )), Times.Once);
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                m => m.IntegrationFormFields.Exists(field => field.FormFieldName == "contactwithcouncillor2" && field.FormFieldValue == string.Empty)
                )), Times.Once);

            for (var i = 1; i < 5; i++)
            {
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councilloremployeename1{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councilloremployeename2{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councillorrelationship1{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
                _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(
                    m => m.IntegrationFormFields.Exists(field => field.FormFieldName == $"councillorrelationship2{i}" && field.FormFieldValue == string.Empty)
                )), Times.Once);
            }
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldThrowException_WhenResponseIsNotOkFrom_VerintService()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressline1",
                                Town = "Town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now
                        },
                    }
                }
            };
            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _applicationService.UpdateAddressHistory(model));
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldCall_VerintServiceGateway()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressline1",
                                Town = "Town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now
                        },
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>  _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK
                    });

            // Act & Assert
            await _applicationService.UpdateAddressHistory(model);

            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn_NotCompletedTaskStatus_WhenFirstAddress_IsNot10YearsFromToday()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress(),
                            DateFrom = DateTime.Now.AddYears(-9)
                        },
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _applicationService.UpdateAddressHistory(model);

            Assert.Equal(ETaskStatus.NotCompleted, result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn_CompletedTaskStatus_WhenFirstAddress_Is10YearsFromToday()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _applicationService.UpdateAddressHistory(model);

            Assert.Equal(ETaskStatus.Completed, result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn_CompletedTaskStatus_WhenMutlipleAddress_AreValid()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _applicationService.UpdateAddressHistory(model);

            Assert.Equal(ETaskStatus.Completed, result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn_NotCompletedTaskStatus_WhenMutlipleAddress_HaveAnInvalidAddressField()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = string.Empty,
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _applicationService.UpdateAddressHistory(model);

            Assert.Equal(ETaskStatus.NotCompleted, result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn_CompletedTaskStatus_WhenMutlipleAddress_OnBothApplications_AreValid()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                },
                SecondApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _applicationService.UpdateAddressHistory(model);

            Assert.Equal(ETaskStatus.Completed, result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn_NotCompletedTaskStatus_WhenMutlipleAddress_OnBothApplications_HaveAnInvalidAddressField()
        {
            // Arrange
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = string.Empty,
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine2",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                },
                SecondApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = string.Empty,
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _applicationService.UpdateAddressHistory(model);

            Assert.Equal(ETaskStatus.NotCompleted, result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldAddAllCorrectIntegrationFormFieldsForSingleApplicant()
        {
            var callback = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                }).Callback<IntegrationFormFieldsUpdateModel>((iff) => callback = iff);

            await _applicationService.UpdateAddressHistory(model);

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "addresshistorystatus" && _.FormFieldValue == "Completed"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "currentdatefrommonthapplicant1"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "currentdatefromyearapplicant1"));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1applicant1"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1postcodeapplicant1"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1datefrommonthapplicant1"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1datefromyearapplicant1"));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2datefromyearapplicant1" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3datefromyearapplicant1" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4datefromyearapplicant1" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5datefromyearapplicant1" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6datefromyearapplicant1" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7datefromyearapplicant1" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8applicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8postcodeapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8datefrommonthapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8datefromyearapplicant1" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "addressadditionalinformation1" && _.FormFieldValue == string.Empty));

            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "currentdatefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "currentdatefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2postcodeapplicant2" ));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7datefromyearapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8applicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8postcodeapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8datefrommonthapplicant2"));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8datefromyearapplicant2" ));
            Assert.False(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "addressadditionalinformation2"));
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldAddAllCorrectIntegrationFormFieldsForDualApplicant()
        {
            var callback = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                },
                SecondApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                }).Callback<IntegrationFormFieldsUpdateModel>((iff) => callback = iff);

            await _applicationService.UpdateAddressHistory(model);

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "addresshistorystatus" && _.FormFieldValue == "Completed"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "currentdatefrommonthapplicant2"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "currentdatefromyearapplicant2"));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1applicant2"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1postcodeapplicant2"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1datefrommonthapplicant2"));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa1datefromyearapplicant2"));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa2datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa3datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa4datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa5datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa6datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa7datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8applicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8postcodeapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8datefrommonthapplicant2" && _.FormFieldValue == string.Empty));
            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "pa8datefromyearapplicant2" && _.FormFieldValue == string.Empty));

            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "addressadditionalinformation2" && _.FormFieldValue == string.Empty));
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldAddAdditonalAddress_ToAdditionalInfomationField()
        {
            var callback = new IntegrationFormFieldsUpdateModel();
            var model = new FosteringCaseAddressHistoryUpdateModel
            {
                FirstApplicant = new FosteringCaseAddressHistoryApplicantUpdateModel
                {
                    AddressHistory = new List<PreviousAddress>
                    {
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine1",
                                Town = "town",
                                Country = "UK",
                            },
                            DateFrom = DateTime.Now.AddYears(-11)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        },
                        new PreviousAddress
                        {
                            Address = new InternationalAddress
                            {
                                AddressLine1 = "addressLine12",
                                Town = "town2",
                                Country = "UK2",
                            },
                            DateFrom = DateTime.Now.AddYears(-3)
                        }
                    }
                }
            };

            _verintServiceGatewayMock.Setup(_ =>
                    _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                }).Callback<IntegrationFormFieldsUpdateModel>((iff) => callback = iff);

            await _applicationService.UpdateAddressHistory(model);


            Assert.True(callback.IntegrationFormFields.Exists(_ => _.FormFieldName == "addressadditionalinformation1" && _.FormFieldValue != string.Empty));
        }
    }
}
