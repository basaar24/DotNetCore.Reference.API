using DotNetCore.Reference.API.Dapper.Models;
using System.Threading.Tasks;

namespace DotNetCore.Reference.API.Dapper.DAC
{
    public interface IAgentRepository
    {
        Task CreateAsync(Agent item);
        Task<AgentResponse> GetAsync(string filters, string sorts, int page, int pageSize, string fields);
        Task DeleteAsync(long id);
        Task<Agent> GetByIdAsync(long id);
        Task UpdateAsync(Agent item);
    }
}
