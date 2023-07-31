using fostering_service.Extensions;
using StockportGovUK.NetStandard.Gateways.Enums;
using Xunit;

namespace fostering_service_tests.Extensions
{
    public class ETaskStatusExtensionTests
    {

        [Theory]
        [InlineData("None", ETaskStatus.None)]
        [InlineData("CantStart", ETaskStatus.CantStart)]
        [InlineData("Completed", ETaskStatus.Completed)]
        [InlineData("NotCompleted", ETaskStatus.NotCompleted)]
        public void GetTaskStatus_ShouldReturnCorrectTaskStatus(string expected, ETaskStatus taskStatus)
        {
            Assert.Equal(expected, taskStatus.GetTaskStatus());
        }
    }
}
