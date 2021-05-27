using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZPool.Areas.Identity.Pages.Account.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.Identity;
using ZPool.Models;
using ZPool.Services.Interfaces;
using Sentry.Protocol;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ZPool.Areas.Identity.Pages.Account.Manage.Tests
{
    [TestClass()]
    public class WhoBookTheRideModelTests
    {
        private readonly Mock<UserManager<AppUser>> mockUserManager;
        private readonly Mock<IBookingService> mockBookingService;
        private readonly Mock<IRideService> mockRideService;

        private readonly List<AppUser> users = new List<AppUser> { new AppUser() { Id = 1 }, new AppUser() { Id = 2 } };

        private readonly WhoBookTheRideModel whoBookTheRideModel;

        public WhoBookTheRideModelTests()
        {
            mockUserManager = MockUserManager<AppUser>(users);

            mockBookingService = new Mock<IBookingService>();
            mockRideService = new Mock<IRideService>();

            whoBookTheRideModel = new WhoBookTheRideModel(mockUserManager.Object, mockBookingService.Object, mockRideService.Object);
        }

        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }


        [TestMethod()]
        public void WhoBookTheRideModelTest()
        {
            Assert.IsNotNull(mockUserManager);
            Assert.IsNotNull(mockBookingService);
            Assert.IsNotNull(mockRideService);
            Assert.IsNotNull(whoBookTheRideModel);
        }

        [TestMethod()]
        public void OnGetAsyncTest()
        {
            //Arrange
            AppUser appUser = new AppUser();
            Ride myRide = new Ride();
            List<Booking> bookings = new List<Booking>() { new Booking() { BookingID = 1 }, new Booking() { BookingID = 2 } };
            int accountOfBookings = bookings.Count();

            mockUserManager.Setup(mockUserManager => mockUserManager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(appUser);

            mockRideService.Setup(mockRideService => mockRideService.GetRide(It.IsAny<int>())).Returns(myRide);

            mockBookingService.Setup(mockBookingService => mockBookingService.GetBookingsByRideId(It.IsAny<int>()))
                .Returns(bookings);

            //Act
            Task task = whoBookTheRideModel.OnGetAsync(1);
            var rideBookings = whoBookTheRideModel.BookingsOfOneRide;

            //Assert
            Assert.IsNotNull(task);
            Assert.IsNotNull(rideBookings);
            Assert.IsTrue(rideBookings.Count() == accountOfBookings);
        }

        [TestMethod()]
        public void OnPostAcceptTest()
        {
            //Arrange
            mockBookingService.Setup(mockBookingService => mockBookingService.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<String>()));

            Booking mockBooking = new Booking() { BookingID = 2, RideID = 10 };
            mockBookingService.Setup(mockBookingService => mockBookingService.GetBookingsByID(It.IsAny<int>())).Returns(mockBooking);

            Ride myRide = new Ride() { RideID = 20 };
            mockRideService.Setup(mockRideService => mockRideService.GetRide(It.IsAny<int>())).Returns(myRide);

            //Act
            var task = whoBookTheRideModel.OnPostAccept(1);

            //Assert
            Assert.IsTrue(whoBookTheRideModel.MyRide.RideID == 20);
        }


        [TestMethod()]
        public void WhenOnPostAccept_DaoThrowException()
        {
            //Arrange
            var expected = "The status of cancelled or rejected bookings cannot be changed.";
            mockBookingService.Setup(mockBookingService => mockBookingService.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<String>()))
                .Throws(new ArgumentException(expected));

            //Act
            var task = whoBookTheRideModel.OnPostAccept(1);
            var actual = whoBookTheRideModel.Message;

            //Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void OnPostRejectTest()
        {
            //Arrange
            mockBookingService.Setup(mockBookingService => mockBookingService.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<String>()));

            Booking mockBooking = new Booking() { BookingID = 2, RideID = 10 };
            mockBookingService.Setup(mockBookingService => mockBookingService.GetBookingsByID(It.IsAny<int>())).Returns(mockBooking);

            Ride myRide = new Ride() { RideID = 20 };
            mockRideService.Setup(mockRideService => mockRideService.GetRide(It.IsAny<int>())).Returns(myRide);

            //Act
            var task = whoBookTheRideModel.OnPostReject(1);

            //Assert 
            Assert.IsTrue(whoBookTheRideModel.MyRide.RideID == 20);
        }

        [TestMethod()]
        public void whenPostReject_ItThrowsException()
        {
            //Arrange
            var expected = "rejected";
            mockBookingService.Setup(mockBookingService => mockBookingService.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<String>()))
                .Throws(new ArgumentException(expected));

            //Act
            var task = whoBookTheRideModel.OnPostReject(1);
            var actual = whoBookTheRideModel.Message;

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void OnPostCancelTest()
        {
            //Arrange
            mockBookingService.Setup(mockBookingService => mockBookingService.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<String>()));

            Booking mockBooking = new Booking() { BookingID = 2, RideID = 10 };
            mockBookingService.Setup(mockBookingService => mockBookingService.GetBookingsByID(It.IsAny<int>())).Returns(mockBooking);

            Ride myRide = new Ride() { RideID = 20 };
            mockRideService.Setup(mockRideService => mockRideService.GetRide(It.IsAny<int>())).Returns(myRide);

            //Act
            var task = whoBookTheRideModel.OnPostCancel(1);

            //Assert 
            Assert.IsTrue(whoBookTheRideModel.MyRide.RideID == 20);
        }


        [TestMethod()]
        public void whenPostCancel_ItThrowsException()
        {
            //Arrange
            var expected = "cancelled";
            mockBookingService.Setup(mockBookingService => mockBookingService.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<String>()))
                .Throws(new ArgumentException(expected));

            //Act
            var task = whoBookTheRideModel.OnPostCancel(1);
            var actual = whoBookTheRideModel.Message;

            //Assert
            Assert.AreEqual(expected, actual);
        }

    }
}