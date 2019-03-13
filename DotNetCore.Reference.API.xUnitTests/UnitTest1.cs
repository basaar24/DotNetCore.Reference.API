using DotNetCore.Reference.API.Dapper.Controllers;
using DotNetCore.Reference.API.Dapper.DAC;
using DotNetCore.Reference.API.Dapper.Models;
using DotNetCore.Reference.API.Dapper.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DotNetCore.Reference.API.Dapper.Tests
{
    public class UnitTest1
    {
        private IConfiguration Configuration;

        public UnitTest1()
        {
            Configuration = new ConfigurationGetter().Configuration;
        }

        [Fact]
        public async Task GetWithEmptyQueryShouldBeOk()
        {
            // Arrange
            string filters = string.Empty;
            string sorts = string.Empty;
            int page = 1;
            int pageSize = 100;
            string fields = string.Empty;
            var _repositoryMock = new Mock<ICustomerRepository>();

            _repositoryMock.Setup(mock => mock.GetAsync(filters, sorts, page, pageSize, fields))
                .Returns(Task.FromResult(new CustomerResponse
                {
                    Page = 1,
                    PageSize = 100,
                    TotalItems = 2,
                    Entities = new List<Customer>
                        {
                            new Customer { Id = 1, Name = "Customer 01", Jurisdiction = "CA", Active = true },
                            new Customer { Id = 2, Name = "Customer 02", Jurisdiction = "CA", Active = true }
                        }
                }));

            var _controller = new CustomerController(_repositoryMock.Object);

            // Act
            var result = await _controller.Get(filters, sorts, page, pageSize, fields);

            // Assert
            var okResult = result.Should().BeOfType<Microsoft.AspNetCore.Mvc.JsonResult>().Subject;
            var customerResponse = result.Value.Should().BeAssignableTo<CustomerResponse>().Subject;

            customerResponse.Entities.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetWithFilterQueryShouldBeOk()
        {
            // Arrange
            string filters = "id>2";
            string sorts = string.Empty;
            int page = 1;
            int pageSize = 100;
            string fields = string.Empty;
            var _repositoryMock = new Mock<ICustomerRepository>();

            _repositoryMock.Setup(mock => mock.GetAsync(filters, sorts, page, pageSize, fields))
                .Returns(Task.FromResult(new CustomerResponse
                {
                    Page = 1,
                    PageSize = 100,
                    TotalItems = 2,
                    Entities = new List<Customer>
                        {
                            new Customer { Id = 1, Name = "Customer 01", Jurisdiction = "CA", Active = true },
                            new Customer { Id = 2, Name = "Customer 02", Jurisdiction = "CA", Active = true }
                        }
                }));

            var _controller = new CustomerController(_repositoryMock.Object);

            // Act
            var result = await _controller.Get(filters, sorts, page, pageSize, fields);

            // Assert
            var okResult = result.Should().BeOfType<Microsoft.AspNetCore.Mvc.JsonResult>().Subject;
            var customerResponse = result.Value.Should().BeAssignableTo<CustomerResponse>().Subject;

            customerResponse.Entities.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetWithFilterQueryShouldBeOkadasd()
        {
            // Arrange
            string filters = "id>2";
            string sorts = string.Empty;
            int page = 1;
            int pageSize = 100;
            string fields = string.Empty;
            var _repositoryMock = new Mock<ICustomerRepository>();

            //_repositoryMock.Setup(mock => mock.GetAsync(filters, sorts, page, pageSize, fields))
            //    .Returns(Task.FromResult(new CustomerResponse
            //    {
            //        Page = 1,
            //        PageSize = 100,
            //        TotalItems = 2,
            //        Entities = new List<Customer>
            //            {
            //                new Customer { Id = 1, Name = "Customer 01", Jurisdiction = "CA", Active = true },
            //                new Customer { Id = 2, Name = "Customer 02", Jurisdiction = "CA", Active = true }
            //            }
            //    }));

            var _controller = new CustomerController(new CustomerRepository(Configuration));//_repositoryMock.Object);

            // Act
            var result = await _controller.Get(filters, sorts, page, pageSize, fields);

            // Assert
            var okResult = result.Should().BeOfType<Microsoft.AspNetCore.Mvc.JsonResult>().Subject;
            var customerResponse = result.Value.Should().BeAssignableTo<CustomerResponse>().Subject;

            //customerResponse.Entities.Count.Should().Be(1);
        }
    }
}
