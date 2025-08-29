Patents.ArtRepoCloud (.NET 6 Solution)

The Patents.ArtRepoCloud solution is a cloud-native .NET 6–based architecture designed for scalable patent artifact management and processing. It leverages GraphQL to provide a flexible and efficient data access layer, while adopting a modular structure that separates concerns across service interfaces, domain logic, infrastructure, persistence, and testing.

Solution Structure:

Patents.ArtRepoCloud.GraphService – GraphQL service built with HotChocolate, exposing schemas and resolvers for querying and managing patent artifact data, enabling integration with client applications.

Patents.ArtRepoCloud.Service – Background worker service responsible for asynchronous processing, queue handling, and long-running patent data operations.

Patents.ArtRepoCloud.Domain – Core domain layer implementing business entities, aggregates, and domain-driven design (DDD) logic specific to patent artifact management.

Patents.ArtRepoCloud.Infrastructure – Infrastructure layer providing implementations for persistence, Cosmos DB access, messaging, and external integrations.

Deployment & Cloud-Native Setup:
The GraphQL Service and Worker Service are containerized with Docker and deployed to Azure Kubernetes Service (AKS). Data persistence is managed directly in Azure Cosmos DB (document storage), while Azure Queue Storage provides reliable message-based orchestration for scalable background processing.
