using System.Threading.Tasks;
using fostering_service.Controllers.Case;
using fostering_service.Services.Case;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockportGovUK.NetStandard.Models.Fostering;
using Xunit;
using System;

namespace fostering_service_tests.Controller.Case
{
    public class CaseControllerTests
    {
        private readonly CaseController _case;
        private readonly Mock<ICaseService> _mockCaseService = new Mock<ICaseService>();
        private readonly Mock<ILogger<CaseController>> _mockLogger = new Mock<ILogger<CaseController>>();

        public CaseControllerTests()
        {
            _case = new CaseController(_mockCaseService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCase_ShouldCallFosteringService()
        {
            // Arrange
            _mockCaseService.Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase());

            // Act 
            await _case.GetCase("12345");

            // Assert
            _mockCaseService.Verify(_ => _.GetCase(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetCase_ShouldReturn200()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase());

            // Act
            var result = await _case.GetCase("1234");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCase_ShouldReturn500()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _case.GetCase("1234");

            // Assert
            var resultType = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, resultType.StatusCode);
        }


    }
}

