using StockportGovUK.NetStandard.Models.Enums;

namespace fostering_service.Extensions
{
    public static class ETaskStatusExtensioncs
    {
        public static string GetTaskStatus(this ETaskStatus value)
        {
            switch (value)
            {
                case ETaskStatus.CantStart:
                    return "CantStart";
                case ETaskStatus.Completed:
                    return "Completed";
                case ETaskStatus.NotCompleted:
                    return "NotCompleted";
                default:
                    return "None";
            }
        }
    }
}
