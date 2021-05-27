using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZPool.Pages.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ZPool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZPool.Models;
using Microsoft.AspNetCore.Mvc;

namespace ZPool.Pages.Bookings.Tests
{
    [TestClass()]
    public class CreateModelTests
    {
        //dependencies
        private readonly Mock<IBookingService> mockBookingService;

        //test target
        private readonly CreateModel createModel;

        public CreateModelTests()
        {

            //instantiate dependency mockup
            mockBookingService = new Mock<IBookingService>();

            //instantiate test target
            createModel = new CreateModel(mockBookingService.Object);
        }

        [TestMethod()]
        public void CreateModelTest()
        {
            //verify test is ready
            Assert.IsNotNull(mockBookingService);
            Assert.IsNotNull(createModel);
        }

        [TestMethod()]
        public void OnGetTest()
        {
            //Act
            var result = createModel.OnGet();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod()]
        public void OnPostAsyncTest()
        {

            //Arrange: default ModelState.IsValid is false
            mockBookingService.Setup(mockBookingService => mockBookingService.AddBooking(It.IsAny<Booking>()));

            //Act
            var result = createModel.OnPostAsync(new Booking());

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }
    }
}