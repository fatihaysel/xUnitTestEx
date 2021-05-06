using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdemyUnitTestEx.Web.Controllers;
using UdemyUnitTestEx.Web.Helper;
using UdemyUnitTestEx.Web.Models;
using UdemyUnitTestEx.Web.Repository;
using Xunit;

namespace UdemyUnitTestEx.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepository;

        private readonly ProductsApiController _controller;

        private readonly Helper _helper;

        private List<Product> _products;

        public ProductApiControllerTest()
        {
            _mockRepository = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepository.Object);
            _helper = new Helper();
            _products = new List<Product>()
            {
                new Product { Id = 1, Name = "Kalem", Color = "red", Stock = 50, Price = 15 },
                new Product { Id = 2, Name = "Defter", Color = "blue", Stock = 20, Price = 5 }

            };


        }

        [Theory]
        [InlineData(1,2,3)]
        [InlineData(0,0,0)]
        public void add_SampleValues_ReturnSumOfArguments(int a,int b,int total)
        {
            var result = _helper.Add(a, b);

            Assert.Equal(total, result);
        }

        [Fact]
        public async void GetProducts_ActionExecutes_ReturnOkResultWithProduct()
        {
            _mockRepository.Setup(x => x.GetAll()).ReturnsAsync(_products);

            var result = await _controller.GetProducts();

            var OkResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsAssignableFrom<IEnumerable<Product>>(OkResult.Value);

            Assert.Equal<int>(2, returnProduct.ToList().Count);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetProduct_ProductIsNull_ReturnNotFound(int id)
        {
            Product product = null;

            _mockRepository.Setup(x => x.GetById(id)).ReturnsAsync(product);

            var result = await _controller.GetProduct(id);

            Assert.IsType<NotFoundResult>(result);
            //NotFound() icerisinde data olsaydi <NotFoundObjectResult>
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetProduct_ProductIsValid_ReturnsOkWithProduct(int Id)
        {
            Product product = _products.First(x => x.Id == Id);

            _mockRepository.Setup(x => x.GetById(Id)).ReturnsAsync(product);

            var result = await _controller.GetProduct(Id);

            var OkResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsType<Product>(OkResult.Value);

            Assert.Equal(Id, returnProduct.Id);
            Assert.Equal(product.Name, returnProduct.Name);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void PutProduct_IdNotEqualProductId_ReturnBadRequest(int Id)
        {
            Product products = _products.First(x => x.Id == Id);

            var result = _controller.PutProduct(0, products);

            Assert.IsType<BadRequestResult>(result);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void PutProduct_IdIsValid_Return_NoContent(int Id)
        {
            Product product = _products.First(x => x.Id == Id);

            var result = _controller.PutProduct(Id, product);

            Assert.IsType<NoContentResult>(result);

            _mockRepository.Setup(x => x.Update(product));

            _mockRepository.Verify(x => x.Update(product), Times.Once());
        }

        [Fact]
        public async void PostProduct_ActionExecute_ReturnCreatedAtAction()
        {
            Product product = _products.First();

            _mockRepository.Setup(x => x.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            var returnResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal("GetProduct", returnResult.ActionName);

            _mockRepository.Verify(x => x.Create(product), Times.Once());

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void DeleteProduct_ProductIsNull_ReturnNotFound(int Id)
        {

            Product product = null;

            _mockRepository.Setup(x => x.GetById(Id)).ReturnsAsync(product);

            var result = await _controller.DeleteProduct(Id);

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void DeleteProduct_ProductIsValid_ReturnOkWithId(int Id)
        {

            Product product = _products.First(x=>x.Id == Id);

            _mockRepository.Setup(x => x.GetById(Id)).ReturnsAsync(product);

            var result = await _controller.DeleteProduct(Id);

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            _mockRepository.Verify(x => x.Delete(product), Times.Once());
        }



    }
}
