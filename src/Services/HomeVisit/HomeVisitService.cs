using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Builder;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.HomeVisit;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;
using fostering_service.Controllers.Case.Models;
using fostering_service.Extensions;
using Model = StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Services.HomeVisit
{
    public class HomeVisitService : IHomeVisitService
    {
        private readonly IVerintServiceGateway _verintServiceGateway;
        private readonly ILogger<HomeVisitService> _logger;
        private readonly string _integrationFormName = "Fostering_Home_Visit";

        public HomeVisitService(IVerintServiceGateway verintServiceGateway, ILogger<HomeVisitService> logger)
        {
            _verintServiceGateway = verintServiceGateway;
            _logger = logger;
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
                .AddField("religionorfaithgroup", model.FirstApplicant.Religion);

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
                    .AddField("religionorfaithgroup2", model.SecondApplicant.Religion);
            }

            formFields.AddField(EFosteringHomeVisitForm.TellUsAboutYourself.GetFormStatusFieldName(),
                completed ? ETaskStatus.Completed.GetTaskStatus() : ETaskStatus.NotCompleted.GetTaskStatus());

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
                    .AddField("jobtitle", string.Empty)
                    .AddField("currentemployer", string.Empty)
                    .AddField("hoursofwork",
                        Enum.GetName(typeof(EHoursOfWork), 0));
            }

            if (model.SecondApplicant != null)
            {

                if (model.SecondApplicant.AreYouEmployed != null && model.SecondApplicant.AreYouEmployed.Value == true)
                {
                    formFields
                        .AddField("employed2", "Yes")
                        .AddField("jobtitle2", model.SecondApplicant.JobTitle)
                        .AddField("currentemployer2", model.SecondApplicant.CurrentEmployer)
                        .AddField("hoursofwork2",
                            Enum.GetName(typeof(EHoursOfWork), model.SecondApplicant.CurrentHoursOfWork));
                }
                else
                {
                    formFields
                        .AddField("employed2", "No")
                        .AddField("jobtitle2", string.Empty)
                        .AddField("currentemployer2", string.Empty)
                        .AddField("hoursofwork2",
                            Enum.GetName(typeof(EHoursOfWork), 0));
                }

                completed = completed && UpdateAboutEmploymentIsCompleted(model.SecondApplicant);
            }

            formFields.AddField(EFosteringHomeVisitForm.YourEmploymentDetails.GetFormStatusFieldName(),
                completed ? ETaskStatus.Completed.GetTaskStatus() : ETaskStatus.NotCompleted.GetTaskStatus());

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

            formFields.AddField(EFosteringHomeVisitForm.LanguageSpokenInYourHome.GetFormStatusFieldName(),
                completed ? ETaskStatus.Completed.GetTaskStatus() : ETaskStatus.NotCompleted.GetTaskStatus());

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
                .AddField(EFosteringHomeVisitForm.YourPartnership.GetFormStatusFieldName(), completed ? ETaskStatus.Completed.GetTaskStatus() : ETaskStatus.NotCompleted.GetTaskStatus())
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

            formFields.AddField(EFosteringHomeVisitForm.YourHealth.GetFormStatusFieldName(), completed ? ETaskStatus.Completed.GetTaskStatus() : ETaskStatus.NotCompleted.GetTaskStatus());

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
            var completed = !string.IsNullOrEmpty(model.ReasonsForFostering) && model.TypesOfFostering.Any();
            var currentStatus = completed
                ? ETaskStatus.Completed
                : ETaskStatus.NotCompleted;

            var formFields = new FormFieldBuilder()
                .AddField("fiichildrenwithdisability", model.TypesOfFostering.Exists(_ => _.Equals("childrenWithDisability")) ? "ChildrenWithDisability" : string.Empty)
                .AddField("fiirespite", model.TypesOfFostering.Exists(_ => _.Equals("respite")) ? "Respite" : string.Empty)
                .AddField("fiishortterm", model.TypesOfFostering.Exists(_ => _.Equals("shortTerm")) ? "ShortTerm" : string.Empty)
                .AddField("fiilongterm", model.TypesOfFostering.Exists(_ => _.Equals("longTerm")) ? "LongTerm" : string.Empty)
                .AddField("fiiunsure", model.TypesOfFostering.Exists(_ => _.Equals("unsure")) ? "Unsure" : string.Empty)
                .AddField("fiishortbreaks", model.TypesOfFostering.Exists(_ => _.Equals("shortBreaks")) ? "ShortBreaks" : string.Empty)
                .AddField("reasonsforfosteringapplicant1", model.ReasonsForFostering ?? string.Empty)
                .AddField("tellusaboutyourinterestinfosteringstatus", Enum.GetName(typeof(ETaskStatus), currentStatus))
                .Build();

            await _verintServiceGateway.UpdateCaseIntegrationFormField(new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormName = _integrationFormName,
                IntegrationFormFields = formFields
            });


            return currentStatus;
        }

        public List<OtherPerson> CreateOtherPersonList(OtherPeopleConfigurationModel config, List<CustomField> formFields, int capacity = 8)
        {
            var otherPersonList = new List<OtherPerson>();

            for (var i = 0; i < capacity; i++)
                otherPersonList.Add(new OtherPerson
                {
                    Address = new Model.Address()
                });

            formFields.ForEach(field =>
            {
                if (string.IsNullOrEmpty(field.Name))
                    return;

                int.TryParse(field.Name[field.Name.Length - 1].ToString(), out var index);

                index--;

                if (index < 0)
                    return;

                if (field.Name.Contains(config.DateOfBirth))
                    otherPersonList[index].DateOfBirth = DateTime.Parse(field.Value);

                if (field.Name.Contains(config.FirstName))
                    otherPersonList[index].FirstName = field.Value;

                if (field.Name.Contains(config.LastName))
                    otherPersonList[index].LastName = field.Value;

                if (field.Name.Contains(config.Gender))
                    otherPersonList[index].Gender = field.Value;

                if (!string.IsNullOrEmpty(config.RelationshipToYou) && field.Name.Contains(config.RelationshipToYou))
                    otherPersonList[index].RelationshipToYou = field.Value;

                if (!string.IsNullOrEmpty(config.Address) && field.Name.Contains(config.Address))
                {
                    var address = field.Value.Split("|");

                    switch (address.Length)
                    {
                        case 0:
                            otherPersonList[index].Address.AddressLine1 = string.Empty;
                            otherPersonList[index].Address.AddressLine2 = string.Empty;
                            otherPersonList[index].Address.Town = string.Empty;
                            break;
                        case 1:
                            otherPersonList[index].Address.AddressLine1 = address[0];
                            break;
                        case 2:
                            otherPersonList[index].Address.AddressLine1 = address[0];
                            otherPersonList[index].Address.Town = address[1];
                            break;
                        default:
                            otherPersonList[index].Address.AddressLine1 = address[0];
                            otherPersonList[index].Address.AddressLine2 = address[1];
                            otherPersonList[index].Address.Town = address[2];
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(config.Postcode) && field.Name.Contains(config.Postcode))
                    otherPersonList[index].Address.Postcode = field.Value;
            });

            return otherPersonList.Where(person =>
                person.Gender != null ||
                person.LastName != null ||
                person.FirstName != null ||
                person.DateOfBirth != null ||
                person.RelationshipToYou != null ||
                person.Address?.AddressLine1 != null ||
                person.Address?.AddressLine2 != null ||
                person.Address?.Town != null ||
                person.Address?.Postcode != null).
                ToList();
        }

        public FormFieldBuilder CreateOtherPersonBuilder(OtherPeopleConfigurationModel config, List<OtherPerson> otherPeople, int capacity = 8)
        {
            var builder = new FormFieldBuilder();

            for (var i = 0; i < otherPeople?.Count; i++)
            {
                var nameSuffix = i + 1;

                builder
                    .AddField($"{config.FirstName}{nameSuffix}", otherPeople[i].FirstName ?? string.Empty)
                    .AddField($"{config.DateOfBirth}{nameSuffix}", otherPeople[i].DateOfBirth?.ToString("dd/MM/yyyy") ?? string.Empty)
                    .AddField($"{config.Gender}{nameSuffix}", otherPeople[i].Gender ?? string.Empty)
                    .AddField($"{config.LastName}{nameSuffix}", otherPeople[i].LastName ?? string.Empty);

                if (!string.IsNullOrEmpty(config.RelationshipToYou))
                    builder.AddField($"{config.RelationshipToYou}{nameSuffix}", otherPeople[i].RelationshipToYou ?? string.Empty);

                if (!string.IsNullOrEmpty(config.Address) && !string.IsNullOrEmpty(config.Postcode))
                {
                    if (otherPeople[i].Address == null)
                    {
                        otherPeople[i].Address = new Model.Address();
                    }
                    builder
                        .AddField($"{config.Address}{nameSuffix}", otherPeople[i].Address.AddressLine1 + "|" + otherPeople[i].Address.AddressLine2 + "|" + otherPeople[i].Address.Town)
                        .AddField($"{config.Postcode}{nameSuffix}", otherPeople[i].Address.Postcode ?? string.Empty);
                }
            }

            for (var i = otherPeople?.Count; i < capacity; i++)
            {
                var nameSuffix = i + 1;

                builder
                    .AddField($"{config.FirstName}{nameSuffix}", string.Empty)
                    .AddField($"{config.DateOfBirth}{nameSuffix}", string.Empty)
                    .AddField($"{config.Gender}{nameSuffix}", string.Empty)
                    .AddField($"{config.LastName}{nameSuffix}", string.Empty);

                if (!string.IsNullOrEmpty(config.RelationshipToYou))
                    builder.AddField($"{config.RelationshipToYou}{nameSuffix}", string.Empty);

                if (!string.IsNullOrEmpty(config.Address) && !string.IsNullOrEmpty(config.Postcode))
                {
                    builder
                    .AddField($"{config.Address}{nameSuffix}", string.Empty)
                        .AddField($"{config.Postcode}{nameSuffix}", string.Empty);
                }
            }

            return builder;
        }

        public async Task<ETaskStatus> UpdateHousehold(FosteringCaseHouseholdUpdateModel model)
        {
            var completed = UpdateHouseholdIsComplete(model) ? ETaskStatus.Completed : ETaskStatus.NotCompleted;

            var formFields = CreateOtherPersonBuilder(ConfigurationModels.HouseholdConfigurationModel, model.AnyOtherPeopleInYourHousehold.GetValueOrDefault() ? model.OtherPeopleInYourHousehold : new List<OtherPerson>())
                .AddField(EFosteringHomeVisitForm.YourHousehold.GetFormStatusFieldName(), completed.GetTaskStatus())
                .AddField("listofpetsandanimals", model.DoYouHaveAnyPets.GetValueOrDefault() ? model.PetsInformation : string.Empty)
                .AddField("doyouhaveanypets", model.DoYouHaveAnyPets == null ? string.Empty : model.DoYouHaveAnyPets == true ? "Yes" : "No")
                .AddField("otherpeopleinyourhousehold", model.AnyOtherPeopleInYourHousehold == null ? string.Empty : model.AnyOtherPeopleInYourHousehold == true ? "Yes" : "No");

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = formFields.Build(),
                IntegrationFormName = _integrationFormName
            };

            var response = await _verintServiceGateway.UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update household failure");
            }

            return completed;
        }

        private bool UpdateHouseholdIsComplete(FosteringCaseHouseholdUpdateModel model)
        {
            bool pets = false, people = false;

            if (model.DoYouHaveAnyPets == false ||
                model.DoYouHaveAnyPets == true && !string.IsNullOrEmpty(model.PetsInformation))
            {
                pets = true;
            }

            if (model.AnyOtherPeopleInYourHousehold == false || model.AnyOtherPeopleInYourHousehold == true
                && model.OtherPeopleInYourHousehold != null
                && model.OtherPeopleInYourHousehold?.Count != 0
                && !model.OtherPeopleInYourHousehold.Exists(
                    person => string.IsNullOrEmpty(person.Gender) ||
                              string.IsNullOrEmpty(person.LastName) ||
                              string.IsNullOrEmpty(person.FirstName) ||
                              string.IsNullOrEmpty(person.RelationshipToYou) ||
                              person.DateOfBirth == null))
            {
                people = true;
            }

            return pets && people;
        }

        private bool UpdateAboutYourselfIsValid(FosteringCaseAboutYourselfApplicantUpdateModel model)
        {
            return !string.IsNullOrEmpty(model.Ethnicity) &&
                !string.IsNullOrEmpty(model.Gender) &&
                !string.IsNullOrEmpty(model.Nationality) &&
                !string.IsNullOrEmpty(model.Religion) &&
                (!model.EverBeenKnownByAnotherName.GetValueOrDefault() || !string.IsNullOrEmpty(model.AnotherName));
        }

        private bool UpdateAboutEmploymentIsCompleted(FosteringCaseYourEmploymentDetailsApplicantUpdateModel model)
        {
            if (model.AreYouEmployed != null && model.AreYouEmployed.Value == false)
            {
                return true;
            }

            if (model.AreYouEmployed != null && model.AreYouEmployed.Value &&
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

        public async Task<ETaskStatus> UpdateChildrenLivingAwayFromHome(FosteringCaseChildrenLivingAwayFromHomeUpdateModel model)
        {
            var completed = UpdateChildrenLivingAwayFromHomeIsComplete(model) ? ETaskStatus.Completed : ETaskStatus.NotCompleted;

            var firstApplicantUnderSixteen = CreateOtherPersonBuilder(ConfigurationModels.FirstApplicantUnderSixteenConfigurationModel, model.FirstApplicant.AnyChildrenUnderSixteen.GetValueOrDefault() ? model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome : new List<OtherPerson>(), 4)
                .AddField(EFosteringHomeVisitForm.ChildrenLivingAwayFromYourHome.GetFormStatusFieldName(), completed.GetTaskStatus())
                .AddField("haschildrenundersixteen1", model.FirstApplicant.AnyChildrenUnderSixteen == null ? string.Empty : model.FirstApplicant.AnyChildrenUnderSixteen == true ? "yes" : "no");

            var firstApplicantOverSixteen = CreateOtherPersonBuilder(ConfigurationModels.FirstApplicantOverSixteenConfigurationModel, model.FirstApplicant.AnyChildrenOverSixteen.GetValueOrDefault() ? model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome : new List<OtherPerson>(), 4)
                .AddField("haschildrenoversixteen1", model.FirstApplicant.AnyChildrenOverSixteen == null ? string.Empty : model.FirstApplicant.AnyChildrenOverSixteen == true ? "yes" : "no")
                .Build();

            List<IntegrationFormField> secondApplicantUnderSixteen = new List<IntegrationFormField>();
            List<IntegrationFormField> secondApplicantOverSixteen = new List<IntegrationFormField>();

            if (model.SecondApplicant != null)
            {
                secondApplicantUnderSixteen = CreateOtherPersonBuilder(ConfigurationModels.SecondApplicantUnderSixteenConfigurationModel, model.SecondApplicant.AnyChildrenUnderSixteen.GetValueOrDefault() ? model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome : new List<OtherPerson>(), 4)
                    .AddField("haschildrenundersixteen2", model.SecondApplicant.AnyChildrenUnderSixteen == null ? string.Empty : model.SecondApplicant.AnyChildrenUnderSixteen == true ? "yes" : "no").Build();

                secondApplicantOverSixteen = CreateOtherPersonBuilder(ConfigurationModels.SecondApplicantOverSixteenConfigurationModel, model.SecondApplicant.AnyChildrenOverSixteen.GetValueOrDefault() ? model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome : new List<OtherPerson>(), 4)
                    .AddField("haschildrenoversixteen2", model.SecondApplicant.AnyChildrenOverSixteen == null ? string.Empty : model.SecondApplicant.AnyChildrenOverSixteen == true ? "yes" : "no").Build();
            }

            var integrationFormFields = model.SecondApplicant != null ? firstApplicantUnderSixteen.Build().Concat(firstApplicantOverSixteen).ToList().Concat(secondApplicantUnderSixteen).ToList().Concat(secondApplicantOverSixteen).ToList() : firstApplicantUnderSixteen.Build().Concat(firstApplicantOverSixteen).ToList();

            var updateModel = new IntegrationFormFieldsUpdateModel
            {
                CaseReference = model.CaseReference,
                IntegrationFormFields = integrationFormFields,
                IntegrationFormName = _integrationFormName
            };

            var response = await _verintServiceGateway.UpdateCaseIntegrationFormField(updateModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Update children living away from home failure");
            }

            return completed;
        }

        private bool UpdateChildrenLivingAwayFromHomeIsComplete(FosteringCaseChildrenLivingAwayFromHomeUpdateModel model)
        {
            bool firstApplicantUnderSixteen = false, firstApplicantOverSixteen = false;

            if (model.FirstApplicant.AnyChildrenUnderSixteen == false || model.FirstApplicant.AnyChildrenUnderSixteen == true
                && model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome != null
                && model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome?.Count != 0
                && !model.FirstApplicant.ChildrenUnderSixteenLivingAwayFromHome.Exists(person =>
                    string.IsNullOrEmpty(person.FirstName) ||
                    string.IsNullOrEmpty(person.LastName) ||
                    string.IsNullOrEmpty(person.Gender) ||
                    person.DateOfBirth == null ||
                    string.IsNullOrEmpty(person.Address.AddressLine1) ||
                    string.IsNullOrEmpty(person.Address.Town) ||
                    string.IsNullOrEmpty(person.Address.Postcode)))
            {
                firstApplicantUnderSixteen = true;
            }

            if (model.FirstApplicant.AnyChildrenOverSixteen == false || model.FirstApplicant.AnyChildrenOverSixteen == true
                && model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome != null
                && model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome?.Count != 0
                && !model.FirstApplicant.ChildrenOverSixteenLivingAwayFromHome.Exists(person =>
                    string.IsNullOrEmpty(person.FirstName) ||
                    string.IsNullOrEmpty(person.LastName) ||
                    string.IsNullOrEmpty(person.Gender) ||
                    person.DateOfBirth == null ||
                    string.IsNullOrEmpty(person.Address.AddressLine1) ||
                    string.IsNullOrEmpty(person.Address.Town) ||
                    string.IsNullOrEmpty(person.Address.Postcode)))
            {
                firstApplicantOverSixteen = true;
            }

            if (model.SecondApplicant != null)
            {
                bool secondApplicantUnderSixteen = false, secondApplicantOverSixteen = false;

                if (model.SecondApplicant.AnyChildrenUnderSixteen == false ||
                    model.SecondApplicant.AnyChildrenUnderSixteen == true
                    && model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome != null
                    && model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome?.Count != 0
                    && !model.SecondApplicant.ChildrenUnderSixteenLivingAwayFromHome.Exists(person =>
                        string.IsNullOrEmpty(person.FirstName) ||
                        string.IsNullOrEmpty(person.LastName) ||
                        string.IsNullOrEmpty(person.Gender) ||
                        person.DateOfBirth == null ||
                        string.IsNullOrEmpty(person.Address.AddressLine1) ||
                        string.IsNullOrEmpty(person.Address.Town) ||
                        string.IsNullOrEmpty(person.Address.Postcode)))
                {
                    secondApplicantUnderSixteen = true;
                }

                if (model.SecondApplicant.AnyChildrenOverSixteen == false ||
                    model.SecondApplicant.AnyChildrenOverSixteen == true
                    && model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome != null
                    && model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome?.Count != 0
                    && !model.SecondApplicant.ChildrenOverSixteenLivingAwayFromHome.Exists(person =>
                        string.IsNullOrEmpty(person.FirstName) ||
                        string.IsNullOrEmpty(person.LastName) ||
                        string.IsNullOrEmpty(person.Gender) ||
                        person.DateOfBirth == null ||
                        string.IsNullOrEmpty(person.Address.AddressLine1) ||
                        string.IsNullOrEmpty(person.Address.Town) ||
                        string.IsNullOrEmpty(person.Address.Postcode)))
                {
                    secondApplicantOverSixteen = true;
                }

                return firstApplicantUnderSixteen && firstApplicantOverSixteen && secondApplicantUnderSixteen && secondApplicantOverSixteen;
            }

            return firstApplicantUnderSixteen && firstApplicantOverSixteen;
        }

        public async Task UpdateStatus(string caseId, ETaskStatus status, EFosteringHomeVisitForm form)
        {
            var formStatusFieldName = form.GetFormStatusFieldName();

            var formFields = new FormFieldBuilder()
                .AddField(formStatusFieldName, status.GetTaskStatus());

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

       
    }
}
