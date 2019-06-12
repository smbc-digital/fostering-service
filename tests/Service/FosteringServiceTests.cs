using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Services;
using Moq;
using StockportGovUK.AspNetCore.Gateways.Response;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
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
            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = CreateBaseCase()
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
            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = CreateBaseCase()
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
            var baseCase = CreateBaseCase();
            baseCase.IntegrationFormFields.Add(
                new CustomField
                {
                    Name = formField,
                    Value = "No"
                }
            );

            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = baseCase
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
            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = CreateBaseCase()
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.Equal("First Name", result.FirstApplicant.FirstName);
            Assert.Equal("Last Name", result.FirstApplicant.LastName);
            Assert.True(result.FirstApplicant.EverBeenKnownByAnotherName);
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
            var baseCase = CreateBaseCase();
            baseCase.IntegrationFormFields.Add(
                new CustomField
                {
                    Name = "withpartner",
                    Value = "Yes"
                }
            );
            baseCase.IntegrationFormFields.AddRange(CreateSecondApplicant());
            _verintServiceGatewayMock
                .Setup(_ => _.GetCase(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<Case>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = baseCase
                });

            // Act 
            var result = await _service.GetCase("1234");

            // Assert
            Assert.NotNull(result.FirstApplicant);
            Assert.NotNull(result.SecondApplicant);
            Assert.Equal("First Name", result.SecondApplicant.FirstName);
            Assert.Equal("Last Name", result.SecondApplicant.LastName);
            Assert.True(result.SecondApplicant.EverBeenKnownByAnotherName);
            Assert.Equal("Nationality", result.SecondApplicant.Nationality);
            Assert.Equal("Ethnicity", result.SecondApplicant.Ethnicity);
            Assert.Equal("Gender", result.SecondApplicant.Gender);
            Assert.Equal("Sexual orientation", result.SecondApplicant.SexualOrientation);
            Assert.Equal("Religion", result.SecondApplicant.Religion);
        }

        private Case CreateBaseCase()
        {
            return new Case
            {
                IntegrationFormFields = new List<CustomField>
                {
                    new CustomField
                    {
                        Name = "firstname",
                        Value = "First Name"
                    },
                    new CustomField
                    {
                        Name = "surname",
                        Value = "Last Name"
                    },
                    new CustomField
                    {
                        Name = "previousname",
                        Value = "Previous name"
                    },
                    new CustomField
                    {
                        Name = "nationality",
                        Value = "Nationality"
                    },
                    new CustomField
                    {
                        Name = "ethnicity",
                        Value = "Ethnicity"
                    },
                    new CustomField
                    {
                        Name = "gender",
                        Value = "Gender"
                    },
                    new CustomField
                    {
                        Name = "sexualorientation",
                        Value = "Sexual orientation"
                    },
                    new CustomField
                    {
                        Name = "religionorfaithgroup",
                        Value = "Religion"
                    }
                }
            };
        }

        private List<CustomField> CreateSecondApplicant()
        {
            return new List<CustomField>
            {
                new CustomField
                {
                    Name = "firstname_2",
                    Value = "First Name"
                },
                new CustomField
                {
                    Name = "surname_2",
                    Value = "Last Name"
                },
                new CustomField
                {
                    Name = "previousname2",
                    Value = "Previous name"
                },
                new CustomField
                {
                    Name = "nationality2",
                    Value = "Nationality"
                },
                new CustomField
                {
                    Name = "ethnicity2",
                    Value = "Ethnicity"
                },
                new CustomField
                {
                    Name = "gender2",
                    Value = "Gender"
                },
                new CustomField
                {
                    Name = "sexualorientation2",
                    Value = "Sexual orientation"
                },
                new CustomField
                {
                    Name = "religionorfaithgroup2",
                    Value = "Religion"
                }
            };
        }


    }
}
