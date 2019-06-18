﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service.Services
{
    public class FosteringService : IFosteringService
    {
        private readonly IVerintServiceGateway _verintServiceGateway;

        public FosteringService(IVerintServiceGateway verintServiceGateway)
        {
            _verintServiceGateway = verintServiceGateway;
        }


        public async Task<FosteringCase> GetCase(string caseId)
        {
            var response = await _verintServiceGateway.GetCase(caseId);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Fostering service exception. Verint service gateway failed to respond with OK. Response: {response}");
            }

            var integrationFormFields = response.ResponseContent.IntegrationFormFields;
            var hasSecondApplicant = integrationFormFields.FirstOrDefault(_ => _.Name == "withpartner")?.Value == "Yes" 
                                     && integrationFormFields.FirstOrDefault(_ => _.Name == "firstname_2")?.Value != null;

            var fosteringCase = new FosteringCase
            {
                Statuses = new FosteringCaseStatuses
                {
                    TellUsAboutYourselfStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "tellusaboutyourselfstatus")?.Value),
                    ChildrenLivingAwayFromYourHomeStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "childrenlivingawayfromyourhomestatus")?.Value),
                    LanguageSpokenInYourHomeStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "languagespokeninyourhomestatus")?.Value),
                    TellUsAboutYourInterestInFosteringStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "tellusaboutyourinterestinfosteringstatus")?.Value),
                    YourEmploymentDetailsStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "youremploymentdetailsstatus")?.Value),
                    YourFosteringHistoryStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "yourfosteringhistorystatus")?.Value),
                    YourHealthStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "yourhealthstatus")?.Value),
                    YourHouseholdStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "yourhouseholdstatus")?.Value),
                    YourPartnershipStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "yourpartnershipstatus")?.Value)
                },
                FirstApplicant = new FosteringApplicant
                {
                    FirstName = integrationFormFields.First(_ => _.Name == "firstname").Value,
                    LastName = integrationFormFields.First(_ => _.Name == "surname").Value,
                    EverBeenKnownByAnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname") != null,
                    AnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname")?.Value ?? string.Empty,
                    Nationality = integrationFormFields.FirstOrDefault(_ => _.Name == "nationality")?.Value ?? string.Empty,
                    Ethnicity = integrationFormFields.FirstOrDefault(_ => _.Name == "ethnicity")?.Value ?? string.Empty,
                    Gender = integrationFormFields.FirstOrDefault(_ => _.Name == "gender")?.Value ?? string.Empty,
                    SexualOrientation = integrationFormFields.FirstOrDefault(_ => _.Name == "sexualorientation")?.Value ?? string.Empty,
                    Religion = integrationFormFields.FirstOrDefault(_ => _.Name == "religionorfaithgroup")?.Value ?? string.Empty,
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth")?.Value ?? string.Empty
                }
            };

            if (hasSecondApplicant)
            {
                fosteringCase.SecondApplicant = new FosteringApplicant
                {
                    FirstName = integrationFormFields.First(_ => _.Name == "firstname_2").Value,
                    LastName = integrationFormFields.First(_ => _.Name == "surname_2").Value,
                    EverBeenKnownByAnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname_2") != null,
                    AnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname_2")?.Value ?? string.Empty,
                    Nationality = integrationFormFields.FirstOrDefault(_ => _.Name == "nationality2")?.Value ?? string.Empty,
                    Ethnicity = integrationFormFields.FirstOrDefault(_ => _.Name == "ethnicity2")?.Value ?? string.Empty,
                    Gender = integrationFormFields.FirstOrDefault(_ => _.Name == "gender2")?.Value ?? string.Empty,
                    SexualOrientation = integrationFormFields.FirstOrDefault(_ => _.Name == "sexualorientation2")?.Value ?? string.Empty,
                    Religion = integrationFormFields.FirstOrDefault(_ => _.Name == "religionorfaithgroup2")?.Value ?? string.Empty,
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth")?.Value ?? string.Empty
                };
            }

            return fosteringCase;
        }

        // TODO: add country of birth to models
        public async Task UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model)
        {
            var formFields = new List<CustomField>
            {
                new CustomField
                {
                    Value = model.FirstApplicant.AnotherName,
                    Name = "previousname"
                },
                new CustomField
                {
                    Value = model.FirstApplicant.Ethnicity,
                    Name = "ethnicity"
                },new CustomField
                {
                    Value = model.FirstApplicant.Gender,
                    Name = "gender"
                },
                new CustomField
                {
                    Value = model.FirstApplicant.Nationality,
                    Name = "nationality"
                },
                new CustomField
                {
                    Value = model.FirstApplicant.Religion,
                    Name = "religionorfaithgroup"
                },
                new CustomField
                {
                    Value = model.FirstApplicant.SexualOrientation,
                    Name = "sexualorientation"
                }
            };

            if (model.SecondApplicant != null)
            {
                formFields.AddRange(new List<CustomField>{
                    new CustomField
                    {
                        Value = model.FirstApplicant.AnotherName,
                        Name = "previousname"
                    },
                    new CustomField
                    {
                        Value = model.FirstApplicant.Ethnicity,
                        Name = "ethnicity"
                    },new CustomField
                    {
                        Value = model.FirstApplicant.Gender,
                        Name = "gender"
                    },
                    new CustomField
                    {
                        Value = model.FirstApplicant.Nationality,
                        Name = "nationality"
                    },
                    new CustomField
                    {
                        Value = model.FirstApplicant.Religion,
                        Name = "religionorfaithgroup"
                    },
                    new CustomField
                    {
                        Value = model.FirstApplicant.SexualOrientation,
                        Name = "sexualorientation"
                    }
                });
            }

            // Call update integration form fields in verint service

            // Log error or warning if http status not 200
        }

        // TODO: Call update integration form fields method
        public async Task UpdateStatus(string caseId, ETaskStatus status, EFosteringCaseForm form)
        {
            var formStatusFieldName = GetFormStatusFieldName(form);

            if (formStatusFieldName == null)
            {
                throw new NullReferenceException("Status field not found");
            }

            var formFields = new List<CustomField>
            {
                new CustomField
                {
                    Value = GetTaskStatus(status),
                    Name = formStatusFieldName
                }
            };

            // Call update integration form fields in verint service

            // Log error or warning if http status not 200

        }

        private ETaskStatus GetTaskStatus(string status)
        {
            switch (status)
            {
                case "CantStart":
                    return ETaskStatus.CantStart;
                case "Completed":
                    return ETaskStatus.Completed;
                case "NotCompleted":
                    return ETaskStatus.NotCompleted;
                default:
                    return ETaskStatus.None;
            }
        }

        private string GetTaskStatus(ETaskStatus status)
        {
            switch (status)
            {
                case ETaskStatus.CantStart:
                    return "CantStart";
                case ETaskStatus.Completed:
                    return "Completed";
                case ETaskStatus.NotCompleted:
                    return "NotCompleted";
                default:
                    return "None";
            }
        }

        private string GetFormStatusFieldName(EFosteringCaseForm form)
        {
            switch (form)
            {
                case EFosteringCaseForm.ChildrenLivingAwayFromYourHome:
                    return "childrenlivingawayfromyourhomestatus";
                case EFosteringCaseForm.LanguageSpokenInYourHome:
                    return "languagespokeninyourhomestatus";
                case EFosteringCaseForm.TellUsAboutYourInterestInFostering:
                    return "tellusaboutyourinterestinfosteringstatus";
                case EFosteringCaseForm.TellUsAboutYourself:
                    return "tellusaboutyourselfstatus";
                case EFosteringCaseForm.YourEmploymentDetails:
                    return "youremploymentdetailsstatus";
                case EFosteringCaseForm.YourFosteringHistory:
                    return "yourfosteringhistorystatus";
                case EFosteringCaseForm.YourHealth:
                    return "yourhealthstatus";
                case EFosteringCaseForm.YourHousehold:
                    return "yourhouseholdstatus";
                case EFosteringCaseForm.YourPartnership:
                    return "yourpartnershipstatus";
                default:
                    return null;
            }
        }
    }
}