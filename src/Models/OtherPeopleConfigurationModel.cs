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
    }

    public class OtherPeopleConfigurationModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string DateOfBirth { get; set; }
    }
}