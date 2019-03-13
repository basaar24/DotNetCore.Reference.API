using System.Collections.Generic;

namespace DotNetCore.Reference.API.Dapper.Models
{
    public class AgentResponse
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<Agent> Entities { get; set; }
    }
}
