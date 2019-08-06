using System;
using System.Threading.Tasks;
using fostering_service.Controllers.HomeVisit;
using fostering_service.Services.HomeVisit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering.HomeVisit;
using Xunit;

namespace fostering_service_tests.Controller.HomeVisit
{
    public class HomeVisitControllerTests
    {
        private readonly Mock<IHomeVisitService> _mockHomeVisitService = new Mock<IHomeVisitService>();
        private readonly Mock<ILogger<HomeVisitController>> _mockLogger = new Mock<ILogger<HomeVisitController>>();
        private readonly HomeVisitController _homeVisit;

        public HomeVisitControllerTests()
        {
            _homeVisit = new HomeVisitController(_mockHomeVisitService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpdateFormStatus_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdateFormStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateFormStatus_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateStatus(It.IsAny<string>(), It.IsAny<ETaskStatus>(), It.IsAny<EFosteringHomeVisitForm>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateFormStatus(new FosteringCaseStatusUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdateAboutYourself(new FosteringCaseAboutYourselfUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateAboutYourself(It.IsAny<FosteringCaseAboutYourselfUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateAboutYourself(new FosteringCaseAboutYourselfUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateYourEmploymentDetails_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdateYourEmploymentDetails(new FosteringCaseYourEmploymentDetailsUpdateModel());

            // Assert
           Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateYourEmploymentDetails_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateAboutYourself(It.IsAny<FosteringCaseAboutYourselfUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateAboutYourself(new FosteringCaseAboutYourselfUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdateLanguagesSpokenInYourHome(new FosteringCaseLanguagesSpokenInYourHomeUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePartnershipSatus_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdatePartnershipStatus(new FosteringCasePartnershipStatusUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateLanguagesSpokenInYourHome_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateLanguagesSpokenInYourHome(It.IsAny<FosteringCaseLanguagesSpokenInYourHomeUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result =
                await _homeVisit.UpdateLanguagesSpokenInYourHome(new FosteringCaseLanguagesSpokenInYourHomeUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdatePartnershipStatus_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdatePartnershipStatus(It.IsAny<FosteringCasePartnershipStatusUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdatePartnershipStatus(new FosteringCasePartnershipStatusUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateYourFosteringHistory_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdateYourFosteringHistory(new FosteringCaseYourFosteringHistoryUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateYourHealth_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateHealthStatus(It.IsAny<FosteringCaseHealthUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateHealthStatus(new FosteringCaseHealthUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateYourHealth_ShouldReturn200()
        {
            // Act
            var result = await _homeVisit.UpdateHealthStatus(new FosteringCaseHealthUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldCallFosteringService()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _homeVisit.UpdateInterestInFostering(new FosteringCaseInterestInFosteringUpdateModel());

            // Assert
            _mockHomeVisitService
                .Verify(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturn200()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _homeVisit.UpdateInterestInFostering(new FosteringCaseInterestInFosteringUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateInterestInFostering_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateInterestInFostering(It.IsAny<FosteringCaseInterestInFosteringUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateInterestInFostering(new FosteringCaseInterestInFosteringUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldCallFosteringService()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _homeVisit.UpdateHousehold(new FosteringCaseHouseholdUpdateModel());

            // Assert
            _mockHomeVisitService
                .Verify(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldReturn200()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _homeVisit.UpdateHousehold(new FosteringCaseHouseholdUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHousehold_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService.Setup(_ => _.UpdateHousehold(It.IsAny<FosteringCaseHouseholdUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateHousehold(new FosteringCaseHouseholdUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldCallFosteringService()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _homeVisit.UpdateChildrenLivingAwayFromHome(new FosteringCaseChildrenLivingAwayFromHomeUpdateModel());

            // Assert
            _mockHomeVisitService
                .Verify(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldReturn200()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _homeVisit.UpdateChildrenLivingAwayFromHome(new FosteringCaseChildrenLivingAwayFromHomeUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UUpdateChildrenLivingAwayFromHome_ShouldReturn500()
        {
            // Arrange
            _mockHomeVisitService
                .Setup(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _homeVisit.UpdateChildrenLivingAwayFromHome(new FosteringCaseChildrenLivingAwayFromHomeUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }
    }
}
