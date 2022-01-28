using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _avaliableDesks;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;
        private readonly DeskBookingRequestProcessor _processor;

        public DeskBookingRequestProcessorTests()
        {
            _request = new DeskBookingRequest
            {
                FirstName = "Danilo",
                LastName = "Neto",
                Email = "daniloneto@fakemail.com",
                Date = new DateTime(2022, 1, 1)
            };

            _avaliableDesks = new List<Desk> {
                new Desk
                {
                    Id = 7
                }
            };

            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();
            _deskRepositoryMock = new Mock<IDeskRepository>();
            _deskRepositoryMock.Setup(x =>
            x.GetAvaliableDesks(_request.Date)).Returns(_avaliableDesks);

            _processor = new DeskBookingRequestProcessor(
                _deskBookingRepositoryMock.Object, _deskRepositoryMock.Object);

        }
        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            //Act
            DeskBookingResult result = _processor.BookDesk(_request);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(_request.FirstName, result.FirstName);
            Assert.Equal(_request.LastName, result.LastName);
            Assert.Equal(_request.Email, result.Email);
            Assert.Equal(_request.Date, result.Date);
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {            
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));
            Assert.Equal("request", exception.ParamName);
            

        }
        [Fact]
        public void ShouldSaveDeskBooking()
        {
            DeskBooking savedDeskBooking = null;
            _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking =>
                {
                    savedDeskBooking = deskBooking;
                });

            _processor.BookDesk(_request);

            _deskBookingRepositoryMock
                .Verify(x => 
                x.Save(It.IsAny<DeskBooking>()), Times.Once);

            Assert.NotNull(savedDeskBooking);
            Assert.Equal(_request.FirstName, savedDeskBooking.FirstName);
            Assert.Equal(_request.LastName, savedDeskBooking.LastName);
            Assert.Equal(_request.Email, savedDeskBooking.Email);
            Assert.Equal(_request.Date, savedDeskBooking.Date);
            Assert.Equal(_avaliableDesks.First().Id, savedDeskBooking.DeskId);


        } 
        [Fact]
        public void ShouldNotSaveDeskBookingIfNoDeskIsAvaliable()
        {
            _avaliableDesks.Clear();

            _processor.BookDesk(_request);

            _deskBookingRepositoryMock
                .Verify(x =>
                x.Save(It.IsAny<DeskBooking>()), Times.Never);

        }
        [Theory]
        [InlineData(DeskBookingResultCode.Success,true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode(
            DeskBookingResultCode expectedResultCode, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _avaliableDesks.Clear();
            }
            var result = _processor.BookDesk(_request);

            Assert.Equal(expectedResultCode, result.Code);
        }
        [Theory]
        [InlineData(5, true)]
        [InlineData(null,false)]
        public void ShouldReturnExpectedDeskBookId(
           int? expectedDeskbookId, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _avaliableDesks.Clear();
            }
            else
            {
                _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                    .Callback<DeskBooking>(deskBooking =>
                    {
                        deskBooking.Id = expectedDeskbookId.Value;
                    });
            }
            var result = _processor.BookDesk(_request);

            Assert.Equal(expectedDeskbookId, result.DeskBookingId);
        }
    }
}
