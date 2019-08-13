using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Models.Fostering;
using StockportGovUK.NetStandard.Models.Models.Verint;

namespace fostering_service.Helpers
{
    public interface ICaseHelper
    {
        List<PreviousAddress> CreateAddressHistoryList(List<CustomField> formFields, bool isSecondApplicant = false);
    }
}
