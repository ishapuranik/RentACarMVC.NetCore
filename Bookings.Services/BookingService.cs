﻿using Bookings.Domain.Repositories.Bookings;
using Bookings.Domain.Repositories.Vehicles;
using Bookings.Persistence.Repositories.Renters;
using Bookings.Shared.Dtos;
using Bookings.Shared.Enums;
using Bookings.Shared.Persistence;

namespace Bookings.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IRenterRepository _renterRepository;

        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IBookingRepository bookingRepository, IVehicleRepository vehicleRepository, IRenterRepository renterRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _vehicleRepository = vehicleRepository;
            _renterRepository = renterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ConfirmBooking(int bookingID)
        {
            var booking = await _bookingRepository.GetAsync(x => x.ID == bookingID);

            if (booking == null)
                throw new KeyNotFoundException($"Booking record having ID {bookingID} not found.");

            booking.BookingStatusID = (int) EnumBookingStatus.Confirmed;
            _bookingRepository.Update(booking);
        }

        public async Task<int> ReserveBooking(ReserveBookingDto request)
        {
            int totalFleetQuantity = 0;

            try
            {
                totalFleetQuantity = _vehicleRepository.GetVehicleFleetQuantity(request.VehicleID);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }

            int bookedFleetQuantity = 0;
            //Get all records for the given vehicle booked under the same date duration
            var bookings = _bookingRepository.GetQuerable(x => x.VehicleID == request.VehicleID && 
                                                                ((x.RenterStartDate.Date >= request.RenterStartDate.Date &&
                                                                x.RenterEndDate.Date <= request.RenterEndDate.Date)
                                                                ||
                                                                (x.RenterStartDate.Date == request.RenterEndDate.Date)) && 
                                                                (x.BookingStatusID == (int)EnumBookingStatus.Confirmed ||
                                                                x.BookingStatusID == (int)EnumBookingStatus.Reserved)); 

            if (bookings != null || bookings.Any())
                bookedFleetQuantity = bookings.Count();

            if (bookedFleetQuantity == totalFleetQuantity)
                return 0;

            var booking = new Domain.Booking()
            {
                VehicleID = request.VehicleID,
                RenterID = request.RenterID,
                BookingStatusID = (int)EnumBookingStatus.Reserved, 
                Reference = request.Reference,
                RenterStartDate = request.RenterStartDate,
                RenterEndDate = request.RenterEndDate,
                TotalCost = request.TotalCost
            };

            _bookingRepository.Add(booking);

            await _unitOfWork.SaveChangesAsync();

            return booking.ID;
        }
    }
}
