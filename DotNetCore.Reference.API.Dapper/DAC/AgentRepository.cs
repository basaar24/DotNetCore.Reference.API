using DotNetCore.API.Common.DotNetCoreSieve;
using DotNetCore.Reference.API.Dapper.Models;
using Dapper;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.API.Common.Exceptions;

namespace DotNetCore.Reference.API.Dapper.DAC
{
    public class AgentRepository : IAgentRepository
    {
        private readonly IConfiguration _config;
        private readonly IDbConnection _connection;

        public AgentRepository(IConfiguration config)
        {
            _config = config;

            // https://jeremylindsayni.wordpress.com/2018/03/15/using-the-azure-key-vault-to-keep-secrets-out-of-your-web-apps-source-code/
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = keyVaultClient.GetSecretAsync(_config.GetConnectionString("DotNetCoreKeyVault")).Result.Value;
            _connection = new SqlConnection(secret.ToString());
        }

        public Task CreateAsync(Agent item)
        {
            using (IDbConnection conn = _connection)
            {
                string sQuery = "INSERT INTO agents(name, location, active) " +
                    "VALUES(@name, @location, @active);";
                conn.Open();

                int rowsAffected = conn.ExecuteAsync(sQuery, new { name = item.Name, location = item.Location, active = item.Active, id = item.Id }).Result;

                conn.Close();

                return Task.CompletedTask;
            }
        }

        public Task DeleteAsync(long id)
        {
            using (IDbConnection conn = _connection)
            {
                string sQuery = "DELETE FROM agents WHERE id = @id;";
                conn.Open();

                int rowsAffected = conn.ExecuteAsync(sQuery, new { id }).Result;

                conn.Close();

                return Task.CompletedTask;
            }
        }

        public async Task<AgentResponse> GetAsync(string filters, string sorts, int page, int pageSize, string fields)
        {
            DotNetCoreSieveModel sieveModel = new DotNetCoreSieveModel
            {
                Filters = filters,
                Sorts = sorts,
                Fields = fields
            };

            List<string> filterStrings = new List<string>(), sortStrings = new List<string>();
            string filtering = string.Empty, sorting = string.Empty;

            if (sieveModel.GetFiltersParsed() != null)
            {
                foreach (var filter in sieveModel.GetFiltersParsed())
                {
                    filterStrings.Add(string.Format("{0}{1}{2}{3}",
                        filter.Names[0],
                        filter.Operator == "==" ? "='" : filter.Operator,
                        filter.Value,
                        filter.Operator == "==" ? "'" : ""));
                }

                filtering = String.Join(" AND ", filterStrings.ToArray());
            }

            if (sieveModel.GetSortsParsed().Count > 0)
            {
                foreach (var sort in sieveModel.GetSortsParsed())
                {
                    sortStrings.Add(string.Format("{0} {1}",
                        sort.Name.Replace("-", ""),
                        sort.Descending ? "DESC" : "ASC"));
                }
            }
            else
            {
                sortStrings.Add("id ASC");
            }

            sorting = String.Join(", ", sortStrings.ToArray());

            using (IDbConnection conn = _connection)
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@filters", filtering);
                parameters.Add("@sorts", sorting);
                parameters.Add("@page", page);
                parameters.Add("@pageSize", pageSize);
                parameters.Add("@fields", fields);

                conn.Open();

                var reader = await conn.QueryMultipleAsync("AgentsGet", parameters, commandType: CommandType.StoredProcedure);

                int totalRows = (await reader.ReadAsync<int>()).FirstOrDefault();

                var result = (await reader.ReadAsync<Agent>()).ToList();

                var response = new AgentResponse()
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalRows,
                    Entities = result
                };

                conn.Close();

                return await Task.Run(() => response);
            }
        }

        public async Task<Agent> GetByIdAsync(long id)
        {
            try
            {
                using (IDbConnection conn = _connection)
                {
                    string sQuery = "SELECT id, name, location, active " +
                        "FROM agents WHERE id = @id;";
                    conn.Open();

                    var result = await conn.QueryAsync<Agent>(sQuery, new { id });

                    conn.Close();

                    return result.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task UpdateAsync(Agent item)
        {
            using (IDbConnection conn = _connection)
            {
                string sQuery = "UPDATE agents " +
                    "SET name = @name, " +
                    "location = @location," +
                    "active = @active " +
                    "WHERE id = @id;";
                conn.Open();

                int rowsAffected = conn.ExecuteAsync(sQuery, new { name = item.Name, location = item.Location, active = item.Active, id = item.Id }).Result;

                if (rowsAffected == 0)
                {
                    throw new DotNetCoreAPICommonException();
                }

                conn.Close();

                return Task.CompletedTask;
            }
        }
    }
}
