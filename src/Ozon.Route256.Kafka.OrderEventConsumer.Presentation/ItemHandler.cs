using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Order;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation;

public class ItemHandler : IHandler<Ignore, string>
{
    private readonly ILogger<ItemHandler> _logger;
    private readonly IItemRepository _itemRepository;

    public ItemHandler(ILogger<ItemHandler> logger, IItemRepository itemRepository)
    {
        _logger = logger;
        _itemRepository = itemRepository;
    }

    [Obsolete]
    public async Task Handle(IReadOnlyCollection<ConsumeResult<Ignore, string>> messages, CancellationToken token)
    {
        foreach (var message in messages)
        {
            var orderEvent = JsonSerializer.Deserialize<OrderEvent>(message.Value);
            if (orderEvent?.Status == Status.Delivered)
            {
                foreach (var position in orderEvent.Positions)
                {
                    long sellerId = long.Parse(position.ItemId.Value.ToString().Substring(0, 6));
                    long productId = long.Parse(position.ItemId.Value.ToString().Substring(6, 6));
                    decimal amount = position.Price.Value;

                    await _itemRepository.UpdateSalesAsync(sellerId, productId, position.Price.Currency, amount, position.Quantity);
                }
            }
        }
    }

}

