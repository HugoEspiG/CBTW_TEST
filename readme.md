# 🌟 Overview
The Library Discovery Engine is a robust backend solution designed to handle messy and inconsistent book search queries. By leveraging Generative AI (Gemini 2.5 Flash Lite) for entity extraction and Dapr Workflows for resilient orchestration, it transforms chaotic user input into a ranked, verified, and grounded list of book results from the Open Library API.

This project follows Clean Architecture principles, ensuring modularity, observability, and scalability.
# 🚀Setup & Installation
1. Prerequisites
Before running the project, ensure you have the following installed:

| Tool                                                                            | Version | Purpose                                   |
| ------------------------------------------------------------------------------- | ------- | ----------------------------------------- |
| [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)              | 8.0+    | Run and build backend services            |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/)               | Latest  | Container runtime for Aspire              |
| [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/)              | 1.14    | Must be init before running the project   |

## Steps
### 1. Clone the repository

```bash
git clone https://github.com/HatchleafEngineering/provider-iq-platform.git
cd src
```
### 2. Initialize Dapr locally with Docker

```bash
dapr init
```
## ⚙️ Configuration & Secrets

### Gemini AI
GEMINI_API_KEY=your_api_key_here

### Dapr Configuration
DAPR_STATE_STORE_NAME=statestore

### Environment
ASPNETCORE_ENVIRONMENT=Development
## 🚀 Running the Project with Aspire

Once all prerequisites are installed and the environment setup is complete, you can start the entire platform using .NET Aspire.

# 🏗️ Architecture Layering
## Library.Api 
 Main entry point. Contains Controllers, Swagger documentation, and modern error-handling middleware using ProblemDetails.
## Library.Core 
 The orchestration hub. Houses Dapr Workflows, Activities, and the core business logic for processing AI-driven requests.
## Library.Domain
 Defines data contracts, AI DTOs, Open Library records, and immutable Domain models using C# Records.
## Library.Services
 Implements external clients (Gemini SDK and Open Library API) with integrated caching strategies via Dapr State Store.

# ⚙️ Discovery Workflow
 The system is powered by the LibraryDiscoveryWorkflow, which orchestrates four critical phases:

 AI Entity Extraction: Gemini parses the "messy blob" to generate a structured search hypothesis (Title, Author, Keywords).

 Tiered Search (Resilience): A smart HTTP client queries Open Library using a tiered fallback strategy (Exact -> Title-Only -> General Query) to maximize result retrieval.

 AI Ranking & Grounding: Gemini evaluates real API results against the initial hypothesis, applying a strict weighting hierarchy (Primary Author vs. Contributor).

 Explainability: The AI generates a factual, grounded explanation for why each book was selected and how it matches the user's intent.

# 🧠 AI Strategy & Hierarchy
 The engine utilizes Gemini 2.5 Flash Lite with Temperature 0.1 and JSON MimeType to ensure deterministic and accurate outputs. The ranking logic follows this strict hierarchy:

 - Weight 1 (Exact Match): Exact Title + Primary Author match.

 - Weight 2 (Contributor Match): Title match where the requested author is found as a contributor (illustrator/editor).

 - Weight 3 (Partial Match): Fuzzy title matches or cases where the title matches but the author differs.

 - Weight 4 (Author Fallback): Top works by the requested author when the specific title is not found.

# 🌉 Dapr Integration
Workflow Orchestration: Manages state and automatic retries if external APIs (Open Library or Gemini) experience transient failures.

State Store (Caching): Implements distributed caching for Open Library searches to reduce latency and API consumption costs.
# 🧪 Testing Strategy
The project includes a dedicated Unit Testing suite to ensure data integrity and the reliability of core logic, especially regarding AI-generated data and caching mechanisms.

Key Test Scenarios:
Cache Key Normalization: Ensures that search terms are consistently transformed (lowercase, space-to-underscore) to prevent cache misses in the Dapr State Store.

Data Integrity (DTOs): Validates that the extraction hypothesis and ranking results maintain their structure throughout the workflow.

Tiered Logic Readiness: Ensures the system can handle empty or ambiguous inputs by correctly identifying search tiers.

Why these tests?
Instead of chasing a high coverage percentage, the testing strategy focuses on Business Logic Stability. By testing the normalization of search keys, we guarantee that the integration with the Dapr State Store remains consistent even if the input source changes.