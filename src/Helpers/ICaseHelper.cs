using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Fostering;
using StockportGovUK.NetStandard.Models.Verint;

namespace fostering_service.Helpers
{
    public interface ICaseHelper
    {
        List<PreviousAddress> CreateAddressHistoryList(List<CustomField> formFields, bool isSecondApplicant = false);
    }
}
