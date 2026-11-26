using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;
using Shared.Messages;
using OrdersAPI.Models;

namespace OrdersAPI.Consumer;

public class ProductCreatedConsumer(OrdersDbContext context) : IConsumer<ProductCreated>
{
    public async Task Consume(ConsumeContext<ProductCreated> consumeContext)
    {
        var message = consumeContext.Message;
        var product = await context.Products.FirstOrDefaultAsync(item => item.Id == message.Id);
        if (product is null)
        {
            context.Products.Add(new Product
            {
                Id = message.Id,
                Name = message.Name,
                Price = message.Price,
                Stock = message.Stock,
                Description = message.Description
            });
        }
        else
        {
            product.Name = message.Name;
            product.Price = message.Price;
            product.Stock = message.Stock;
            product.Description = message.Description;
        }

        await context.SaveChangesAsync();
    }
}
