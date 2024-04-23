using System.Threading.Tasks;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain;

public interface IItemRepository
{
    Task UpdateInventoryAsync(long itemId, int reserved, int sold, int cancelled);

    Task UpdateSalesAsync(long sellerId, long productId, string currency, decimal amount, int quantity);
}
