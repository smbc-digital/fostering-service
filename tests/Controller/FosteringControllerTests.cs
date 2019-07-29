using System;
using System.Threading.Tasks;
using fostering_service.Controllers;
using fostering_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly Mock<ILogger<FosteringController>> _mockLogger = new Mock<ILogger<FosteringController>>();
        private readonly FosteringController _controller;

        public FosteringControllerTests()
        {
            _controller = new FosteringController(_mockFosteringService.Object, _mockLogger.Object);
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
           Assert.IsType<OkObjectResult>(result);
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

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldCallFosteringService()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _controller.UpdateChildrenLivingAwayFromHome(new FosteringCaseChildrenLivingAwayFromHomeUpdateModel());

            // Assert
            _mockFosteringService
                .Verify(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateChildrenLivingAwayFromHome_ShouldReturn200()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _controller.UpdateChildrenLivingAwayFromHome(new FosteringCaseChildrenLivingAwayFromHomeUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UUpdateChildrenLivingAwayFromHome_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateChildrenLivingAwayFromHome(It.IsAny<FosteringCaseChildrenLivingAwayFromHomeUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateChildrenLivingAwayFromHome(new FosteringCaseChildrenLivingAwayFromHomeUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldCallFosteringService()
        {
            //Arrange
            _mockFosteringService
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
            await _controller.UpdateGpDetails(model);

            //Assert
            _mockFosteringService.Verify(_ => _.UpdateGpDetails(model), Times.Once);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldReturn200()
        {
            //Arrange
            _mockFosteringService
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
            var result = await _controller.UpdateGpDetails(model);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateReferences_ShouldCallFosteringService()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            await _controller.UpdateReferences(new FosteringCaseReferenceUpdateModel());

            // Assert
            _mockFosteringService
                .Verify(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReferences_ShouldReturn200()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()))
                .ReturnsAsync(ETaskStatus.Completed);

            // Act
            var result = await _controller.UpdateReferences(new FosteringCaseReferenceUpdateModel());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateGpDetails_ShouldReturn500()
        {
            //Arrange
            _mockFosteringService
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
            var result = await _controller.UpdateGpDetails(model);

            //Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }

        [Fact]
        public async Task UpdateReferences_ShouldReturn500()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.UpdateReferences(It.IsAny<FosteringCaseReferenceUpdateModel>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateReferences(new FosteringCaseReferenceUpdateModel());

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }
    }
}
