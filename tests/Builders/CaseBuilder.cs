using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Verint;

namespace fostering_service_tests.Builders
{
    public class CaseBuilder
    {
        private List<CustomField> _integrationFormFields = new List<CustomField>();
        private string _definitionName = "Test_Definition";
        private string _enquirySubject = "Test_Subject";
        private string _enquiryReason = "Test_Reason";
        private string _enquiryType = "Test_Tyoe";

        public Case Build()
        {
            return new Case
            {
                IntegrationFormFields = _integrationFormFields,
                DefinitionName = _definitionName,
                EnquirySubject = _enquirySubject,
                EnquiryReason = _enquiryReason,
                EnquiryType = _enquiryType
            };
        }

        public CaseBuilder WithIntegrationFormField(string name, string value)
        {
            _integrationFormFields.Add(new CustomField(name, value));
            return this;
        }

        public CaseBuilder WithDefinitionName(string name)
        {
            _definitionName = name;
            return this;
        }

        public CaseBuilder WithEnquirySubject(string name)
        {
            _enquirySubject = name;
            return this;
        }

        public CaseBuilder WithEnquiryReason(string name)
        {
            _enquiryReason = name;
            return this;
        }

        public CaseBuilder WithEnquiryType(string name)
        {
            _enquiryType = name;
            return this;
        }
    }
}
