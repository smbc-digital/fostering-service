using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Services
{
    public interface IFosteringService
    {
        Task<FosteringCase> GetCase(string caseId);
    }
}
