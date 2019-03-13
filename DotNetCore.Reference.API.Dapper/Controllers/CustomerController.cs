using DotNetCore.API.Common;
using DotNetCore.API.Common.Helpers;
using DotNetCore.API.Common.DotNetCoreSieve;
using DotNetCore.Reference.API.Dapper.DAC;
using DotNetCore.Reference.API.Dapper.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetCore.Reference.API.Dapper.Controllers
{
    [Produces("application/json")]
    [Route("api/v3/[controller]")]
    [EnableCors("CorsPolicy")]
    public partial class CustomerController : DotNetCoreAPIController
    {
        private ICustomerRepository _implementation;

        public CustomerController(ICustomerRepository implementation)
        {
            _implementation = implementation;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer item)
        {
            item.Validate();
            await _implementation.CreateAsync(item);
            return Created("GetCustomerById", new { id = item.Id });
        }

        [HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            await _implementation.DeleteAsync(id);
            return Ok();
        }

        [HttpGet]
        public async Task<JsonResult> Get(string filters = "", string sorts = "", int page = 1, int pageSize = 100, string fields = "")
        {
            DotNetCoreSieveModel sieveModel = new DotNetCoreSieveModel
            {
                Filters = filters,
                Sorts = sorts,
                Fields = fields
            };

            var properties = (new Customer()).GetType().GetProperties();

            if (!sieveModel.AreFiltersValid())
            {
                throw new DotNetCoreRefDapperException();
            }

            string message = string.Empty;

            if (!sieveModel.AreFieldsValid(properties, out message))
            {
                throw new DotNetCoreRefDapperException();
            }

            var result = await _implementation.GetAsync(filters, sorts, page, pageSize, fields);
            foreach (var res in result.Entities)
            {
                res.SetSerializableProperties(fields);
            }

            return new JsonResult(
                    result,
                    new Newtonsoft.Json.JsonSerializerSettings()
                    {
                        ContractResolver = new ShouldSerializeContractResolver()
                    });
        }

        [HttpGet("{id}", Name = "GetCustomerById")]
        public async Task<IActionResult> GetById(long id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _implementation.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Customer item)
        {
            if (item.Id <= 0)
            {
                throw new DotNetCoreRefDapperException();
            }

            await _implementation.UpdateAsync(item);
            return Ok();
        }
    }
}