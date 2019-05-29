using System;
using fostering_service.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace fostering_service_tests.Controllers
{
    public class ValuesControllerTests
    {
        private readonly ValuesController _valuesController;

        public ValuesControllerTests()
        {
            _valuesController = new ValuesController();
        }

        [Fact]
        public void Get_ShouldReturnOK()
        {
            // Act
            var response = _valuesController.Get();
            var statusResponse = response as OkObjectResult;
            
            // Assert
            Assert.NotNull(statusResponse);
            Assert.Equal(200, statusResponse.StatusCode);
        }
    }
}
