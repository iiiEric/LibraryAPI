📚 Library API – RESTful Web API with ASP.NET Core 9
This project is a RESTful Web API built with ASP.NET Core 9, designed for managing authors, books, and comments in a secure, scalable, and testable way.

✨ Features
📖 Public GET endpoints for retrieving authors, books, and comments

🔐 Secure POST, PUT, PATCH, and DELETE endpoints requiring JWT authentication

🧑‍💼 Only users with the admin role can request tokens

🔄 CORS enabled to allow cross-origin requests (e.g., frontend apps)

🧪 Full unit testing and integration testing suite

Uses NSubstitute for mocking dependencies in unit tests

Covers core logic and API endpoints with real middleware/pipeline behavior

🔧 Built-in dependency injection with correct lifetimes:

Singleton, Scoped, and Transient services depending on usage

🧹 Clean architecture: separation of concerns, DTOs, validation, and structured error responses

🔐 Authentication & Authorization
Token-based authentication with JWT

Admin-only token generation

Role-based access control for secure operations

🧪 Testing
✅ Unit tests: lightweight and fast, mocking services and dependencies using NSubstitute

🔄 Integration tests: verify complete request/response cycles and middleware behavior using WebApplicationFactory

Ensures correctness of:

Controller logic and routing

Auth flows

Validation and error responses

Service-layer interactions

🛠️ Tech Stack
ASP.NET Core 9

C#

xUnit for testing

NSubstitute for mocking

Swagger / OpenAPI for API documentation

CORS, JWT, and custom middleware
