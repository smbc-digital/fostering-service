﻿using System;
using System.Collections.Generic;
using fostering_service.Services;
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
    public class BlockFormUpdateAttributeTests
    {
        private readonly BlockFormUpdateAttribute _attribute = new BlockFormUpdateAttribute();
        private readonly Mock<IFosteringService> _mockFosteringService = new Mock<IFosteringService>();
        private readonly Mock<ILogger<BlockFormUpdateAttribute>> _mockLogger = new Mock<ILogger<BlockFormUpdateAttribute>>();
        private readonly Mock<IServiceProvider> _mockRequestServices = new Mock<IServiceProvider>();
        private readonly ActionExecutingContext _actionExecutingContext;

        public BlockFormUpdateAttributeTests()
        {
            var actionArguments = new Dictionary<string, object>
            {
                { "model", new TestingModel() }
            };

            _mockRequestServices
                .Setup(_ => _.GetService(typeof(IFosteringService)))
                .Returns(_mockFosteringService.Object);

            _mockRequestServices
                .Setup(_ => _.GetService(typeof(ILogger<BlockFormUpdateAttribute>)))
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
            _mockFosteringService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    HomeVisitDateTime = DateTime.MaxValue
                });

            // Act
            _attribute.OnActionExecuting(_actionExecutingContext);

            // Assert
            _mockFosteringService.Verify(_ => _.GetCase(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void OnActionExecuting_ShouldThrowException()
        {
            // Arrange
            _mockFosteringService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .Throws(new Exception());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _attribute.OnActionExecuting(_actionExecutingContext));
            Assert.Contains("BlockFormUpdateAttribute: Error getting case with reference", ex.Message);
        }

        [Fact]
        public void OnActionExecuting_ShouldReturn423StatusCode()
        {
            // Arrange
            _mockFosteringService
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
            _mockFosteringService
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
            _mockFosteringService
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new FosteringCase
                {
                    HomeVisitDateTime = DateTime.MinValue
                });

            // Act 
            _attribute.OnActionExecuting(_actionExecutingContext);

            // Assert 
            Assert.Null(_actionExecutingContext.Result);
            _mockRequestServices.Verify(_ => _.GetService(typeof(ILogger<BlockFormUpdateAttribute>)), Times.Once);
        }

    }

    internal class TestingModel
    {
        public string CaseReference { get; set; } = "12345678";
    }
}