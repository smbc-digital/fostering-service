using System.Collections.Generic;
using System.Linq;
using StockportGovUK.NetStandard.Models.Models.Verint;
using StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Mappers
{
    public static class ReferenceDetailsMapper
    {
        public static ReferenceDetails MapToReferenceDetails(
                                                    List<CustomField> fields, 
                                                    string firstNameFieldName, 
                                                    string lastNameFieldName, 
                                                    string relationshipFieldName,
                                                    string yearsKnownFieldName,
                                                    string emailFieldName,
                                                    string phoneFieldName,
                                                    string addressFieldName, 
                                                    string placeRefFieldName, 
                                                    string postcodeFieldName)
        {
            var referenceDetails = new ReferenceDetails
            {
                FirstName = fields.FirstOrDefault(_ => _.Name == firstNameFieldName)?.Value ?? string.Empty,
                LastName = fields.FirstOrDefault(_ => _.Name == lastNameFieldName)?.Value ?? string.Empty,
                RelationshipToYou = fields.FirstOrDefault(_ => _.Name == relationshipFieldName)?.Value ?? string.Empty,
                NumberOfYearsKnown = fields.FirstOrDefault(_ => _.Name == yearsKnownFieldName)?.Value ?? string.Empty,
                EmailAddress = fields.FirstOrDefault(_ => _.Name == emailFieldName)?.Value ?? string.Empty,
                PhoneNumber = fields.FirstOrDefault(_ => _.Name == phoneFieldName)?.Value ?? string.Empty,
                Address = AddressMapper.MapToFosteringAddress(fields, addressFieldName, placeRefFieldName, postcodeFieldName)
            };

            return referenceDetails;
        }
    }
}
