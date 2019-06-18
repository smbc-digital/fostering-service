using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Services;
using fostering_service_tests.Builders;
using Moq;
using StockportGovUK.AspNetCore.Gateways.Response;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Verint;
using Xunit;

namespace fostering_service_tests.Service
{
    public class FosteringServiceTests
    {
        private readonly Mock<IVerintServiceGateway> _verintServiceGatewayMock = new Mock<IVerintServiceGateway>();
        private FosteringService _service;

        public FosteringServiceTests()
        {
            _service = new FosteringService(_verintServiceGatewayMock.Object);
        }

        [Fact]
        public async Task GetCase_ShouldCall_VerintService()
        {
            // Arrange 
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            await _service.GetCase("1234");

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.GetCase("1234"));
        }

        [Fact]
        public async Task GetCase_ShouldThrowException_OnNon200Response()
        {
            // Arrange 
            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetCase("1234"));
        }

        [Fact]
        public async Task GetCase_ShouldReturn_FosteringCase()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.IsType<FosteringCase>(result);
        }

        [Theory]
        [InlineData("withpartner")]
        [InlineData("")]
        public async Task GetCase_ShouldOnlyHaveOneApplicant(string formField)
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField(formField, "No")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("1234");

            // Assert
            Assert.NotNull(result.FirstApplicant);
            Assert.Null(result.SecondApplicant);
        }

        [Fact]
        public async Task GetCase_ShouldMapFirstApplicant_ToFosteringCase()
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("religionorfaithgroup", "Religion")
                .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                .WithIntegrationFormField("gender", "Gender")
                .WithIntegrationFormField("ethnicity", "Ethnicity")
                .WithIntegrationFormField("nationality", "Nationality")
                .WithIntegrationFormField("previousname", "Previous name")
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.Equal("First Name", result.FirstApplicant.FirstName);
            Assert.Equal("Last Name", result.FirstApplicant.LastName);
            Assert.True(result.FirstApplicant.EverBeenKnownByAnotherName);
            Assert.Equal("Previous name", result.FirstApplicant.AnotherName);
            Assert.Equal("Nationality", result.FirstApplicant.Nationality);
            Assert.Equal("Ethnicity", result.FirstApplicant.Ethnicity);
            Assert.Equal("Gender", result.FirstApplicant.Gender);
            Assert.Equal("Sexual orientation", result.FirstApplicant.SexualOrientation);
            Assert.Equal("Religion", result.FirstApplicant.Religion);
            Assert.NotNull(result.FirstApplicant);
            Assert.Null(result.SecondApplicant);
        }

        [Fact]
        public async Task GetCase_ShouldMapSecondApplicant_ToFosteringCase()
        {
            // Arrange
            var entity = new CaseBuilder()
                            .WithIntegrationFormField("religionorfaithgroup", "Religion")
                            .WithIntegrationFormField("sexualorientation", "Sexual orientation")
                            .WithIntegrationFormField("gender", "Gender")
                            .WithIntegrationFormField("ethnicity", "Ethnicity")
                            .WithIntegrationFormField("nationality", "Nationality")
                            .WithIntegrationFormField("previousname", "Previous name")
                            .WithIntegrationFormField("surname", "Last Name")
                            .WithIntegrationFormField("firstname", "First Name")
                            .WithIntegrationFormField("withpartner", "Yes")
                            .WithIntegrationFormField("firstname_2", "First Name")
                            .WithIntegrationFormField("surname_2", "Last Name")
                            .WithIntegrationFormField("previousname_2", "Previous name")
                            .WithIntegrationFormField("nationality2", "Nationality")
                            .WithIntegrationFormField("ethnicity2", "Ethnicity")
                            .WithIntegrationFormField("gender2", "Gender")
                            .WithIntegrationFormField("sexualorientation2", "Sexual orientation")
                            .WithIntegrationFormField("religionorfaithgroup2", "Religion")
                            .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act 
            var result = await _service.GetCase("1234");
        
            // Assert
            Assert.NotNull(result.FirstApplicant);
            Assert.NotNull(result.SecondApplicant);
            Assert.Equal("First Name", result.SecondApplicant.FirstName);
            Assert.Equal("Last Name", result.SecondApplicant.LastName);
            Assert.True(result.SecondApplicant.EverBeenKnownByAnotherName);
            Assert.Equal("Previous name", result.SecondApplicant.AnotherName);
            Assert.Equal("Nationality", result.SecondApplicant.Nationality);
            Assert.Equal("Ethnicity", result.SecondApplicant.Ethnicity);
            Assert.Equal("Gender", result.SecondApplicant.Gender);
            Assert.Equal("Sexual orientation", result.SecondApplicant.SexualOrientation);
            Assert.Equal("Religion", result.SecondApplicant.Religion);
        }

        [Theory]
        [InlineData("None", ETaskStatus.None)]
        [InlineData("CantStart", ETaskStatus.CantStart)]
        [InlineData("Completed", ETaskStatus.Completed)]
        [InlineData("NotCompleted", ETaskStatus.NotCompleted)]
        public async Task GetCase_ShouldMapStatus(string status, ETaskStatus expectedStatus)
        {
            // Arrange
            var entity = new CaseBuilder()
                .WithIntegrationFormField("surname", "Last Name")
                .WithIntegrationFormField("firstname", "First Name")
                .WithIntegrationFormField("tellusaboutyourselfstatus", status)
                .Build();

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = entity
                });

            // Act
            var result = await _service.GetCase("");

            // Assert
            Assert.Equal(expectedStatus, result.Statuses.TellUsAboutYourselfStatus);
        }

        // TODO Write this test once call to verint gateway is done
        public async Task UpdateStatus_ShouldCall_VerintService_WithCorrectStatusField()
        {
            // Arrange

            // Act

            // Assert
        }

    }
}
