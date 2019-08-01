using System.Collections.Generic;
using System.Linq;
using StockportGovUK.NetStandard.Models.Models.Verint;
using Address = StockportGovUK.NetStandard.Models.Models.Fostering.Address;

namespace fostering_service.Mappers
{
    public static class AddressMapper
    {
        public static Address MapToFosteringAddress(List<CustomField> fields, string addressFieldName, string placeRefFieldName, string postcodeFieldName)
        {
            var address = new Address
            {
                AddressLine1 = string.Empty,
                AddressLine2 = string.Empty,
                Town = string.Empty
            };

            address.PlaceRef = fields.FirstOrDefault(_ => _.Name == placeRefFieldName)?.Value ?? string.Empty;
            address.Postcode = fields.FirstOrDefault(_ => _.Name == postcodeFieldName)?.Value ?? string.Empty;

            var addressFieldValue = fields.FirstOrDefault(_ => _.Name == addressFieldName)?.Value;

            if (string.IsNullOrEmpty(address.PlaceRef) && !string.IsNullOrEmpty(addressFieldValue))
            {
                var splittedAddress = addressFieldValue.Split("|");

                switch (splittedAddress.Length)
                {
                    case 0:
                        break;
                    case 1:
                        address.AddressLine1 = splittedAddress[0];
                        break;
                    case 2:
                        address.AddressLine1 = splittedAddress[0];
                        address.AddressLine2 = splittedAddress[1];
                        break;
                    default:
                        address.AddressLine1 = splittedAddress[0];
                        address.AddressLine2 = splittedAddress[1];
                        address.Town = splittedAddress[2];
                        break;
                }
            }

            if (!string.IsNullOrEmpty(address.PlaceRef) && !string.IsNullOrEmpty(addressFieldValue))
            {
                address.SelectedAddress = addressFieldValue;
            }

            return address;
        }

        public static List<IntegrationFormField> MapToVerintAddress(Address address, string addressFieldName, string placeRefFieldName, string postcodeFieldName)
        {
            var fields = new List<IntegrationFormField>
            {
                new IntegrationFormField()
                {
                    FormFieldName = postcodeFieldName,
                    FormFieldValue = address.Postcode
                }
            };

            if (!string.IsNullOrEmpty(address.PlaceRef))
            {
                fields.AddRange(new [] {
                    new IntegrationFormField
                    {
                        FormFieldName = placeRefFieldName,
                        FormFieldValue = address.PlaceRef
                    },
                    new IntegrationFormField
                    {
                        FormFieldName=  addressFieldName,
                        FormFieldValue=  address.SelectedAddress
                    }
                });
            }
            else
            {
                fields.AddRange(new [] {
                    new IntegrationFormField
                    {
                        FormFieldName = addressFieldName,
                        FormFieldValue = $"{address.AddressLine1}|{address.AddressLine2}|{address.Town}"
                    },
                    new IntegrationFormField
                    {
                        FormFieldName = placeRefFieldName,
                        FormFieldValue = null
                    }
                });
            }

            return fields;
        }

        public static Address Validate(this Address address)
        {
            if (!string.IsNullOrEmpty(address.AddressLine1) && !string.IsNullOrEmpty(address.Town) &&
                !string.IsNullOrEmpty(address.Postcode))
            {
                return address;
            }

            if (!string.IsNullOrEmpty(address.PlaceRef) && !string.IsNullOrEmpty(address.Postcode))
            {
                return address;
            }

            return null;
        }
    }
}
