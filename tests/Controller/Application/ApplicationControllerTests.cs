﻿using fostering_service.Controllers.Application;
using fostering_service.Services.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Fostering.Application;
using Xunit;
using ETaskStatus = StockportGovUK.NetStandard.Gateways.Enums.ETaskStatus;
using OkObjectResult = Microsoft.AspNetCore.Mvc.OkObjectResult;

namespace fostering_service_tests.Controller.Application
{
    public class ApplicationControllerTests
    {
        private readonly ApplicationController _application;
        private readonly Mock<IApplicationService> _mockApplicationService = new Mock<IApplicationService>();
        private readonly Mock<ILogger<ApplicationController>> _mockLogger = new Mock<ILogger<ApplicationController>>();

        public ApplicationControllerTests()
        {
            _application = new ApplicationController(_mockApplicationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldCallFosteringService()
        {
            //Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateGpDetails(It.IsAny<FosteringCaseGpDetailsUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);
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

            //Act
            await _application.UpdateGpDetails(model);

            //Assert
            _mockApplicationService.Verify(_ => _.UpdateGpDetails(model), Times.Once);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldReturn200()
        {
            //Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateGpDetails(It.IsAny<FosteringCaseGpDetailsUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);
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

            //Act
            var result = await _application.UpdateGpDetails(model);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldReturn500()
        {
            //Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateGpDetails(It.IsAny<FosteringCaseGpDetailsUpdateModel>()))
                .ThrowsAsync(new Exception());
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

            //Act
            var result = await _application.UpdateGpDetails(model);

            //Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateReferences_ShouldCallFosteringService()
        {
            // Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _application.UpdateReferences(new FosteringCaseReferenceUpdateModel());

            // Assert
            _mockApplicationService
                .Verify(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReferences_ShouldReturn200()
        {
            // Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _application.UpdateReferences(new FosteringCaseReferenceUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateReferences_ShouldReturn500()
        {
            // Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _application.UpdateReferences(new FosteringCaseReferenceUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateStatus_ShouldCallApplicationService()
        {
            // Act
            await _application.UpdateStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            _mockApplicationService.Verify(_ => _.UpdateStatus(It.IsAny<string>(), It.IsAny<ETaskStatus>(), It.IsAny<EFosteringApplicationForm>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturn200()
        {
            // Act
            var result = await _application.UpdateStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturn500()
        {
            // Arrange
            _mockApplicationService
                .Setup(_ => _.UpdateStatus(It.IsAny<string>(), It.IsAny<ETaskStatus>(),
                    It.IsAny<EFosteringApplicationForm>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _application.UpdateStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldCallApplicationService()
        {
            // Arrange
            var model = new FosteringCaseCouncillorsUpdateModel
            {
                CaseReference = "12345678"
            };

            // Act
            await _application.UpdateCouncillorsDetails(model);

            // Assert
            _mockApplicationService.Verify(_ => _.UpdateCouncillorsDetails(model), Times.Once);
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldReturn200()
        {
            // Act
            var result = await _application.UpdateCouncillorsDetails(new FosteringCaseCouncillorsUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateCouncillorsDetails_ShouldReturn500()
        {
            // Arrange 
            _mockApplicationService
                .Setup(_ => _.UpdateCouncillorsDetails(It.IsAny<FosteringCaseCouncillorsUpdateModel>()))
                .Throws(new Exception());

            // Act
            var result = await _application.UpdateCouncillorsDetails(new FosteringCaseCouncillorsUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn200()
    {
            // Act
            var result = await _application.UpdateAddressHistory(new FosteringCaseAddressHistoryUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAddressHistory_ShouldReturn500()
        {
            // Arrange 
            _mockApplicationService
                .Setup(_ => _.UpdateAddressHistory(It.IsAny<FosteringCaseAddressHistoryUpdateModel>()))
                .Throws(new Exception());

            // Act
            var result = await _application.UpdateAddressHistory(new FosteringCaseAddressHistoryUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }
    }
}
