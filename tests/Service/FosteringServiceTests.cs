using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using fostering_service.Services;
using fostering_service_tests.Builders;
using Moq;
using StockportGovUK.AspNetCore.Gateways.Response;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;
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

        [Fact]
        public async Task UpdateStatus_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _service.UpdateStatus("", ETaskStatus.None, EFosteringCaseForm.ChildrenLivingAwayFromYourHome);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateStatus("", ETaskStatus.None, EFosteringCaseForm.ChildrenLivingAwayFromYourHome));
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldCallVerintServiceGateway()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
                    AnotherName = "another-name",
                    Nationality = "nationality",
                    EverBeenKnownByAnotherName = true,
                    PlaceOfBirth = "place-of-birth"
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock.Verify(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAboutYourself_ShouldThrowError()
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
                    AnotherName = "another-name",
                    Nationality = "nationality",
                    EverBeenKnownByAnotherName = true,
                    PlaceOfBirth = "place-of-birth"
                }
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateAboutYourself(model));
        }

        [Theory]
        [InlineData(true, ETaskStatus.NotCompleted)]
        [InlineData(false, ETaskStatus.Completed)]
        public async Task UpdateAboutYourself_ShouldReturnETaskStatus(bool hasAnotherName, ETaskStatus expectedStatus)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                }
            };

            // Act
            var result = await _service.UpdateAboutYourself(model);

            // Assert
            Assert.Equal(expectedStatus, result);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == expectedStatus.ToString())
                )), Times.Once);
        }

        [Theory]
        [InlineData(false, "false")]
        [InlineData(null, "")]
        public async Task UpdateAboutYourself_ShouldMapApplicantToIntegratedFormFields(bool? hasAnotherName, string expectedAnotherName)
        {
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == "Completed") 
           )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel => 
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "previousname" && match.FormFieldValue == "") 
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel => 
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "ethnicity" && match.FormFieldValue == model.FirstApplicant.Ethnicity)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "gender" && match.FormFieldValue == model.FirstApplicant.Gender) 
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "nationality" && match.FormFieldValue == model.FirstApplicant.Nationality)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "placeofbirth" && match.FormFieldValue == model.FirstApplicant.PlaceOfBirth)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "religionorfaithgroup" && match.FormFieldValue == model.FirstApplicant.Religion)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "sexualorientation" && match.FormFieldValue == model.FirstApplicant.SexualOrientation)
            )), Times.Once);
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "hasanothername" && match.FormFieldValue == expectedAnotherName)
            )), Times.Once);
        }

        [Theory]
        [InlineData(false, "false")]
        [InlineData(null, "")]
        public async Task UpdateAboutYourself_ShouldMapSecondApplicant(bool? hasAnotherName, string expectedAnotherName)
        { 
            // Arrange
            _verintServiceGatewayMock
                .Setup(_ => _.UpdateCaseIntegrationFormField(It.IsAny<IntegrationFormFieldsUpdateModel>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            var model = new FosteringCaseAboutYourselfUpdateModel
            {
                CaseReference = "0121DO1",
                SecondApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = hasAnotherName
                },
                FirstApplicant = new FosteringCaseAboutYourselfApplicantUpdateModel
                {
                    Religion = "religion",
                    Ethnicity = "ethnicity",
                    Gender = "gender",
                    SexualOrientation = "sexual-orientation",
                    Nationality = "nationality",
                    PlaceOfBirth = "place-of-birth",
                    EverBeenKnownByAnotherName = false
                }
            };

            // Act
            await _service.UpdateAboutYourself(model);

            // Assert
            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "tellusaboutyourselfstatus" && match.FormFieldValue == "Completed")
           )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "previousname_2" && match.FormFieldValue == "")
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "ethnicity2" && match.FormFieldValue == model.SecondApplicant.Ethnicity)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "gender2" && match.FormFieldValue == model.SecondApplicant.Gender)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "nationality2" && match.FormFieldValue == model.SecondApplicant.Nationality)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "placeofbirth_2" && match.FormFieldValue == model.SecondApplicant.PlaceOfBirth)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "religionorfaithgroup2" && match.FormFieldValue == model.SecondApplicant.Religion)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "sexualorientation2" && match.FormFieldValue == model.SecondApplicant.SexualOrientation)
            )), Times.Once);

            _verintServiceGatewayMock
                .Verify(_ => _.UpdateCaseIntegrationFormField(It.Is<IntegrationFormFieldsUpdateModel>(updateModel =>
                    updateModel.IntegrationFormFields.Exists(match => match.FormFieldName == "hasanothername2" && match.FormFieldValue == expectedAnotherName)
            )), Times.Once);
        }
    }
}
