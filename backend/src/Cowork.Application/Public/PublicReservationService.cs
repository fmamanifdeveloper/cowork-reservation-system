using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Application.Reservations;
using Cowork.Domain.Entities;

namespace Cowork.Application.Public;

public sealed class PublicReservationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ReservationService _reservationService;
    private readonly IUnitOfWork _unitOfWork;

    public PublicReservationService(
        ICustomerRepository customerRepository,
        ReservationService reservationService,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PublicReservationResponse> CreateAsync(
        PublicCreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerFullName))
            throw new BusinessRuleException("Customer full name is required.");

        var customer = await GetOrCreateCustomerAsync(request, cancellationToken);

        var reservation = await _reservationService.CreateAsync(
            new CreateReservationRequest(
                request.SpaceId,
                customer.Id,
                request.StartTime,
                request.EndTime),
            cancellationToken);

        return new PublicReservationResponse(
            reservation.Id,
            reservation.ReservationCode,
            reservation.CustomerId,
            reservation.SpaceId,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Status,
            reservation.BaseAmount,
            reservation.FinalAmount,
            reservation.PricingBreakdown);
    }

    private async Task<Customer> GetOrCreateCustomerAsync(
        PublicCreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(
                request.CustomerEmail,
                cancellationToken);

            if (existingCustomer is not null)
                return existingCustomer;
        }

        var customer = new Customer(
            Guid.NewGuid(),
            request.CustomerFullName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.CustomerDocumentNumber);

        _customerRepository.Add(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer;
    }
}