using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Order;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation;
using System.Text.Json;
using Xunit;

namespace Tests
{
    public class ItemHandlerTests
    {
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<ILogger<ItemHandler>> _loggerMock;
        private readonly ItemHandler _handler;

        public ItemHandlerTests()
        {
            _itemRepositoryMock = new Mock<IItemRepository>();
            _loggerMock = new Mock<ILogger<ItemHandler>>();
            _handler = new ItemHandler(_loggerMock.Object, _itemRepositoryMock.Object);
        }


        [Fact]
        [Obsolete]
        public async Task Handle_CreatedOrder_UpdatesInventoryAsReserved()
        {
            // Arrange
            var orderEvent = new OrderEvent(
                new OrderId(1),            
                new UserId(1),             
                new WarehouseId(1),        
                Status.Created,           
                DateTime.UtcNow,           
                new[]                      
                {
            new OrderEventPosition(
                new ItemId(1),
                1,
                new Money(100, "USD")
            )
                }
            );

            var serializedOrderEvent = JsonSerializer.Serialize(orderEvent);

            var message = new ConsumeResult<Ignore, string>
            {
                Message = new Message<Ignore, string> { Value = serializedOrderEvent }
            };

            // Act
            await _handler.Handle(new[] { message }, CancellationToken.None);

            // Assert
            _itemRepositoryMock.Verify(repo => repo.UpdateInventoryAsync(1, 1, 0, 0), Times.Once);
        }


        [Fact]
        [Obsolete]
        public async Task Handle_DeliveredOrder_UpdatesSalesAsync()
        {
            // Arrange
            var orderEvent = new OrderEvent(
                new OrderId(2),
                new UserId(2),
                new WarehouseId(2),
                Status.Delivered,
                DateTime.UtcNow,
                new[]
                {
                    new OrderEventPosition(
                        new ItemId(123456789012),
                        5,
                        new Money(200, "USD")
                    )
                }
            );

            var serializedOrderEvent = JsonSerializer.Serialize(orderEvent);
            var message = new ConsumeResult<Ignore, string>
            {
                Message = new Message<Ignore, string> { Value = serializedOrderEvent }
            };

            decimal expectedAmount = orderEvent.Positions[0].Price.Value * orderEvent.Positions[0].Quantity;

            // Act
            await _handler.Handle(new[] { message }, CancellationToken.None);

            // Assert
            _itemRepositoryMock.Verify(repo => repo.UpdateSalesAsync(
                123456,                  
                789012,                 
                "USD",                   
                expectedAmount,         
                5                       
            ), Times.Once);
        }
    }

}