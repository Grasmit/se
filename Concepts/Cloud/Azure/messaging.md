<!-- Prev: observability.md | Next: architecture.md -->

# Messaging & Events — Service Bus, Event Grid, Event Hubs

When to use what

- Azure Service Bus: reliable enterprise messaging, queues and topics (pub/sub), exactly-once/ordered semantics.
- Event Grid: lightweight event delivery and routing, best for reactive event-driven architectures.
- Event Hubs: high-throughput telemetry and streaming ingestion for analytics.

Integration patterns

- Use Service Bus topics for domain events (e.g., OrderCreated) to decouple services.
- Use Event Grid for resource-level events and routing to serverless handlers.
- Use Event Hubs when ingesting large streams (telemetry, clickstreams) into Databricks or Stream Analytics.

ASP.NET integration

- For Service Bus, use the Azure.Messaging.ServiceBus client, or Azure Functions triggers for processing.
- For Event Grid, send events via SDK or HTTP and subscribe with WebHooks, Functions, or Logic Apps.

Design guidance

- Model business events explicitly; version events when necessary.
- Add dead-lettering and retries for resilient processing.

Links

- Prev: [Observability & Monitoring](observability.md)
- Next: [Architecture Diagrams & Flows](architecture.md)
