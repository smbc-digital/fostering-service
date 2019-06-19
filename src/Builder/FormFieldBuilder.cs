using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service.Builder
{
    public class FormFieldBuilder
    {
        private List<CustomField> _formFields = new List<CustomField>();

        public List<CustomField> Build()
        {
            return _formFields;
        }

        public FormFieldBuilder AddField(string name, string value)
        {
            _formFields.Add(new CustomField{ Name = name, Value = value});
            return this;
        }
    }
}
