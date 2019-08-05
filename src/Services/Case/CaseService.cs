using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using fostering_service.Mappers;
using fostering_service.Controllers.Case.Models;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Verint;
using Model = StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Services.Case
{
    public class CaseService : ICaseService
    {
        private readonly IVerintServiceGateway _verintServiceGateway;
        private readonly ILogger<CaseService> _logger;

        public CaseService(IVerintServiceGateway verintServiceGateway, ILogger<CaseService> logger)
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
    }
}
