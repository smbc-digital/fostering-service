using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service_tests.Builders
{
    public class CaseBuilder
    {
        private List<CustomField> _integrationFormFields = new List<CustomField>();

        public Case Build()
        {
            return new Case
            {
                IntegrationFormFields = _integrationFormFields
            };
        }

        public CaseBuilder WithIntegrationFormField(string name, string value)
        {
            _integrationFormFields.Add(new CustomField{ Name = name, Value = value});
            return this;
        }
    }
}
