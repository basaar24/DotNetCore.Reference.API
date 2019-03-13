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
    [Route("api/v1/[controller]")]
    [EnableCors("CorsPolicy")]
    public class AgentController : DotNetCoreAPIController
    {
        private IAgentRepository _implementation;

        public AgentController(IAgentRepository implementation)
        {
            _implementation = implementation;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Agent item)
        {
            item.Validate();
            await _implementation.CreateAsync(item);

            Helpers.SendReceiveQueueClient sender = new Helpers.SendReceiveQueueClient();
            await sender.SendMessagesAsync(item);
            await sender.CloseAsync();

            return Created("GetAgentById", new { id = item.Id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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

            var properties = (new Agent()).GetType().GetProperties();

            if (!sieveModel.AreFiltersValid())
            {
                return new JsonResult(BadRequest());
            }

            string message = string.Empty;

            if (!sieveModel.AreFieldsValid(properties, out message))
            {
                return new JsonResult(BadRequest());
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

        [HttpGet("{id}", Name = "GetAgentById")]
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
        public async Task<IActionResult> Update(Agent item)
        {
            if (item.Id <= 0)
            {
                return BadRequest();
            }

            item.Validate();
            await _implementation.UpdateAsync(item);
            return Ok();
        }

        [HttpGet("swagger", Name = "GetSwaggerSpec")]
        public IActionResult GetSwaggerSpec(string format)
        {
            if (format.Equals("json"))
            {
                var provider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(System.IO.Directory.GetCurrentDirectory());
                var contents = provider.GetDirectoryContents(string.Empty);
                var fileInfo = provider.GetFileInfo("Swagger.json");
                return Ok(fileInfo.CreateReadStream());
            }

            return BadRequest();
        }
    }
}