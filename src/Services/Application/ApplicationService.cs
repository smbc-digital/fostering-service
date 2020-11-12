using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Builder;
using fostering_service.Extensions;
using fostering_service.Mappers;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Gateways.VerintService;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Fostering;
using StockportGovUK.NetStandard.Models.Fostering.Application;
using StockportGovUK.NetStandard.Models.Verint;
using StockportGovUK.NetStandard.Models.Verint.Update;

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

        public async Task<ETaskStatus> UpdateCouncillorsDetails(FosteringCaseCouncillorsUpdateModel model)
        {
            var builder = new FormFieldBuilder();

            CreateCouncillorsDetailsIntegratedFormFields(builder,
                model.FirstApplicant.HasContactWithCouncillor ? model.FirstApplicant.CouncillorRelationshipDetails : new List<CouncillorRelationshipDetailsUpdateModel>());

            CreateCouncillorsDetailsIntegratedFormFields(builder,
                model.SecondApplicant?.HasContactWithCouncillor != null && model.SecondApplicant.HasContactWithCouncillor
                    ? model.SecondApplicant.CouncillorRelationshipDetails
                    : new List<CouncillorRelationshipDetailsUpdateModel>(),
                true);

            builder
                .AddField("contactwithcouncillor1", model.FirstApplicant.HasContactWithCouncillor.ToString().ToLower())
                .AddField("contactwithcouncillor2", model.SecondApplicant?.HasContactWithCouncillor.ToString().ToLower() ?? string.Empty)
                .AddField(EFosteringApplicationForm.CouncillorsOrEmployees.GetFormStatusFieldName(), ETaskStatus.Completed.GetTaskStatus());

            var response = await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = builder.Build(),
                IntegrationFormName = _applicationFormName
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Application Service. UpdateCouncillorsDetails: Failed to update. Verint service response: {response}");
            }

            return ETaskStatus.Completed;
        }

        public async Task<ETaskStatus> UpdateAddressHistory(FosteringCaseAddressHistoryUpdateModel model)
        {
            var builder = new FormFieldBuilder();

            CreateAddressHistoryIntegratedFormFields(builder, model.FirstApplicant.AddressHistory);

            if (model.SecondApplicant != null)
            {
                CreateAddressHistoryIntegratedFormFields(builder, model.SecondApplicant != null ? model.SecondApplicant?.AddressHistory : new List<PreviousAddress>(), true);
            }

            var addressHistoryTaskStatus = AddressHistoryTaskStatus(model.FirstApplicant.AddressHistory) 
                                           && (model.SecondApplicant == null || AddressHistoryTaskStatus(model.SecondApplicant.AddressHistory));

            builder.AddField(EFosteringApplicationForm.AddressHistory.GetFormStatusFieldName(), addressHistoryTaskStatus ? ETaskStatus.Completed.GetTaskStatus() : ETaskStatus.NotCompleted.GetTaskStatus());

            var response = await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = builder.Build(),
                IntegrationFormName = _applicationFormName
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Application Service. UpdateAddressHistory: Failed to update. Verint service response: {response}");
            }

            return addressHistoryTaskStatus ? ETaskStatus.Completed : ETaskStatus.NotCompleted;
        }

        private void CreateCouncillorsDetailsIntegratedFormFields(FormFieldBuilder builder,
            List<CouncillorRelationshipDetailsUpdateModel> model, bool secondApplicant = false)
        {
            var applicantPrefix = secondApplicant ? "2" : "1";

            for (var i = 0; i < model.Count; i++)
            {
                var nameSuffix = i + 1;

                builder
                    .AddField($"councilloremployeename{applicantPrefix}{nameSuffix}",
                        model[i].CouncillorName ?? string.Empty)
                    .AddField($"councillorrelationship{applicantPrefix}{nameSuffix}",
                        model[i].Relationship ?? string.Empty);
            }

            for (var i = model.Count; i < 4; i++)
            {
                var nameSuffix = i + 1;

                builder
                    .AddField($"councilloremployeename{applicantPrefix}{nameSuffix}", string.Empty)
                    .AddField($"councillorrelationship{applicantPrefix}{nameSuffix}", string.Empty);
            }
        }
        
        private void CreateAddressHistoryIntegratedFormFields(FormFieldBuilder builder,
            List<PreviousAddress> model, bool secondApplicant = false)
        {
            var applicantSuffix = secondApplicant ? "2" : "1";

            builder
                .AddField($"currentdatefrommonthapplicant{applicantSuffix}", model[0].DateFrom?.Month.ToString() ?? string.Empty)
                .AddField($"currentdatefromyearapplicant{applicantSuffix}", model[0].DateFrom?.Year.ToString() ?? string.Empty);

            if (model.Count > 1)
            {
                for (var i = 1; i < model.Count; i++)
                {
                    builder
                        .AddField($"pa{i}applicant{applicantSuffix}",
                            model[i].Address.AddressLine1 + "|" + model[i].Address.AddressLine2 + "|" +
                            model[i].Address.Town + "|" + model[i].Address.County + "|" + model[i].Address.Country)
                        .AddField($"pa{i}postcodeapplicant{applicantSuffix}",
                            model[i].Address.Postcode ?? string.Empty)
                        .AddField($"pa{i}datefrommonthapplicant{applicantSuffix}", model[i].DateFrom?.Month.ToString() ?? string.Empty)
                        .AddField($"pa{i}datefromyearapplicant{applicantSuffix}", model[i].DateFrom?.Year.ToString() ?? string.Empty);

                    if (i == 8)
                    {
                        break;
                    }
                }
            }

            if (model.Count < 8)
            {
                for (int i = model.Count; i < 9; i++)
                {
                    builder
                        .AddField($"pa{i}applicant{applicantSuffix}", string.Empty)
                        .AddField($"pa{i}postcodeapplicant{applicantSuffix}", string.Empty)
                        .AddField($"pa{i}datefrommonthapplicant{applicantSuffix}", string.Empty)
                        .AddField($"pa{i}datefromyearapplicant{applicantSuffix}", string.Empty);
                }
            }

            if (model.Count > 8)
            {
                var additionalAddress = string.Empty;
                for (var i = 9; i < model.Count; i++)
                {
                    additionalAddress += model[i].Address.AddressLine1 + "|" + model[i].Address.AddressLine2 + "|" +
                                         model[i].Address.Town + "|" + model[i].Address.County + "|" +
                                         model[i].Address.Country + "|" + model[i].Address.Postcode + "|" + 
                                         model[i].DateFrom?.Month + "|" + model[i].DateFrom?.Year;

                    additionalAddress += (i == model.Count - 1) ? string.Empty : "—";
                }
                builder
                    .AddField($"addressadditionalinformation{applicantSuffix}", additionalAddress);
            }
            else
            {
                builder
                    .AddField($"addressadditionalinformation{applicantSuffix}", "");
            }
        }

        private bool AddressHistoryTaskStatus(List<PreviousAddress> address)
        {
            var isAddressWithinLastTenYears = address.Any(_ => _.DateFrom.HasValue && _.DateFrom.Value < DateTime.Now.AddYears(-10));

            return (address.Count > 1)
                ? isAddressWithinLastTenYears && address.Count > 1 && !address
                      .Skip(1)
                      .Any(previousAddress =>
                          previousAddress.DateFrom == null || string.IsNullOrEmpty(previousAddress.Address.AddressLine1)
                                                           || string.IsNullOrEmpty(previousAddress.Address.Town)
                                                           || string.IsNullOrEmpty(previousAddress.Address.Country))
                : isAddressWithinLastTenYears;
        }
    }
}
