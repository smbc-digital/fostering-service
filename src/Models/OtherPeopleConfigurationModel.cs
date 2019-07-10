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
            AddressLine1 = "Under16Address1",
            AddressLine2 = "",
            Town = "",
            Postcode = ""
        };

        public static OtherPeopleConfigurationModel SecondApplicantUnderSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "",
            LastName = "",
            Gender = "",
            DateOfBirth = "",
            AddressLine1 = "",
            AddressLine2 = "",
            Town = "",
            Postcode = ""
        };

        public static OtherPeopleConfigurationModel FirstApplicantOverSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "",
            LastName = "",
            Gender = "",
            DateOfBirth = "",
            AddressLine1 = "",
            AddressLine2 = "",
            Town = "",
            Postcode = ""
        };

        public static OtherPeopleConfigurationModel SecondApplicantOverSixteenConfigurationModel = new OtherPeopleConfigurationModel
        {
            FirstName = "",
            LastName = "",
            Gender = "",
            DateOfBirth = "",
            AddressLine1 = "",
            AddressLine2 = "",
            Town = "",
            Postcode = ""
        };
    }

    public class OtherPeopleConfigurationModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string Town { get; set; }

        public string Postcode { get; set; }
    }
}