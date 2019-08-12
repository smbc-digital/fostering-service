using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Builder;
using fostering_service.Extensions;
using fostering_service.Mappers;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering.Application;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;

namespace fostering_service.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly IVerintServiceGateway _verintServiceGateway;
        private readonly ILogger<ApplicationService> _logger;
        private readonly string _applicationFormName = "Fostering_Application";

        public ApplicationService(IVerintServiceGateway verintServiceGateway, ILogger<ApplicationService> logger)
        {
            _verintServiceGateway = verintServiceGateway;
            _logger = logger;
        }

        public async Task UpdateStatus(string caseId, ETaskStatus status, EFosteringApplicationForm form)
        {
            var fields = new FormFieldBuilder()
                .AddField(form.GetFormStatusFieldName(), status.GetTaskStatus())
                .Build();

            var response = await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = caseId,
                IntegrationFormName = _applicationFormName,
                IntegrationFormFields = fields
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(
                    $"Application Service. UpdateStatus: Failed to update status. Verint service response: {response}");
            }
        }

        public async Task<ETaskStatus> UpdateGpDetails(FosteringCaseGpDetailsUpdateModel model)
        {
            var firstApplicantFormFields = new FormFieldBuilder()
                .AddField("nameofgp", model.FirstApplicant.NameOfGp)
                .AddField("nameofpractice", model.FirstApplicant.NameOfGpPractice)
                .AddField("gpphonenumber", model.FirstApplicant.GpPhoneNumber)
                .AddField(EFosteringApplicationForm.GpDetails.GetFormStatusFieldName(), ETaskStatus.Completed.GetTaskStatus())
                .Build();
            firstApplicantFormFields.AddRange(AddressMapper.MapToVerintAddress(model.FirstApplicant.GpAddress, "addressofpractice", "placerefofpractice", "postcodeofpractice"));

            var secondApplicantFormFields = new List<IntegrationFormField>();

            if (model.SecondApplicant != null)
            {
                secondApplicantFormFields = new FormFieldBuilder()
                    .AddField("nameofgp2", model.SecondApplicant.NameOfGp)
                    .AddField("nameofpractice2", model.SecondApplicant.NameOfGpPractice)
                    .AddField("gpphonenumber2", model.SecondApplicant.GpPhoneNumber)
                    .Build();
                secondApplicantFormFields.AddRange(AddressMapper.MapToVerintAddress(model.SecondApplicant.GpAddress, "addressofpractice2", "placerefofpractice2", "postcodeofpractice2"));
            }

            var result = await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = firstApplicantFormFields.Concat(secondApplicantFormFields).ToList(),
                IntegrationFormName = _applicationFormName
            });

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update Gp details failure");
            }

            return ETaskStatus.Completed;
        }

        public async Task<ETaskStatus> UpdateReferences(FosteringCaseReferenceUpdateModel model)
        {
            var formFields = new FormFieldBuilder()
                .AddField("prffirstname", model.FamilyReference.FirstName)
                .AddField("prflastname", model.FamilyReference.LastName)
                .AddField("prfrelation", model.FamilyReference.RelationshipToYou)
                .AddField("prfyears", model.FamilyReference.NumberOfYearsKnown)
                .AddField("prfemail", model.FamilyReference.EmailAddress)
                .AddField("prfcontact", model.FamilyReference.PhoneNumber)
                .AddField("prf11firstname", model.FirstPersonalReference.FirstName)
                .AddField("prf1lastname", model.FirstPersonalReference.LastName)
                .AddField("prf1relation", model.FirstPersonalReference.RelationshipToYou)
                .AddField("prf1years", model.FirstPersonalReference.NumberOfYearsKnown)
                .AddField("prf1email", model.FirstPersonalReference.EmailAddress)
                .AddField("prf1contact", model.FirstPersonalReference.PhoneNumber)
                .AddField("prf2firstname", model.SecondPersonalReference.FirstName)
                .AddField("prf2lastname", model.SecondPersonalReference.LastName)
                .AddField("prf2relation", model.SecondPersonalReference.RelationshipToYou)
                .AddField("prf2years", model.SecondPersonalReference.NumberOfYearsKnown)
                .AddField("prf2email", model.SecondPersonalReference.EmailAddress)
                .AddField("prf2contact", model.SecondPersonalReference.PhoneNumber)
                .AddField(EFosteringApplicationForm.References.GetFormStatusFieldName(), ETaskStatus.Completed.GetTaskStatus())
                .Build();

            formFields.AddRange(AddressMapper.MapToVerintAddress(model.FamilyReference.Address, "prfaddress",
                "prfplaceref", "prfpostcode"));

            formFields.AddRange(AddressMapper.MapToVerintAddress(model.FirstPersonalReference.Address, "prf1address",
                "prf1placeref", "prf1postcode"));

            formFields.AddRange(AddressMapper.MapToVerintAddress(model.SecondPersonalReference.Address, "prf2address",
                "prf2placeref", "prf2postcode"));

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                IntegrationFormName = _applicationFormName,
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields
            };

            var response = await _verintServiceGateway
                .UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update references failure");
            }

            return ETaskStatus.Completed;
        }

        public Task<ETaskStatus> UpdateAddressHistory(FosteringCaseAddressHistoryUpdateModel model)
        {
            var builder = new FormFieldBuilder();

            throw new NotImplementedException();
        }
    }
}
