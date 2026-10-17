using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;
using NetTopologySuite.Algorithm;
using RTools_NTS.Util;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class GetCurrentCustomerProfileQueryHandler(IQuerySession querySession)
    : IRequestHandler<GetCurrentCustomerProfileQuery, CustomerProfileResult>
{
    public async Task<CustomerProfileResult> Handle(
        GetCurrentCustomerProfileQuery request,
        CancellationToken cancellationToken)
    {
        var readModel = await querySession.LoadAsync<CustomerReadModel>(request.CustomerId, cancellationToken);

        if (readModel is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        return ToResult(readModel);
    }

    internal static CustomerProfileResult ToResult(CustomerReadModel readModel)
    {
        var status = readModel.EmailVerificationStatus == "PendingVerification"
            ? EmailVerificationStatus.PendingVerification
            : EmailVerificationStatus.Verified;

        return new CustomerProfileResult(
            readModel.Id,
            readModel.FirstName,
            readModel.LastName,
            readModel.Email,
            readModel.PendingEmail,
            status,
            readModel.Gender,
            readModel.DateOfBirth);
    }
}
