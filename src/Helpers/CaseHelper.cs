using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Verint;

namespace fostering_service.Helpers
{
    public class CaseHelper : ICaseHelper
    {
        public List<PreviousAddress> CreateAddressHistoryList(List<CustomField> formFields, bool isSecondApplicant = false)
        {
            var applicantPrefix = !isSecondApplicant ? "1" : "2";

            var addressList = new List<PreviousAddress>();
            var currentAddressInfo = new PreviousAddress();

            var currentDateFromMonth = formFields.FirstOrDefault(_ => _.Name == $"currentdatefrommonthapplicant{applicantPrefix}")?.Value;
            var currentDateFromYear = formFields.FirstOrDefault(_ => _.Name == $"currentdatefromyearapplicant{applicantPrefix}")?.Value;


            if (currentDateFromMonth == null || currentDateFromYear == null)
            {
                currentAddressInfo = new PreviousAddress { Address = new InternationalAddress() };
            }
            else
            {
                currentAddressInfo = new PreviousAddress
                {
                    DateFrom = new DateTime(int.Parse(currentDateFromYear), int.Parse(currentDateFromMonth), 01),
                    Address = new InternationalAddress()
                };
            }

            for (int i = 1; i < 9; i++)
            {
                var previousAddress = new PreviousAddress();
                var intAddress = new InternationalAddress();
                var address = formFields.FirstOrDefault(_ => _.Name == $"pa{i}applicant{applicantPrefix}");
                if (address != null)
                {
                    var splitAddress = address.Value.Split("|");
                    intAddress.AddressLine1 = splitAddress[0];
                    intAddress.AddressLine2 = splitAddress[1];
                    intAddress.Town = splitAddress[2];
                    intAddress.County = splitAddress[3];
                    intAddress.Country = splitAddress[4];
                    intAddress.Postcode = formFields.FirstOrDefault(_ => _.Name == $"pa{i}postcodeapplicant{applicantPrefix}")?.Value;
                }

                var month = formFields.FirstOrDefault(_ => _.Name == $"pa{i}datefrommonthapplicant{applicantPrefix}");
                var year = formFields.FirstOrDefault(_ => _.Name == $"pa{i}datefromyearapplicant{applicantPrefix}");

                if (month != null && year != null)
                {
                    previousAddress.DateFrom = new DateTime(int.Parse(year.Value), int.Parse(month.Value), 01);
                }

                previousAddress.Address = intAddress;

                addressList.Add(previousAddress);
            }

            if (formFields.FirstOrDefault(_ => _.Name == $"addressadditionalinformation{applicantPrefix}") != null)
            {
                var additionalAddressList =
                    formFields.FirstOrDefault(_ => _.Name == $"addressadditionalinformation{applicantPrefix}").Value.Split("—");

                for (int i = 0; i < additionalAddressList.Length; i++)
                {
                    var splitAdditionalAddress = additionalAddressList[i].Split("|");
                    var previousAddress = new PreviousAddress
                    {
                        Address = new InternationalAddress()
                    };

                    previousAddress.Address.AddressLine1 = splitAdditionalAddress[0];
                    previousAddress.Address.AddressLine2 = splitAdditionalAddress[1];
                    previousAddress.Address.Town = splitAdditionalAddress[2];
                    previousAddress.Address.County = splitAdditionalAddress[3];
                    previousAddress.Address.Country = splitAdditionalAddress[4];
                    previousAddress.Address.Postcode = splitAdditionalAddress[5];
                    previousAddress.DateFrom = new DateTime(int.Parse(splitAdditionalAddress[7]), int.Parse(splitAdditionalAddress[6]), 1);

                    addressList.Add(previousAddress);
                }
            }

            var sortedAddressList = addressList
                .Where(address =>
                    address.DateFrom != null &&
                    address.Address != null)
                .ToList();

            var addressResult = new List<PreviousAddress>();

            addressResult.Add(currentAddressInfo);
            addressResult.AddRange(sortedAddressList);

            return addressResult;
        }
    }
}
