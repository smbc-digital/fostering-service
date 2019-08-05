namespace fostering_service.Controllers.Case.Models
{
    public static class ConfigurationModels
    {
        public static OtherPeopleConfigurationModel HouseholdConfigurationModel = new OtherPeopleConfigurationModel
        {
            DateOfBirth = "opdateofbirth",
            FirstName = "opfirstname",
            Gender = "opgender",
            LastName = "oplastname",
            RelationshipToYou = "oprelationshiptoapplicant"
        };

        public static OtherPeopleConfigurationModel FirstApplicantUnderSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "under16firstname1",
            LastName = "under16lastname1",
            Gender = "under16gender1",
            DateOfBirth = "under16dateofbirth1",
            Address = "under16address1",
            Postcode = "under16postcode1"
        };

        public static OtherPeopleConfigurationModel SecondApplicantUnderSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "under16firstname2",
            LastName = "under16lastname2",
            Gender = "under16gender2",
            DateOfBirth = "under16dateofbirth2",
            Address = "under16address2",
            Postcode = "under16postcode2"
        };

        public static OtherPeopleConfigurationModel FirstApplicantOverSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "over16firstname1",
            LastName = "over16lastname1",
            Gender = "over16gender1",
            DateOfBirth = "over16dateofbirth1",
            Address = "over16address1",
            Postcode = "over16postcode1"
        };

        public static OtherPeopleConfigurationModel SecondApplicantOverSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "over16firstname2",
            LastName = "over16lastname2",
            Gender = "over16gender2",
            DateOfBirth = "over16dateofbirth2",
            Address = "over16address2",
            Postcode = "over16postcode2"
        };
    }

    public class OtherPeopleConfigurationModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string RelationshipToYou { get; set; }

        public string Address { get; set; }

        public string Postcode { get; set; }
    }
}