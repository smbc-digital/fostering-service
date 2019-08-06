using System;
using System.Collections.Generic;
using fostering_service.Services;
using fostering_service.Services.Case;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using Xunit;

namespace fostering_service.Attributes
{
    public class BlockApplicationUpdateTests
    {
        private readonly BlockApplicationUpdate _attribute = new BlockApplicationUpdate();
        private readonly Mock<ICaseService> _mockCaseService = new Mock<ICaseService>();
        private readonly Mock<ILogger<BlockApplicationUpdate>> _mockLogger = new Mock<ILogger<BlockApplicationUpdate>>();
        private readonly Mock<IServiceProvider> _mockRequestServices = new Mock<IServiceProvider>();
        private readonly ActionExecutingContext _actionExecutingContext;

        public BlockApplicationUpdateTests()
        {
            var actionArguments = new Dictionary<string, object>
            {
                { "model", new ApplicationTestingModel() }
            };

            _mockRequestServices
                .Setup(_ => _.GetService(typeof(ICaseService)))
                .Returns(_mockCaseService.Object);

            _mockRequestServices
                .Setup(_ => _.GetService(typeof(ILogger<BlockApplicationUpdate>)))
                .Returns(_mockLogger.Object);


            var httpContextMock = new Mock<HttpContext>();
            httpContextMock
                .SetupGet(_ => _.RequestServices)
                .Returns(_mockRequestServices.Object);

            var actionContext = new ActionContext(
                httpContextMock.Object,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                Mock.Of<ModelStateDictionary>()
            );

            _actionExecutingContext =
                new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, Mock.Of<Controller>());
        }

        [Fact]
        public void OnActionExecuting_ShouldCallFosteringService()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    HomeVisitDateTime = DateTime.MaxValue
                });

            // Act
            _attribute.OnActionExecuting(_actionExecutingContext);

            // Assert
            _mockCaseService.Verify(_ => _.GetCase(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void OnActionExecuting_ShouldThrowException()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .Throws(new Exception());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _attribute.OnActionExecuting(_actionExecutingContext));
            Assert.Contains("BlockApplicationUpdate: Error getting case with reference", ex.Message);
        }

        [Fact]
        public void OnActionExecuting_ShouldReturn423StatusCode()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    EnableAdditionalInformationSection = false
                });

            // Act
            _attribute.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.IsType<Http423Result>(_actionExecutingContext.Result);
        }

        [Fact]
        public void OnActionExecuting_ShouldReturnNull()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    EnableAdditionalInformationSection = true
                });

            // Act 
            _attribute.OnActionExecuting(_actionExecutingContext);

            // Assert 
            Assert.Null(_actionExecutingContext.Result);
        }
    }

    internal class ApplicationTestingModel
    {
        public string CaseReference { get; set; } = "12345678";
    }
}