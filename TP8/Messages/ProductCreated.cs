namespace ProductsAPI.Messages;

public record ProductCreated(int Id, string Name, decimal Price, int Stock, string? Description);
