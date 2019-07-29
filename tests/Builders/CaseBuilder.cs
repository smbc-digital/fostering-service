using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service_tests.Builders
{
    public class CaseBuilder
    {
        private List<CustomField> _integrationFormFields = new List<CustomField>();
        private string _definitionName = "Test_Definition";

        public Case Build()
        {
            return new Case
            {
                IntegrationFormFields = _integrationFormFields,
                DefinitionName = _definitionName
            };
        }

        public CaseBuilder WithIntegrationFormField(string name, string value)
        {
            _integrationFormFields.Add(new CustomField{ Name = name, Value = value});
            return this;
        }

        public CaseBuilder WithDefinitionName(string name)
        {
            _definitionName = name;
            return this;
        }
    }
}
