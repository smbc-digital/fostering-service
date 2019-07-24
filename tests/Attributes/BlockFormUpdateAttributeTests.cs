using System;
using System.Collections.Generic;
using fostering_service.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Xunit;

namespace fostering_service.Attributes
{
    public class BlockFormUpdateAttributeTests
    {
        private readonly BlockFormUpdateAttribute _attribute = new BlockFormUpdateAttribute();
        private readonly Mock<IFosteringService> _mockFosteringService = new Mock<IFosteringService>();

        public BlockFormUpdateAttributeTests()
        {
            //_mockHttpContext
            //    .Setup(_ => _.ActionArguments)
            //    .Returns(new Dictionary<string, object> {
            //        {
            //            "model", new TestingModel()
            //        }
            //    });

            var mockRequestService = new Mock<IServiceProvider>();

            mockRequestService
                .Setup(_ => _.GetService(typeof(IFosteringService)))
                .Returns(_mockFosteringService);


        }

        [Fact(Skip = "WIP")]
        public void OnActionExecuting_ShouldCallFosteringService()
        {
            // Arrange


            // Act
            //_attribute.OnActionExecuting(_mockHttpContext.Object);

            // Assert
        }
    }

    internal class TestingModel
    {
        public string CaseReference { get; set; } = "12345678";
    }
}