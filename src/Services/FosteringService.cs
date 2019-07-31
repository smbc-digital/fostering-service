using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Builder;
using fostering_service.Mappers;
using fostering_service.Models;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Verint.Update;
using Model = StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Services
{
    public class FosteringService : IFosteringService
    {
        private readonly IVerintServiceGateway _verintServiceGateway;
        private readonly ILogger<FosteringService> _logger;
        private readonly string _integrationFormName = "Fostering_Home_Visit";
        private readonly string _applicationFormName = "Fostering_Application";

        public FosteringService(IVerintServiceGateway verintServiceGateway, ILogger<FosteringService> logger)
        {
            _verintServiceGateway = verintServiceGateway;
            _logger = logger;
        }

        public async Task<FosteringCase> GetCase(string caseId)
        {
            var response = await _verintServiceGateway.GetCase(caseId);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"FosteringService GetCase an exception has occured while getting case from verint service, statuscode: {response.StatusCode}");
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
                    YourPartnershipStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "yourpartnershipstatus")?.Value),
                    References = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "references")?.Value),
                    GpDetailsStatus = GetTaskStatus(integrationFormFields.FirstOrDefault(_ => _.Name == "gpdetailsstatus")?.Value)
                },
                FirstApplicant = new FosteringApplicant
                {
                    FirstName = integrationFormFields.First(_ => _.Name == "firstname").Value,
                    LastName = integrationFormFields.First(_ => _.Name == "surname").Value,
                    AnotherName = integrationFormFields.FirstOrDefault(_ => _.Name == "previousname")?.Value ?? string.Empty,
                    Nationality = integrationFormFields.FirstOrDefault(_ => _.Name == "nationality")?.Value ?? string.Empty,
                    Ethnicity = integrationFormFields.FirstOrDefault(_ => _.Name == "ethnicity")?.Value ?? string.Empty,
                    Gender = integrationFormFields.FirstOrDefault(_ => _.Name == "gender")?.Value ?? string.Empty,
                    Religion = integrationFormFields.FirstOrDefault(_ => _.Name == "religionorfaithgroup")?.Value ?? string.Empty,
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth")?.Value ?? string.Empty,
                    CurrentEmployer = integrationFormFields.FirstOrDefault(_ => _.Name == "currentemployer")?.Value ?? string.Empty,
                    JobTitle = integrationFormFields.FirstOrDefault(_ => _.Name == "jobtitle")?.Value ?? string.Empty,
                    ChildrenUnderSixteenLivingAwayFromHome = CreateOtherPersonList(ConfigurationModels.FirstApplicantUnderSixteenConfigurationModel, integrationFormFields, 4),
                    ChildrenOverSixteenLivingAwayFromHome = CreateOtherPersonList(ConfigurationModels.FirstApplicantOverSixteenConfigurationModel, integrationFormFields, 4),
                    NameOfGp = integrationFormFields.FirstOrDefault(_ => _.Name == "nameofgp")?.Value,
                    NameOfGpPractice = integrationFormFields.FirstOrDefault(_ => _.Name == "nameofpractice")?.Value,
                    GpPhoneNumber = integrationFormFields.FirstOrDefault(_ => _.Name == "gpphonenumber")?.Value,
                    GpAddress = AddressMapper.MapToFosteringAddress(integrationFormFields, "addressofpractice", "placerefofpractice", "postcodeofpractice").Validate()
                },
                WithPartner = integrationFormFields.FirstOrDefault(_ => _.Name == "withpartner")?.Value ?? "yes",
                PrimaryLanguage = integrationFormFields.FirstOrDefault(_ => _.Name == "primarylanguage")?.Value ?? string.Empty,
                OtherLanguages = integrationFormFields.FirstOrDefault(_ => _.Name == "otherlanguages")?.Value ?? string.Empty,
                TypesOfFostering = new List<string>(),
                ReasonsForFostering = integrationFormFields.FirstOrDefault(_ => _.Name == "reasonsforfosteringapplicant1")?.Value ?? string.Empty,
                OtherPeopleInYourHousehold = CreateOtherPersonList(ConfigurationModels.HouseholdConfigurationModel, integrationFormFields),
                PetsInformation = integrationFormFields.FirstOrDefault(_ => _.Name == "listofpetsandanimals")?.Value ?? string.Empty,
                EnableAdditionalInformationSection = string.Equals(response.ResponseContent.EnquirySubject, "Fostering", StringComparison.CurrentCultureIgnoreCase) && string.Equals(response.ResponseContent.EnquiryReason, "Fostering Application", StringComparison.CurrentCultureIgnoreCase) && string.Equals(response.ResponseContent.EnquiryType, "3. Application", StringComparison.CurrentCultureIgnoreCase)
            };

            fosteringCase.FamilyReference = ReferenceDetailsMapper.MapToReferenceDetails(integrationFormFields,
                "prffirstname", "prflastname", "prfrelation", "prfyears", 
                "prfemail", "prfcontact", "prfaddress",
                "prfplaceref", "prfpostcode");

            fosteringCase.FirstPersonalReference = ReferenceDetailsMapper.MapToReferenceDetails(integrationFormFields,
                "prf11firstname", "prf1lastname", "prf1relation", "prf1years",
                "prf1email", "prf1contact", "prf1address",
                "prf1placeref", "prf1postcode");

            fosteringCase.SecondPersonalReference = ReferenceDetailsMapper.MapToReferenceDetails(integrationFormFields,
                "prf2firstname", "prf2lastname", "prf2relation", "prf2years",
                "prf2email", "prf2contact", "prf2address",
                "prf2placeref", "prf2postcode");

            var anyOtherPeopleInYourHousehold = integrationFormFields.FirstOrDefault(_ => _.Name == "otherpeopleinyourhousehold")?.Value;
            if (!string.IsNullOrEmpty(anyOtherPeopleInYourHousehold))
            {
                fosteringCase.AnyOtherPeopleInYourHousehold = anyOtherPeopleInYourHousehold.ToLower() == "yes";
            }

            var HomeVisitDate = integrationFormFields.FirstOrDefault(_ => _.Name == "dateofthehomevisit")?.Value;
            var HomeVisitTime = integrationFormFields.FirstOrDefault(_ => _.Name == "timeofhomevisit")?.Value;
            if (!string.IsNullOrEmpty(HomeVisitDate + HomeVisitTime))
            {
                fosteringCase.HomeVisitDateTime = DateTime.Parse($"{HomeVisitDate} {HomeVisitTime}");

            }

            var doYouHaveAnyPets = integrationFormFields.FirstOrDefault(_ => _.Name == "doyouhaveanypets")?.Value;
            if (!string.IsNullOrEmpty(doYouHaveAnyPets))
            {
                fosteringCase.DoYouHaveAnyPets = doYouHaveAnyPets.ToLower() == "yes";
            }

            var firstApplicantAnyChildrenUnderSixteen = integrationFormFields.FirstOrDefault(_ => _.Name == "haschildrenundersixteen1")?.Value;
            if (!string.IsNullOrEmpty(firstApplicantAnyChildrenUnderSixteen))
            {
                fosteringCase.FirstApplicant.AnyChildrenUnderSixteen = firstApplicantAnyChildrenUnderSixteen.ToLower() == "yes";
            }

            var firstApplicantAnyChildrenOverSixteen = integrationFormFields.FirstOrDefault(_ => _.Name == "haschildrenoversixteen1")?.Value;
            if (!string.IsNullOrEmpty(firstApplicantAnyChildrenOverSixteen))
            {
                fosteringCase.FirstApplicant.AnyChildrenOverSixteen = firstApplicantAnyChildrenOverSixteen.ToLower() == "yes";
            }

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

            if (integrationFormFields.Exists(_ => _.Name == "registereddisabled"))
            {
                fosteringCase.FirstApplicant.RegisteredDisabled = integrationFormFields.FirstOrDefault(_ => _.Name == "registereddisabled")?.Value.ToLower() == "yes";
            }

            if (integrationFormFields.Exists(_ => _.Name == "practitioner"))
            {
                fosteringCase.FirstApplicant.Practitioner = integrationFormFields.FirstOrDefault(_ => _.Name == "practitioner")?.Value.ToLower() == "yes";
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
                    Religion = integrationFormFields.FirstOrDefault(_ => _.Name == "religionorfaithgroup2")?.Value ?? string.Empty,
                    PlaceOfBirth = integrationFormFields.FirstOrDefault(_ => _.Name == "placeofbirth_2")?.Value ?? string.Empty,
                    CurrentEmployer = integrationFormFields.FirstOrDefault(_ => _.Name == "currentemployer2")?.Value ?? string.Empty,
                    JobTitle = integrationFormFields.FirstOrDefault(_ => _.Name == "jobtitle2")?.Value ?? string.Empty,
                    ChildrenUnderSixteenLivingAwayFromHome = CreateOtherPersonList(ConfigurationModels.SecondApplicantUnderSixteenConfigurationModel, integrationFormFields, 4),
                    ChildrenOverSixteenLivingAwayFromHome = CreateOtherPersonList(ConfigurationModels.SecondApplicantOverSixteenConfigurationModel, integrationFormFields, 4),
                    NameOfGp = integrationFormFields.FirstOrDefault(_ => _.Name == "nameofgp2")?.Value,
                    NameOfGpPractice = integrationFormFields.FirstOrDefault(_ => _.Name == "nameofpractice2")?.Value,
                    GpPhoneNumber = integrationFormFields.FirstOrDefault(_ => _.Name == "gpphonenumber2")?.Value,
                    GpAddress = AddressMapper.MapToFosteringAddress(integrationFormFields, "addressofpractice2", "placerefofpractice2", "postcodeofpractice2").Validate()
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

                var registereddisabled2 = integrationFormFields.FirstOrDefault(_ => _.Name == "registereddisabled2")?.Value;
                if (!string.IsNullOrEmpty(registereddisabled2))
                {
                    fosteringCase.SecondApplicant.RegisteredDisabled = registereddisabled2.ToLower() == "yes";
                }

                var practitioner2 = integrationFormFields.FirstOrDefault(_ => _.Name == "practitioner2")?.Value;
                if (!string.IsNullOrEmpty(practitioner2))
                {
                    fosteringCase.SecondApplicant.Practitioner = practitioner2.ToLower() == "yes";
                }

                var secondApplicantAnyChildrenUnderSixteen = integrationFormFields.FirstOrDefault(_ => _.Name == "haschildrenundersixteen2")?.Value;
                if (!string.IsNullOrEmpty(secondApplicantAnyChildrenUnderSixteen))
                {
                    fosteringCase.SecondApplicant.AnyChildrenUnderSixteen = secondApplicantAnyChildrenUnderSixteen.ToLower() == "yes";
                }

                var secondApplicantAnyChildrenOverSixteen = integrationFormFields.FirstOrDefault(_ => _.Name == "haschildrenoversixteen2")?.Value;
                if (!string.IsNullOrEmpty(secondApplicantAnyChildrenOverSixteen))
                {
                    fosteringCase.SecondApplicant.AnyChildrenOverSixteen = secondApplicantAnyChildrenOverSixteen.ToLower() == "yes";
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

        public async Task<ETaskStatus> UpdateGpDetails(FosteringCaseGpDetailsUpdateModel model)
        {
            var firstApplicantFormFields = new FormFieldBuilder()
                .AddField("nameofgp", model.FirstApplicant.NameOfGp)
                .AddField("nameofpractice", model.FirstApplicant.NameOfGpPractice)
                .AddField("gpphonenumber", model.FirstApplicant.GpPhoneNumber)
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
                .AddField(GetFormStatusFieldName(EFosteringCaseForm.YourHousehold), GetTaskStatus(completed))
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
                .AddField(GetFormStatusFieldName(EFosteringCaseForm.ChildrenLivingAwayFromYourHome), GetTaskStatus(completed))
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
                .AddField("prf2contact", model.SecondPersonalReference.PhoneNumber).Build();

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
                case EFosteringCaseForm.References:
                    return "yourreferencesstatus";
                case EFosteringCaseForm.GpDetails:
                    return "gpdetailsstatus";
                default:
                    return null;
            }
        }
    }
}