using Npgsql;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using System.Threading.Tasks;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;

public sealed class ItemRepository : IItemRepository
{
    private readonly string _connectionString;

    public ItemRepository(string connectionString) => _connectionString = connectionString;

    public async Task UpdateInventoryAsync(long itemId, int reserved, int sold, int cancelled)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new NpgsqlCommand(
            @"UPDATE Inventory SET 
                Reserved = Reserved + @reserved,
                Sold = Sold + @sold,
                Cancelled = Cancelled + @cancelled,
                LastUpdated = CURRENT_TIMESTAMP
              WHERE ItemId = @itemId", connection);

        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@reserved", reserved);
        command.Parameters.AddWithValue("@sold", sold);
        command.Parameters.AddWithValue("@cancelled", cancelled);

        await command.ExecuteNonQueryAsync();
    }
    public async Task UpdateSalesAsync(long sellerId, long productId, string currency, decimal amount, int quantity)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new NpgsqlCommand(@"
            INSERT INTO Sales (SellerId, ProductId, Currency, TotalSalesAmount, TotalUnitsSold, LastUpdated)
            VALUES (@sellerId, @productId, @currency, @amount, @quantity, CURRENT_TIMESTAMP)
            ON CONFLICT (SellerId, ProductId, Currency) 
            DO UPDATE SET 
                TotalSalesAmount = Sales.TotalSalesAmount + EXCLUDED.TotalSalesAmount,
                TotalUnitsSold = Sales.TotalUnitsSold + EXCLUDED.TotalUnitsSold,
                LastUpdated = CURRENT_TIMESTAMP
            ", connection);

        command.Parameters.AddWithValue("@sellerId", sellerId);
        command.Parameters.AddWithValue("@productId", productId);
        command.Parameters.AddWithValue("@currency", currency);
        command.Parameters.AddWithValue("@amount", amount);
        command.Parameters.AddWithValue("@quantity", quantity);

        await command.ExecuteNonQueryAsync();
    }

}
