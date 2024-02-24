# Cinema Ticket Booking System Web API - An example in Modern Software Web API project

I chanced upon a debate on Reddit concerning a take-home project that many deemed overly ambitious:
- https://www.reddit.com/r/dotnet/comments/1841x0f/does_this_takehome_project_look_okay/

Contrary to the crowd, it piqued my interest as a perfect little project to exhibit a fully working clean Web API project, built with a contemporary Tech Stack and Clean Architectural patterns.

## Tech Stack, Patterns & Dev practices
- **.Net 8**: Use the latest (at the time of creation) .Net framework.
- **C# 12**
- **Web API**
- **Clean Onion Architecture**
- **Docker Containers**
- **Domain Driven Design**
- **Test Driven Development**: With User Stories as the basis for each Domain level Unit Test
- **Test Coverage**: Extensive coverage of the domain business logic via NUnit Tests and Fluent Assertions.
- **Repository Pattern**: Allowing the business and application logic to be database agnostic.
- **Cosmos DB**: Cloud based Document Database chosen for scalability.
  
Wip/Future additions
- **JWT Authentication**
- **CI/CD pipelines**
- **Cloud based hosting**: With load balancing and elastic scaling
- **Rate Limit**
- **.NET Aspire**
- **Redis database caching**


## Contributions?

Engagement is always welcome. 
If you have any feedback or wish to discuss any aspect of this project, please feel free to reach out.
Feel free to submit Issues or Pull Requests with any suggestions.


## Instructions to run the project

$env:AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE = "127.0.0.1"
# Now run your command to start the Cosmos DB Emulator

docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

docker run --publish 8081:8081 --publish 10250-10255:10250-10255 --interactive --tty -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=127.0.0.1 mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest


