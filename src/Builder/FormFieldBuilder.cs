using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Verint;

namespace fostering_service.Builder
{
    public class FormFieldBuilder
    {
        private List<IntegrationFormField> _formFields = new List<IntegrationFormField>();

        public List<IntegrationFormField> Build()
        {
            return _formFields;
        }

        public FormFieldBuilder AddField(string name, string value)
        {
            _formFields.Add(new IntegrationFormField { FormFieldName = name, FormFieldValue = value});
            return this;
        }
    }
}
