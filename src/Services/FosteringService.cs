﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Builder;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;

namespace fostering_service.Services
{
    public class FosteringService : IFosteringService
    {
        private readonly IVerintServiceGateway _verintServiceGateway;
        private readonly string _integrationFormName = "Fostering_Home_Visit";

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
                    AnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname")?.Value ?? string.Empty,
                    Nationality = integrationFormFields.FirstOrDefault(_ => _.Name == "nationality")?.Value ?? string.Empty,
                    Ethnicity = integrationFormFields.FirstOrDefault(_ => _.Name == "ethnicity")?.Value ?? string.Empty,
                    Gender = integrationFormFields.FirstOrDefault(_ => _.Name == "gender")?.Value ?? string.Empty,
                    SexualOrientation = integrationFormFields.FirstOrDefault(_ => _.Name == "sexualorientation")?.Value ?? string.Empty,
                    Religion = integrationFormFields.FirstOrDefault(_ => _.Name == "religionorfaithgroup")?.Value ?? string.Empty,
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth")?.Value ?? string.Empty,
                    CurrentEmployer = integrationFormFields.FirstOrDefault(_ => _.Name == "currentemployer")?.Value ?? string.Empty,
                    JobTitle = integrationFormFields.FirstOrDefault(_ => _.Name == "jobtitle")?.Value ?? string.Empty,
                    RegisteredDisabled = integrationFormFields.FirstOrDefault(_ => _.Name == "registereddisabled")?.Value.ToLower() == "yes",
                    Practitioner = integrationFormFields.FirstOrDefault(_ => _.Name == "practitioner")?.Value.ToLower() == "yes"
                },
                WithPartner = integrationFormFields.FirstOrDefault(_ => _.Name == "withpartner")?.Value ?? "yes",
                PrimaryLanguage = integrationFormFields.FirstOrDefault(_ => _.Name == "primarylanguage")?.Value ?? string.Empty,
                OtherLanguages = integrationFormFields.FirstOrDefault(_ => _.Name == "otherlanguages")?.Value ?? string.Empty,
                TypesOfFostering = new List<string>(),
                ReasonsForFostering = integrationFormFields.FirstOrDefault(_ => _.Name == "reasonsforfosteringapplicant1")?.Value ?? string.Empty
        };

            var marriedOrInACivilPartnership = integrationFormFields.FirstOrDefault(_ => _.Name == "marriedorinacivilpartnership")?.Value;
            if (!string.IsNullOrEmpty(marriedOrInACivilPartnership))
            {
                fosteringCase.MarriedOrInACivilPartnership = marriedOrInACivilPartnership.ToLower() == "yes";
            }

            var marriageDate = integrationFormFields.FirstOrDefault(_ => _.Name == "dateofreg")?.Value;
            if (!string.IsNullOrEmpty(marriageDate))
            {
                fosteringCase.DateOfMarriage = DateTime.Parse(marriageDate);
            }

            var movedInTogetherDate = integrationFormFields.FirstOrDefault(_ => _.Name == "datesetuphousehold")?.Value;
            if (!string.IsNullOrEmpty(movedInTogetherDate))
            {
                fosteringCase.DateMovedInTogether = DateTime.Parse(movedInTogetherDate);
            }

            var hasAnotherNameApplicant1 = integrationFormFields.FirstOrDefault(_ => _.Name == "hasanothername")?.Value;
            if (!string.IsNullOrEmpty(hasAnotherNameApplicant1))
            {
                fosteringCase.FirstApplicant.EverBeenKnownByAnotherName = hasAnotherNameApplicant1.ToLower() == "true";
            }

            if (!string.IsNullOrEmpty(integrationFormFields.FirstOrDefault(_ => _.Name == "employed")?.Value))
            {
                fosteringCase.FirstApplicant.AreYouEmployed = integrationFormFields.FirstOrDefault(_ => _.Name == "employed")?.Value.ToLower() == "yes";
            }

            if (!string.IsNullOrEmpty(integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork")?.Value))
            {
                fosteringCase.FirstApplicant.CurrentHoursOfWork = (EHoursOfWork)Enum.Parse(typeof(EHoursOfWork),
                    integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork")?.Value, true);
            }

            var hasPreviouslyApplied = integrationFormFields.FirstOrDefault(_ => _.Name == "previouslyappliedapplicant1")?.Value;
            if (!string.IsNullOrWhiteSpace(hasPreviouslyApplied))
            {
                fosteringCase.FirstApplicant.PreviouslyApplied = hasPreviouslyApplied.ToLower() == "yes";
            }

            if (integrationFormFields.Exists(_ => _.Name == "fiichildrenwithdisability"))
            {
                fosteringCase.TypesOfFostering.Add("childrenWithDisability");
            }

            if (integrationFormFields.Exists(_ => _.Name == "fiirespite"))
            {
                fosteringCase.TypesOfFostering.Add("respite");
            }

            if (integrationFormFields.Exists(_ => _.Name == "fiishortterm"))
            {
                fosteringCase.TypesOfFostering.Add("shortTerm");
            }

            if (integrationFormFields.Exists(_ => _.Name == "fiilongterm"))
            {
                fosteringCase.TypesOfFostering.Add("longTerm");
            }

            if (integrationFormFields.Exists(_ => _.Name == "fiiunsure"))
            {
                fosteringCase.TypesOfFostering.Add("unsure");
            }

            if (integrationFormFields.Exists(_ => _.Name == "fiishortbreaks"))
            {
                fosteringCase.TypesOfFostering.Add("shortBreaks");
            }

            if (hasSecondApplicant)
            {
                fosteringCase.SecondApplicant = new FosteringApplicant
                {
                    FirstName = integrationFormFields.First(_ => _.Name == "firstname_2").Value,
                    LastName = integrationFormFields.First(_ => _.Name == "surname_2").Value,
                    AnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname_2")?.Value ?? string.Empty,
                    Nationality = integrationFormFields.FirstOrDefault(_ => _.Name == "nationality2")?.Value ?? string.Empty,
                    Ethnicity = integrationFormFields.FirstOrDefault(_ => _.Name == "ethnicity2")?.Value ?? string.Empty,
                    Gender = integrationFormFields.FirstOrDefault(_ => _.Name == "gender2")?.Value ?? string.Empty,
                    SexualOrientation = integrationFormFields.FirstOrDefault(_ => _.Name == "sexualorientation2")?.Value ?? string.Empty,
                    Religion = integrationFormFields.FirstOrDefault(_ => _.Name == "religionorfaithgroup2")?.Value ?? string.Empty,
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth_2")?.Value ?? string.Empty,
                    CurrentEmployer = integrationFormFields.FirstOrDefault(_ => _.Name == "currentemployer2")?.Value ?? string.Empty,
                    JobTitle = integrationFormFields.FirstOrDefault(_ => _.Name == "jobtitle2")?.Value ?? string.Empty,
                    RegisteredDisabled = integrationFormFields.FirstOrDefault(_ => _.Name == "registereddisabled2")?.Value.ToLower() == "yes",
                    Practitioner = integrationFormFields.FirstOrDefault(_ => _.Name == "practitioner2")?.Value.ToLower() == "yes"
                };

                var hasAnotherNameApplicant2 = integrationFormFields.FirstOrDefault(_ => _.Name == "hasanothername2")?.Value;
                if (!string.IsNullOrEmpty(hasAnotherNameApplicant2))
                {
                    fosteringCase.SecondApplicant.EverBeenKnownByAnotherName =
                        hasAnotherNameApplicant2.ToLower() == "true";
                }

                if (!string.IsNullOrWhiteSpace(integrationFormFields.FirstOrDefault(_ => _.Name == "employed2")?.Value))
                {
                    fosteringCase.SecondApplicant.AreYouEmployed = integrationFormFields.FirstOrDefault(_ => _.Name == "employed2")?.Value.ToLower() == "yes";
                }

                if (!string.IsNullOrEmpty(integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork2")?.Value))
                {
                    fosteringCase.SecondApplicant.CurrentHoursOfWork = (EHoursOfWork)Enum.Parse(typeof(EHoursOfWork),
                        integrationFormFields.FirstOrDefault(_ => _.Name == "hoursofwork2")?.Value, true);
                }

                var hasPreviouslyAppliedApplicant2 = integrationFormFields.FirstOrDefault(_ => _.Name == "previouslyappliedapplicant2")?.Value;
                if (!string.IsNullOrWhiteSpace(hasPreviouslyAppliedApplicant2))
                {
                    fosteringCase.SecondApplicant.PreviouslyApplied = hasPreviouslyAppliedApplicant2.ToLower() == "yes";
                }
            }

            return fosteringCase;
        }

        public async Task<ETaskStatus> UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model)
        {
            var completed = UpdateAboutYourselfIsValid(model.FirstApplicant);

            var formFields = new FormFieldBuilder()
                .AddField("previousname", model.FirstApplicant.EverBeenKnownByAnotherName.GetValueOrDefault() ? model.FirstApplicant.AnotherName : "")
                .AddField("hasanothername",
                    model.FirstApplicant.EverBeenKnownByAnotherName == null
                        ? ""
                        : model.FirstApplicant.EverBeenKnownByAnotherName.ToString().ToLower())
                .AddField("ethnicity", model.FirstApplicant.Ethnicity)
                .AddField("gender", model.FirstApplicant.Gender)
                .AddField("nationality", model.FirstApplicant.Nationality)
                .AddField("placeofbirth", model.FirstApplicant.PlaceOfBirth)
                .AddField("religionorfaithgroup", model.FirstApplicant.Religion)
                .AddField("sexualorientation", model.FirstApplicant.SexualOrientation);

            if (model.SecondApplicant != null)
            {
                completed = completed && UpdateAboutYourselfIsValid(model.SecondApplicant);

                formFields
                    .AddField("previousname_2", model.SecondApplicant.EverBeenKnownByAnotherName.GetValueOrDefault() ? model.SecondApplicant.AnotherName : "")
                    .AddField("hasanothername2",
                        model.SecondApplicant.EverBeenKnownByAnotherName == null
                            ? ""
                            : model.SecondApplicant.EverBeenKnownByAnotherName.ToString().ToLower())
                    .AddField("ethnicity2", model.SecondApplicant.Ethnicity)
                    .AddField("gender2", model.SecondApplicant.Gender)
                    .AddField("placeofbirth_2", model.SecondApplicant.PlaceOfBirth)
                    .AddField("nationality2", model.SecondApplicant.Nationality)
                    .AddField("religionorfaithgroup2", model.SecondApplicant.Religion)
                    .AddField("sexualorientation2", model.SecondApplicant.SexualOrientation);
            }

            formFields.AddField(GetFormStatusFieldName(EFosteringCaseForm.TellUsAboutYourself),
                GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted));

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                IntegrationFormName = _integrationFormName,
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields.Build()
            };

            var response = await _verintServiceGateway
                .UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update about-yourself failure");
            }

            return completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted;
        }

        public async Task<ETaskStatus> UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model)
        {

            var formFields = new FormFieldBuilder();
            var completed = UpdateAboutEmploymentIsCompleted(model.FirstApplicant);

            if (model.FirstApplicant.AreYouEmployed.Value)
            {
                formFields
                    .AddField("employed", model.FirstApplicant.AreYouEmployed.Value ? "Yes" : "No")
                    .AddField("jobtitle", model.FirstApplicant.JobTitle)
                    .AddField("currentemployer", model.FirstApplicant.CurrentEmployer)
                    .AddField("hoursofwork",
                        Enum.GetName(typeof(EHoursOfWork), model.FirstApplicant.CurrentHoursOfWork));
            }
            else
            {
                formFields
               .AddField("employed", "No")
                    .AddField("jobtitle",string.Empty)
                    .AddField("currentemployer", string.Empty)
                    .AddField("hoursofwork", 
                        Enum.GetName(typeof(EHoursOfWork), 0));
            }

            if (model.SecondApplicant != null)
            {

                completed = completed && UpdateAboutEmploymentIsCompleted(model.SecondApplicant);
                if (model.SecondApplicant.AreYouEmployed.Value)
                {
                    formFields
                        .AddField("employed2", model.SecondApplicant.AreYouEmployed.Value ? "Yes" : "No")
                        .AddField("jobtitle2", model.SecondApplicant.JobTitle)
                        .AddField("currentemployer2", model.SecondApplicant.CurrentEmployer)
                        .AddField("hoursofwork2",
                            Enum.GetName(typeof(EHoursOfWork), model.SecondApplicant.CurrentHoursOfWork));
                }
                else
                {
                    formFields
                        .AddField("employed2", model.SecondApplicant.AreYouEmployed.Value ? "Yes" : "No")
                        .AddField("jobtitle2", string.Empty)
                        .AddField("currentemployer2", string.Empty)
                        .AddField("hoursofwork2",
                            Enum.GetName(typeof(EHoursOfWork), 0));
                }
            }

            formFields.AddField(GetFormStatusFieldName(EFosteringCaseForm.YourEmploymentDetails),
                GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted));

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                IntegrationFormName = _integrationFormName,
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields.Build()
            };

            var response = await _verintServiceGateway
                .UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update about-your employment details failure");
            }

            return completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted;

        }

        public async Task<ETaskStatus> UpdateLanguagesSpokenInYourHome(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model)
        {
            var formFields = new FormFieldBuilder();
            var completed = UpdateLanguagesSpokenInYourHomeIsValid(model);

            formFields
                .AddField("primarylanguage", model.PrimaryLanguage)
                .AddField("otherlanguages", model.OtherLanguages);

            formFields.AddField(GetFormStatusFieldName(EFosteringCaseForm.LanguageSpokenInYourHome),
                GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted));

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                IntegrationFormName = _integrationFormName,
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields.Build()
            };

            var response = await _verintServiceGateway
                .UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update language-spoken-in-your-home failure");
            }

            return completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted;
        }

        public async Task<ETaskStatus> UpdatePartnershipStatus(FosteringCasePartnershipStatusUpdateModel model)
        {
            var marriedOrInACivilPartnership = string.Empty;

            if (model.MarriedOrInACivilPartnership != null)
            {
                marriedOrInACivilPartnership = model.MarriedOrInACivilPartnership.GetValueOrDefault() ? "Yes" : "No";
            }

            var completed = model.MarriedOrInACivilPartnership != null &&
                            (!model.MarriedOrInACivilPartnership.GetValueOrDefault() || model.DateOfMarriage != null) &&
                            (model.MarriedOrInACivilPartnership.GetValueOrDefault() || model.DateMovedInTogether != null);

            var formFields = new FormFieldBuilder()
                .AddField("datesetuphousehold",
                    model.DateMovedInTogether == null
                        ? string.Empty
                        : model.DateMovedInTogether.GetValueOrDefault().ToString("dd/MM/yyyy"))
                .AddField("dateofreg",
                    model.DateOfMarriage == null
                        ? string.Empty
                        : model.DateOfMarriage.GetValueOrDefault().ToString("dd/MM/yyyy"))
                .AddField("marriedorinacivilpartnership", marriedOrInACivilPartnership)
                .AddField(GetFormStatusFieldName(EFosteringCaseForm.YourPartnership), GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted))
                .Build();


            await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields,
                IntegrationFormName = _integrationFormName
            });

            return completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted;
        }

        public async Task<ETaskStatus> UpdateYourFosteringHistory(FosteringCaseYourFosteringHistoryUpdateModel model)
        {
            var formFields = new FormFieldBuilder();
            var previouslyApplied = string.Empty;
            var isCompleted = false;

            if (model.FirstApplicant.PreviouslyApplied != null)
            {
                previouslyApplied = model.FirstApplicant.PreviouslyApplied.GetValueOrDefault() ? "Yes" : "No";
                isCompleted = true;
            }

            formFields.AddField("previouslyappliedapplicant1", previouslyApplied);

            if (model.SecondApplicant != null)
            {
                if (model.SecondApplicant.PreviouslyApplied != null)
                {
                    previouslyApplied = model.SecondApplicant.PreviouslyApplied.GetValueOrDefault() ? "Yes" : "No";
                }
                else
                {
                    isCompleted = false;
                }

                formFields.AddField("previouslyappliedapplicant2", previouslyApplied);
            }

            var currentStatus = isCompleted
                                ? ETaskStatus.Completed
                                : ETaskStatus.NotCompleted;

            var builtfields = formFields.AddField("yourfosteringhistorystatus", Enum.GetName(typeof(ETaskStatus), currentStatus)).Build();

            await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = builtfields,
                IntegrationFormName = _integrationFormName
            });

            return currentStatus;
        }

        public async Task<ETaskStatus> UpdateHealthStatus(FosteringCaseHealthUpdateModel model)
        {
            var completed = UpdateHealthStatusIsCompleted(model);
            var formFields = new FormFieldBuilder();

            if (model.FirstApplicant.RegisteredDisabled != null)
            {
                formFields
                    .AddField("registereddisabled", model.FirstApplicant.RegisteredDisabled.GetValueOrDefault() ? "Yes" : "No");
            }
            if (model.FirstApplicant.Practitioner != null)
            {
                formFields
                    .AddField("practitioner", model.FirstApplicant.Practitioner.GetValueOrDefault() ? "Yes" : "No");
            }

            if (model.SecondApplicant != null)
            {
                if (model.SecondApplicant.RegisteredDisabled != null)
                {
                    formFields
                        .AddField("registereddisabled2", model.SecondApplicant.RegisteredDisabled.GetValueOrDefault() ? "Yes" : "No");
                }
                if (model.SecondApplicant.Practitioner != null)
                {
                    formFields
                        .AddField("practitioner2", model.SecondApplicant.Practitioner.GetValueOrDefault() ? "Yes" : "No");
                }
            }

            formFields.AddField(GetFormStatusFieldName(EFosteringCaseForm.YourHealth), GetTaskStatus(completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted));

            var response = await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields.Build(),
                IntegrationFormName = _integrationFormName
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update health-status failure");
            }

            return completed ? ETaskStatus.Completed : ETaskStatus.NotCompleted;
        }

        public async Task<ETaskStatus> UpdateInterestInFostering(FosteringCaseInterestInFosteringUpdateModel model)
        {
            var formFields = new FormFieldBuilder()
                .AddField("fiichildrenwithdisability", model.TypesOfFostering.Exists(_ => _.Equals("childrenWithDisability")) ? "ChildrenWithDisability" : string.Empty)
                .AddField("fiirespite", model.TypesOfFostering.Exists(_ => _.Equals("respite")) ? "Respite" : string.Empty)
                .AddField("fiishortterm", model.TypesOfFostering.Exists(_ => _.Equals("shortTerm")) ? "ShortTerm" : string.Empty)
                .AddField("fiilongterm", model.TypesOfFostering.Exists(_ => _.Equals("longTerm")) ? "LongTerm" : string.Empty)
                .AddField("fiiunsure", model.TypesOfFostering.Exists(_ => _.Equals("unsure")) ? "Unsure" : string.Empty)
                .AddField("fiishortbreaks", model.TypesOfFostering.Exists(_ => _.Equals("shortBreaks")) ? "ShortBreaks" : string.Empty)
                .AddField("reasonsforfosteringapplicant1", model.ReasonsForFostering ?? string.Empty)
                .Build();

            await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormName = _integrationFormName,
                IntegrationFormFields = formFields
            });

            var completed = !string.IsNullOrEmpty(model.ReasonsForFostering) && model.TypesOfFostering.Any();

            return completed 
                ? ETaskStatus.Completed 
                : ETaskStatus.NotCompleted;
        }

        private bool UpdateAboutYourselfIsValid(FosteringCaseAboutYourselfApplicantUpdateModel model)
        {
            return !string.IsNullOrEmpty(model.Ethnicity) &&
                !string.IsNullOrEmpty(model.Gender) &&
                !string.IsNullOrEmpty(model.Nationality) &&
                !string.IsNullOrEmpty(model.Religion) &&
                !string.IsNullOrEmpty(model.SexualOrientation) &&
                (!model.EverBeenKnownByAnotherName.GetValueOrDefault() || !string.IsNullOrEmpty(model.AnotherName));
        }

        private bool UpdateAboutEmploymentIsCompleted(FosteringCaseYourEmploymentDetailsApplicantUpdateModel model)
        {
            if (model.AreYouEmployed.Value == false)
            {
                return true;
            }

            if (model.AreYouEmployed.Value &&
              !string.IsNullOrEmpty(model.JobTitle) &&
              !string.IsNullOrEmpty(model.CurrentEmployer) &&
              Enum.IsDefined(typeof(EHoursOfWork), model.CurrentHoursOfWork))
            {
                return true;
            }

            return false;
        }

        private bool UpdateHealthStatusIsCompleted(FosteringCaseHealthUpdateModel model)
        {
            if (model.SecondApplicant != null ?
                (model.FirstApplicant.RegisteredDisabled != null && model.FirstApplicant.Practitioner != null &&
                model.SecondApplicant.RegisteredDisabled != null && model.SecondApplicant.Practitioner != null) :
                model.FirstApplicant.RegisteredDisabled != null && model.FirstApplicant.Practitioner != null)
            {
                return true;
            }

            return false;
        }

        private bool UpdateLanguagesSpokenInYourHomeIsValid(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model)
        {
            return !string.IsNullOrEmpty(model.PrimaryLanguage) && !string.IsNullOrEmpty(model.OtherLanguages);
        }

        public async Task UpdateStatus(string caseId, ETaskStatus status, EFosteringCaseForm form)
        {
            var formStatusFieldName = GetFormStatusFieldName(form);

            var formFields = new FormFieldBuilder()
                .AddField(formStatusFieldName, GetTaskStatus(status));

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                IntegrationFormName = _integrationFormName,
                CaseReference = caseId,
                IntegrationFormFields = formFields.Build()
            };

            var response = await _verintServiceGateway
                .UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Passive update-status failure");
            }
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