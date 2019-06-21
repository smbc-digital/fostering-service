using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Builder;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;

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
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth")?.Value ?? string.Empty,
                    CurrentEmployer = integrationFormFields.FirstOrDefault(_ => _.Name == "currentemployer")?.Value ?? string.Empty,
                    JobTitle = integrationFormFields.FirstOrDefault(_ => _.Name == "jobtitle")?.Value ?? string.Empty
                }
            };

            if (!string.IsNullOrEmpty(integrationFormFields.FirstOrDefault(_ => _.Name == "employed").Value))
            {
                fosteringCase.FirstApplicant.AreYouEmployed = integrationFormFields.FirstOrDefault(_ => _.Name == "employed").Value.ToLower() == "yes";
            }

            if (!string.IsNullOrEmpty(integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork")?.Value))
            {
                fosteringCase.FirstApplicant.CurrentHoursOfWork = (EHoursOfWork) Enum.Parse(typeof(EHoursOfWork),
                    integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork").Value, true);
            }

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
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth2")?.Value ?? string.Empty,
                    CurrentEmployer = integrationFormFields.FirstOrDefault(_ => _.Name == "currentemployer2")?.Value ?? string.Empty,
                    JobTitle = integrationFormFields.FirstOrDefault(_ => _.Name == "jobtitle2")?.Value ?? string.Empty,
                };

                if (!string.IsNullOrWhiteSpace(integrationFormFields.FirstOrDefault(_ => _.Name == "employed2").Value))
                {
                    fosteringCase.SecondApplicant.AreYouEmployed = integrationFormFields.FirstOrDefault(_ => _.Name == "employed2").Value.ToLower() == "yes";
                }

                if (!string.IsNullOrEmpty(integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork2")?.Value))
                {
                    fosteringCase.FirstApplicant.CurrentHoursOfWork = (EHoursOfWork)Enum.Parse(typeof(EHoursOfWork),
                        integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork2").Value, true);
                }
            }

            return fosteringCase;
        }

        // TODO: add country of birth to models
        public async Task UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model)
        {
            var completed = UpdateAboutYourselfIsValid(model.FirstApplicant);

            var formFields = new FormFieldBuilder()
                .AddField("previousname", model.FirstApplicant.AnotherName)
                .AddField("ethnicity", model.FirstApplicant.Ethnicity)
                .AddField("gender", model.FirstApplicant.Gender)
                .AddField("nationality", model.FirstApplicant.Nationality)
                .AddField("religionorfaithgroup", model.FirstApplicant.Religion)
                .AddField("sexualorientation", model.FirstApplicant.SexualOrientation);

            if (model.SecondApplicant != null)
            {
                completed = completed && UpdateAboutYourselfIsValid(model.SecondApplicant);

                formFields
                    .AddField("previousname_2", model.SecondApplicant.AnotherName)
                    .AddField("ethnicity2", model.SecondApplicant.Ethnicity)
                    .AddField("gender2", model.SecondApplicant.Gender)
                    .AddField("nationality2", model.SecondApplicant.Nationality)
                    .AddField("religionorfaithgroup2", model.SecondApplicant.Religion)
                    .AddField("sexualorientation2", model.SecondApplicant.SexualOrientation);
            }

            formFields.AddField(GetFormStatusFieldName(EFosteringCaseForm.TellUsAboutYourself),
                GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted));

            // Call update integration form fields in verint service call with formFields.Build()

            // Log error or warning if http status not 200
        }

        public async Task UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model)
        {

            var formFields = new FormFieldBuilder();
            var completed = UpdateAboutEmploymentIsCompleted(model.FirstApplicant);

            if (model.FirstApplicant.AreYouEmployed.Value)
            {
                formFields
                    .AddField("employed", model.FirstApplicant.AreYouEmployed.Value.ToString())
                    .AddField("jobtitle", model.FirstApplicant.JobTitle)
                    .AddField("currenemployer", model.FirstApplicant.CurrentEmployer)
                    .AddField("hoursofwork",
                        Enum.GetName(typeof(EHoursOfWork), model.FirstApplicant.CurrentHoursOfWork));
            }
            else
            {
                formFields
               .AddField("employed", model.FirstApplicant.AreYouEmployed.Value.ToString())
                    .AddField("jobtitle",string.Empty)
                    .AddField("currenemployer", string.Empty)
                    .AddField("hoursofwork", 
                        Enum.GetName(typeof(EHoursOfWork), null));
            }

            if (model.SecondApplicant != null)
            {

                completed = UpdateAboutEmploymentIsCompleted(model.SecondApplicant);
                if (model.FirstApplicant.AreYouEmployed.Value)
                {
                    formFields
                        .AddField("employed2", model.FirstApplicant.AreYouEmployed.Value.ToString())
                        .AddField("jobtitle2", model.FirstApplicant.JobTitle)
                        .AddField("currenemployer2", model.FirstApplicant.CurrentEmployer)
                        .AddField("hoursofwork2",
                            Enum.GetName(typeof(EHoursOfWork), model.FirstApplicant.CurrentHoursOfWork));
                }
                else
                {
                    formFields
                        .AddField("employed2", model.FirstApplicant.AreYouEmployed.Value.ToString())
                        .AddField("jobtitle2", string.Empty)
                        .AddField("currenemployer2", string.Empty)
                        .AddField("hoursofwork2",
                            Enum.GetName(typeof(EHoursOfWork), null));
                }
            }

            formFields.AddField(GetFormStatusFieldName(EFosteringCaseForm.YourEmploymentDetails),
                GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted));

        }

        private bool UpdateAboutYourselfIsValid(FosteringCaseAboutYourselfApplicantUpdateModel model)
        {
            return !string.IsNullOrEmpty(model.Ethnicity) &&
                !string.IsNullOrEmpty(model.Gender) &&
                !string.IsNullOrEmpty(model.Nationality) &&
                !string.IsNullOrEmpty(model.Religion) &&
                !string.IsNullOrEmpty(model.SexualOrientation) &&
                (!model.EverBeenKnownByAnotherName || !string.IsNullOrEmpty(model.AnotherName));
        }

        private bool UpdateAboutEmploymentIsCompleted(FosteringCaseYourEmploymentDetailsApplicantUpdateModel model)
        {
            if (model.AreYouEmployed.Value == false)
            {
                return true;
            }

            if(model.AreYouEmployed.Value  && 
              !string.IsNullOrEmpty(model.JobTitle) &&
              !string.IsNullOrEmpty(model.CurrentEmployer) &&
              Enum.IsDefined(typeof(EHoursOfWork),model.CurrentHoursOfWork))
            {
                return true;
            }

            return false;
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