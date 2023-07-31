using StockportGovUK.NetStandard.Gateways.Models.Fostering;
using StockportGovUK.NetStandard.Gateways.Models.Verint;

namespace fostering_service.Helpers
{
    public interface ICaseHelper
    {
        List<PreviousAddress> CreateAddressHistoryList(List<CustomField> formFields, bool isSecondApplicant = false);
    }
}
