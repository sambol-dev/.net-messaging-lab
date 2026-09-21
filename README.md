# .NET Messaging Lab

Laboratório prático de mensageria assíncrona com .NET 10 e RabbitMQ.

O objetivo do projeto é estudar, de forma prática, padrões e mecanismos comuns em sistemas distribuídos baseados em mensagens.

## Stack

- .NET 10
- C# 
- RabbitMQ 4.x
- RabbitMQ.Client 7.2.1
- Entity Framework Core 10
- SQLite
- xUnit
- Debian Linux

## Arquitetura

```text
                    ┌──────────────────┐
                    │  Messaging.Api   │
                    │    Publisher     │
                    └────────┬─────────┘
                             │
                             ▼
                    orders.exchange
                             │
                             ▼
                    orders.created
                             │
                             ▼
                    ┌──────────────────┐
                    │ OrderCreated     │
                    │    Consumer      │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │    Processor     │
                    └────────┬─────────┘
                             │
                    ┌────────┴────────┐
                    ▼                 ▼
               Idempotency         Handler
                 SQLite