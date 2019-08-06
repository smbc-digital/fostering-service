using System;
using System.Collections.Generic;
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
    public class BlockHomeVisitUpdateTests
    {
        private readonly Mock<ICaseService> _mockCaseService = new Mock<ICaseService>();
        private readonly BlockHomeVisitUpdate _attribute = new BlockHomeVisitUpdate();
        private readonly Mock<ILogger<BlockHomeVisitUpdate>> _mockLogger = new Mock<ILogger<BlockHomeVisitUpdate>>();
        private readonly Mock<IServiceProvider> _mockRequestServices = new Mock<IServiceProvider>();
        private readonly ActionExecutingContext _actionExecutingContext;

        public BlockHomeVisitUpdateTests()
        {
            var actionArguments = new Dictionary<string, object>
            {
                { "model", new HomeVisitTestingModel() }
            };

            _mockRequestServices
                .Setup(_ => _.GetService(typeof(ICaseService)))
                .Returns(_mockCaseService.Object);

            _mockRequestServices
                .Setup(_ => _.GetService(typeof(ILogger<BlockHomeVisitUpdate>)))
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
            Assert.Contains("BlockHomeVisitUpdate: Error getting case with reference", ex.Message);
        }

        [Fact]
        public void OnActionExecuting_ShouldReturn423StatusCode()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    HomeVisitDateTime = DateTime.Now
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
                    HomeVisitDateTime = DateTime.Now.AddDays(1)
                });

            // Act 
            _attribute.OnActionExecuting(_actionExecutingContext);
            
            // Assert 
            Assert.Null(_actionExecutingContext.Result);
        }

        [Fact]
        public void OnActionExecuting_ShouldCallLogger()
        {
            // Arrange
            _mockCaseService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    HomeVisitDateTime = DateTime.MinValue
                });

            // Act 
            _attribute.OnActionExecuting(_actionExecutingContext);

            // Assert 
            Assert.Null(_actionExecutingContext.Result);
            _mockRequestServices.Verify(_ => _.GetService(typeof(ILogger<BlockHomeVisitUpdate>)), Times.Once);
        }

    }

    internal class HomeVisitTestingModel
    {
        public string CaseReference { get; set; } = "12345678";
    }
}