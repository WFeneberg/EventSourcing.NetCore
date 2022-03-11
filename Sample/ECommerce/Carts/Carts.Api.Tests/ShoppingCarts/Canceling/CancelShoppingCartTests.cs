using System.Net;
using Carts.Api.Requests.Carts;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.FinalizingCart;
using Carts.ShoppingCarts.GettingCartById;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts.Canceling;

public class ConfirmShoppingCartFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/ShoppingCarts";

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        var openResponse = await Post(new OpenShoppingCartRequest(ClientId));
        openResponse.EnsureSuccessStatusCode();

        ShoppingCartId = await openResponse.GetResultFromJson<Guid>();

        CommandResponse = await Delete(
            $"{ShoppingCartId}",
            new RequestOptions { IfMatch = 1.ToString() }
        );
    }
}

public class ConfirmShoppingCartTests: IClassFixture<ConfirmShoppingCartFixture>
{
    private readonly ConfirmShoppingCartFixture fixture;

    public ConfirmShoppingCartTests(ConfirmShoppingCartFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Put_Should_Return_OK()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Confirm_ShoppingCart()
    {
        // prepare query
        var query = $"{fixture.ShoppingCartId}";

        //send query
        var queryResponse = await fixture.Get(query, 30,
            check: async response => (await response.GetResultFromJson<ShoppingCartDetails>()).Version == 2);

        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<ShoppingCartDetails>();
        cartDetails.Should().NotBeNull();
        cartDetails.Id.Should().Be(fixture.ShoppingCartId);
        cartDetails.Status.Should().Be(ShoppingCartStatus.Canceled);
        cartDetails.ClientId.Should().Be(fixture.ClientId);
        cartDetails.Version.Should().Be(2);

        fixture.PublishedExternalEventsOfType<CartFinalized>();
    }
}
