using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class AddCustomerAddressHandlerTests
{
    [Fact]
    public void ValidateThrowsWithoutAddressMutation_GivenRequiredFieldsAreInvalid()
    {
        var validator = new CustomerAddressValidator();
        var existingAddresses = new List<Address>();

        var act = () => validator.Validate(
            icon: "home",
            label: " ",
            firstName: "Jamie",
            lastName: "Baker",
            street: "Cookie Lane",
            houseNumber: "12",
            houseNumberAddition: null,
            city: "Baker City",
            postalCode: "1234 AB",
            province: "North Brabant",
            country: "Netherlands",
            phone: "+31612345678",
            isDefault: true,
            existingAddresses: existingAddresses);

        act.Should().Throw<CustomerAddressValidationException>()
            .Which.Errors.Should().ContainKey("label");
        existingAddresses.Should().BeEmpty();
    }

    [Fact]
    public void EncryptEncryptsAddressPii_GivenValidAddress()
    {
        var encryptionService = Substitute.For<IEncryptionService>();
        var customerKey = new byte[] { 1, 2, 3 };
        encryptionService.Encrypt(Arg.Any<string>(), customerKey).Returns(call => $"enc-{call.ArgAt<string>(0)}");
        var encryption = new CustomerAddressEncryption(encryptionService);
        var address = new ValidatedCustomerAddress(
            "home",
            "Home",
            "Jamie",
            "Baker",
            "Cookie Lane",
            "12",
            "A",
            "Baker City",
            "1234 AB",
            "North Brabant",
            "Netherlands",
            "+31612345678",
            true);

        var result = encryption.Encrypt(customerKey, address);

        result.EncryptedFirstName.Should().Be("enc-Jamie");
        result.EncryptedStreet.Should().Be("enc-Cookie Lane");
        result.EncryptedHouseNumberAddition.Should().Be("enc-A");
        result.EncryptedPhone.Should().Be("enc-+31612345678");
    }
}
