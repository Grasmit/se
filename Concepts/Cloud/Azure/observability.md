<!-- Prev: hosting.md | Next: messaging.md -->

# Observability & Monitoring — Application Insights and Log Analytics

Goals

- Understand application behavior, performance bottlenecks and failures.
- Provide actionable telemetry for developers and SREs.

Core services

- Application Insights: traces, exceptions, request/response telemetry, dependency tracking and performance profiling.
- Log Analytics (part of Azure Monitor): centralized queries and dashboards across resources.

Integration tips for ASP.NET

- Add the Application Insights SDK and enable automatic collection for requests, exceptions and dependencies.
- Use `ILogger` to record structured logs; they map to AI traces and to Log Analytics when forwarded.
- Configure sampling early to control costs for high-throughput workloads.

Useful queries and alerts

- Create alerts on high failure rate, increased response time, or spikes in dependency failures.
- Build dashboards for error budget, latency percentiles, and database call durations.

Links

- Prev: [Hosting & Databases](hosting.md)
- Next: [Messaging & Events](messaging.md)
