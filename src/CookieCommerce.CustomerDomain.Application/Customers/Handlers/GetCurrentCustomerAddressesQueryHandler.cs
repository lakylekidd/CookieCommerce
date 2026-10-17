using CookieCommerce.CustomerDomain.Application.Customers.Queries;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public sealed class GetCurrentCustomerAddressesQueryHandler(IQuerySession querySession)
    : IRequestHandler<GetCurrentCustomerAddressesQuery, IReadOnlyList<CustomerAddressReadModel>>,
        IRequestHandler<GetCurrentCustomerDefaultAddressQuery, CustomerAddressReadModel?>
{
    public async Task<IReadOnlyList<CustomerAddressReadModel>> Handle(
        GetCurrentCustomerAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var readModel = await LoadCustomerAsync(request.CustomerId, cancellationToken);

        return readModel.Addresses
            .OrderBy(address => address.Index)
            .ThenBy(address => address.CreatedAtUtc)
            .ThenBy(address => address.AddressId)
            .ToList();
    }

    public async Task<CustomerAddressReadModel?> Handle(
        GetCurrentCustomerDefaultAddressQuery request,
        CancellationToken cancellationToken)
    {
        var readModel = await LoadCustomerAsync(request.CustomerId, cancellationToken);

        return readModel.Addresses
            .OrderBy(address => address.Index)
            .ThenBy(address => address.CreatedAtUtc)
            .ThenBy(address => address.AddressId)
            .FirstOrDefault(address => address.IsDefault);
    }

    private async Task<CustomerReadModel> LoadCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var readModel = await querySession.LoadAsync<CustomerReadModel>(customerId, cancellationToken);

        if (readModel is null)
            throw new MissingCustomerProfileException(customerId);

        return readModel;
    }
}
