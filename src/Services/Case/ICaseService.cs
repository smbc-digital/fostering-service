using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Services.Case
{
    public interface ICaseService
    {
        Task<FosteringCase> GetCase(string caseId);
    }
}
