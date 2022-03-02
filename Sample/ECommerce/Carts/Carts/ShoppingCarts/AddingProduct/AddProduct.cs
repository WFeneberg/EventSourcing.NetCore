using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.Pricing;
using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace Carts.ShoppingCarts.AddingProduct;

public class AddProduct: ICommand
{

    public Guid CartId { get; }

    public ProductItem ProductItem { get; }

    private AddProduct(Guid cartId, ProductItem productItem)
    {
        CartId = cartId;
        ProductItem = productItem;
    }
    public static AddProduct Create(Guid cartId, ProductItem productItem)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new AddProduct(cartId, productItem);
    }
}

internal class HandleAddProduct:
    ICommandHandler<AddProduct>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;
    private readonly IProductPriceCalculator productPriceCalculator;

    public HandleAddProduct(
        IMartenRepository<ShoppingCart> cartRepository,
        IProductPriceCalculator productPriceCalculator
    )
    {
        this.cartRepository = cartRepository;
        this.productPriceCalculator = productPriceCalculator;
    }

    public Task<Unit> Handle(AddProduct command, CancellationToken cancellationToken)
    {
        return cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.AddProduct(productPriceCalculator, command.ProductItem),
            cancellationToken);
    }
}