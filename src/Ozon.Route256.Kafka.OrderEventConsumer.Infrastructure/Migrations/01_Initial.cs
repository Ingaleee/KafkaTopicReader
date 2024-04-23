using System;
using FluentMigrator;

using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(1, "Initial migration")]
public sealed class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
        CREATE TABLE IF NOT EXISTS Inventory (
            ItemId BIGINT PRIMARY KEY,
            Reserved INT DEFAULT 0,
            Sold INT DEFAULT 0,
            Cancelled INT DEFAULT 0,
            LastUpdated TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        );

         CREATE TABLE IF NOT EXISTS Sales (
                    SellerId BIGINT,
                    ProductId BIGINT,
                    Currency VARCHAR(3),
                    TotalSalesAmount DECIMAL(18,2),
                    TotalUnitsSold INT,
                    LastUpdated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (SellerId, ProductId, Currency)
        );
    ";
}
