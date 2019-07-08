using System;
using System.Threading.Tasks;
using fostering_service.Controllers;
using fostering_service.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using Xunit;

namespace fostering_service_tests.Controller
{
    public class FosteringControllerTests
    {
        private readonly Mock<IFosteringService> _mockFosteringService = new Mock<IFosteringService>();
        private FosteringController _controller;

        public FosteringControllerTests()
        {
            _controller = new FosteringController(_mockFosteringService.Object);
        }

        [Fact]
        public async Task GetCase_ShouldCallFosteringService()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase());

            // Act
            await _controller.GetCase("12345");

            // Assert
            _mockFosteringService.Verify(_ => _.GetCase(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetCase_ShouldReturn200()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase());

            // Act
            var result = await _controller.GetCase("1234");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCase_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.GetCase("1234");

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateFormStatus_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdateFormStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateFormStatus_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateStatus(It.IsAny<string>(), It.IsAny<ETaskStatus>(), It.IsAny<EFosteringCaseForm>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateFormStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdateAboutYourself(new FosteringCaseAboutYourselfUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateAboutYourself(It.IsAny<FosteringCaseAboutYourselfUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateAboutYourself(new FosteringCaseAboutYourselfUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateYourEmploymentDetails_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdateYourEmploymentDetails(new FosteringCaseYourEmploymentDetailsUpdateModel());

            // Assert
           Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateYourEmploymentDetails_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateAboutYourself(It.IsAny<FosteringCaseAboutYourselfUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateAboutYourself(new FosteringCaseAboutYourselfUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdateLanguagesSpokenInYourHome(new FosteringCaseLanguagesSpokenInYourHomeUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePartnershipSatus_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdatePartnershipStatus(new FosteringCasePartnershipStatusUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateLanguagesSpokenInYourHome(It.IsAny<FosteringCaseLanguagesSpokenInYourHomeUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result =
                await _controller.UpdateLanguagesSpokenInYourHome(new FosteringCaseLanguagesSpokenInYourHomeUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdatePartnershipStatus(It.IsAny<FosteringCasePartnershipStatusUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdatePartnershipStatus(new FosteringCasePartnershipStatusUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateYourFosteringHistory_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdateYourFosteringHistory(new FosteringCaseYourFosteringHistoryUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateYourHealth_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateHealthStatus(It.IsAny<FosteringCaseHealthUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateHealthStatus(new FosteringCaseHealthUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateYourHealth_ShouldReturn200()
        {
            // Act
            var result = await _controller.UpdateHealthStatus(new FosteringCaseHealthUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldCallFosteringService()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _controller.UpdateInterestInFostering(new FosteringCaseInterestInFosteringUpdateModel());

            // Assert
            _mockFosteringService
                .Verify(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturn200()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _controller.UpdateInterestInFostering(new FosteringCaseInterestInFosteringUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateInterestInFostering(new FosteringCaseInterestInFosteringUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldCallFosteringService()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _controller.UpdateHousehold(new FosteringCaseHouseholdUpdateModel());

            // Assert
            _mockFosteringService
                .Verify(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldReturn200()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _controller.UpdateHousehold(new FosteringCaseHouseholdUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateHousehold(new FosteringCaseHouseholdUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }
    }
}
