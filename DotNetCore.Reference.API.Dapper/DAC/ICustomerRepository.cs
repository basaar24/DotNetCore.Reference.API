using DotNetCore.Reference.API.Dapper.Models;
using System.Threading.Tasks;

namespace DotNetCore.Reference.API.Dapper.DAC
{
    public interface ICustomerRepository
    {
        Task CreateAsync(Customer item);
        Task<CustomerResponse> GetAsync(string filters, string sorts, int page, int pageSize, string fields);
        Task DeleteAsync(long id);
        Task<Customer> GetByIdAsync(long id);
        Task UpdateAsync(Customer item);
    }
}
