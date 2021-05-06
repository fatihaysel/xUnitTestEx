using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdemyUnitTestEx.Web.Controllers;
using UdemyUnitTestEx.Web.Models;
using UdemyUnitTestEx.Web.Repository;
using Xunit;

namespace UdemyUnitTestEx.Test
{
    public class ProductsControllerTest
    {
        private Mock<IRepository<Product>> _mockRepository;

        private readonly ProductsController _controller;

        private List<Product> _products;

        public ProductsControllerTest()
        {
            _mockRepository = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepository.Object);
            _products = new List<Product>() { new Product { Id = 1, Name = "Kalem", Color = "red", Stock = 50, Price = 15 }, new Product { Id = 2, Name = "Defter", Color = "blue", Stock = 20, Price = 5 } };
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnProductList()
        {
            _mockRepository.Setup(x => x.GetAll()).ReturnsAsync(_products);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            var productsList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            Assert.Equal(2, productsList.Count());

        }

        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Details(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);

        }

        [Fact]
        public async void Details_IdIsValidAndProductIsNull_ReturnsNotFound()
        {
            Product product = null;

            _mockRepository.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Details(0);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Details_IdIsValidAndProductNotNull_ReturnProduct(int productId)
        {
            Product product = _products.First(x => x.Id == productId);

            _mockRepository.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Details(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            Assert.IsType<ViewResult>(_controller.Create());
        }

        [Fact]
        public async void CreatePOST_InValidModelState_ReturnView()
        {
            //InValid model test edildigi icin Error ekledik.
            _controller.ModelState.AddModelError("Name", "Name alani gereklidir.");

            var result = await _controller.Create(_products.First());

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);

        }

        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirecToIndexAction()
        {
            var result = await _controller.Create(_products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMeyhodExecute()
        {
            Product newProduct = null;

            _mockRepository.Setup(x => x.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);

            var result = await _controller.Create(_products.First());

            _mockRepository.Verify(x => x.Create(It.IsAny<Product>()),
                Times.Once);

            Assert.Equal(_products.First().Id, newProduct.Id);
        }

        [Fact]
        public async void CreatePOST_InValidModelState_NeverCreateExecute()
        {
            // InValid model test edildigi icin Error ekledik.
            _controller.ModelState.AddModelError("Name", " ");

            var result = await _controller.Create(_products.First());

            _mockRepository.Verify(x => x.Create(It.IsAny<Product>()), Times.Never);

        }

        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexPage()
        {
            var result = await _controller.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index",redirect.ActionName);
        }

        [Fact]
        public async void Edit_IdIsValidAndProductIsNull_ReturnNotFound()
        {
            var result = await _controller.Edit(0);
            
            Product product = null;

            _mockRepository.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ProductAndIdIsValid_ReturnProduct(int Id)
        {
            Product product = _products.First(x => x.Id == Id);

            _mockRepository.Setup(x => x.GetById(Id)).ReturnsAsync(product);

            var result = await _controller.Edit(Id);

            var view = Assert.IsType<ViewResult>(result);

            var resultModel = Assert.IsAssignableFrom<Product>(view.Model);

            Assert.Equal(product.Id, resultModel.Id);
            Assert.Equal(product.Name, resultModel.Name);
        }

        [Theory]
        [InlineData(1)]
        //[InlineData(2)]
        public void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int Id)
        {
            var result = _controller.Edit(Id, _products.First(x => x.Id == 2));

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void EditPOST_InValidModel_ReturnViewProduct(int Id)
        {
            _controller.ModelState.AddModelError("Name", " ");

            var result = _controller.Edit(Id, _products.First(x => x.Id == Id));

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<Product>(viewResult.Model);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void EditPOST_ModelStateIsValid_ReturnRedirectToIndexAction(int Id)
        {
            var result = _controller.Edit(Id, _products.First(x => x.Id == Id));

            var actionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", actionResult.ActionName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void EditPOST_ModelStateIsValid_UpdateProduct(int Id)
        {
            
            
            _mockRepository.Setup(x => x.Update(_products.First(x=>x.Id == Id)));
            
            _controller.Edit(Id, _products.First(x => x.Id == Id));

            _mockRepository.Verify(x=>x.Update(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Delete_IdIsValidAndProductIsNull_ReturnNotFound(int Id)
        {
            var result = await _controller.Delete(Id);

            Product product = null;

            _mockRepository.Setup(x => x.GetById(Id)).ReturnsAsync(product);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Delete_IdIsValidAndProductIsValid_ReturnNotFound(int Id)
        {
            Product product = _products.First(x => x.Id == Id);

            _mockRepository.Setup(x => x.GetById(Id)).ReturnsAsync(product);

            var result = await _controller.Delete(Id);

            var viewResult = Assert.IsType<ViewResult>(result);

            var ss = Assert.IsAssignableFrom<Product>(viewResult.Model);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void DeletePOST_ActionExecutes_ReturnRedirectToIndexPage(int Id)
        {
            var result = await _controller.DeleteConfirmed(Id);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void DeletePOST_ActionExecutes_DeleteProduct(int Id)
        {
            var product = _products.First(x => x.Id == Id);

            _mockRepository.Setup(x => x.Delete(product));

            await _controller.DeleteConfirmed(Id);

            _mockRepository.Verify(x=>x.Delete(It.IsAny<Product>()),Times.Once());
        }

    }

}
     
