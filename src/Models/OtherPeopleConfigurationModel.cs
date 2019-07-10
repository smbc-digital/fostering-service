namespace fostering_service.Models
{
    public static class ConfigurationModels
    {
        public static OtherPeopleConfigurationModel HouseholdConfigurationModel = new OtherPeopleConfigurationModel
        {
            DateOfBirth = "opdateofbirth",
            FirstName = "opfirstname",
            Gender = "opgender",
            LastName = "oplastname"
        };

        public static OtherPeopleConfigurationModel FirstApplicantUnderSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "Under16FirstName1",
            LastName = "Under16LastName1",
            Gender = "Under16Gender1",
            DateOfBirth = "Under16DateOfBirth1",
            Address = "Under16Address1",
            Postcode = "Under16Postcode1"
        };

        public static OtherPeopleConfigurationModel SecondApplicantUnderSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "Under16FirstName2",
            LastName = "Under16LastName2",
            Gender = "Under16Gender2",
            DateOfBirth = "Under16DateOfBirth2",
            Address = "Under16Address2",
            Postcode = "Under16Postcode2"
        };

        public static OtherPeopleConfigurationModel FirstApplicantOverSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "Over16FirstName1",
            LastName = "Over16LastName1",
            Gender = "Over16Gender1",
            DateOfBirth = "Over16DateOfBirth1",
            Address = "Over16Address1",
            Postcode = "Over16Postcode1"
        };

        public static OtherPeopleConfigurationModel SecondApplicantOverSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "Over16FirstName2",
            LastName = "Over16LastName2",
            Gender = "Over16Gender2",
            DateOfBirth = "Over16DateOfBirth2",
            Address = "Over16Address2",
            Postcode = "Over16Postcode2"
        };
    }

    public class OtherPeopleConfigurationModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string Address { get; set; }

        public string Postcode { get; set; }
    }
}