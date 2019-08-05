using fostering_service.Controllers.Application;
using fostering_service.Services.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using System;
using System.Threading.Tasks;
using Xunit;

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
    }
}
