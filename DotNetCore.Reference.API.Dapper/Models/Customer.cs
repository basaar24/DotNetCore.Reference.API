using DotNetCore.API.Common.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DotNetCore.Reference.API.Dapper.Models
{
    public partial class Customer : DotNetCoreTypeBase
    {
        [JsonProperty("Id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("Name", Required = Required.Always)]
        [Required]
        [StringLength(150, MinimumLength = 1)]
        public string Name { get; set; }

        [JsonProperty("Jurisdiction", Required = Required.Always)]
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Jurisdiction { get; set; }

        [JsonProperty("Active", Required = Required.Always)]
        [Required]
        public bool Active { get; set; }
    }
}
