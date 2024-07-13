# Cinema Ticket Booking System Web API - An example Web API project

I chanced upon a debate on Reddit concerning a take-home project that many deemed overly ambitious:
- https://www.reddit.com/r/dotnet/comments/1841x0f/does_this_takehome_project_look_okay/

![image](https://github.com/user-attachments/assets/5a33b9d5-42c6-4ef7-8ee4-2a83ee484737)

Contrary to the crowd, it piqued my interest as a perfect little project to exhibit a fully working clean Web API project, built with a contemporary Tech Stack and Clean Architectural patterns.

## Tech Stack, Patterns & Dev practices
- **.Net 8**: Use the latest (at the time of creation) .Net framework.
- **C# 12**:
- **Web API**: RESTful and using Minimal API for enhances performance.
- **Clean Onion Architecture**
- **Docker Containers**
- **Domain Driven Design**
- **Test Driven Development**: With User Stories as the basis for each Domain level Unit Test.
- **Test Coverage**: Extensive coverage of the domain business logic via NUnit Tests and Fluent Assertions.
- **Repository Pattern**: Allowing the business and application logic to be database agnostic.
- **Cosmos DB**: Cloud based Document Database chosen for scalability.
- **Open API**: Auto API documentation (formally called Swagger).
- **Redis caching**: Fase in-memory caching.
- **.NET Aspire**: An opinionated, cloud ready stack for building observable, production ready, distributed applications. 
  
Wip/Future additions
- **JWT Authentication**
- **CI/CD pipelines**
- **Cloud based hosting**: With load balancing and elastic scaling.
- **Rate Limit**


## Contributions?

Engagement is always welcome. 
If you have any feedback or wish to discuss any aspect of this project, please feel free to reach out.
Feel free to submit Issues or Pull Requests with any suggestions.


# Instructions to run the project

Set Startup Project to be AsipreApp.AppHost
Run the solution.

In the Aspire Window click on "cosmos -  Details" 
Then take the port number from "emulator target port":
![image](https://github.com/user-attachments/assets/629201ae-a1fe-4c54-b517-a43c4f2d96da)

Now navigate to this url where [portnumber] is replaced with the port from above: https://localhost:[portnumber]/_explorer/index.html
Note: CosmodDB emulator usually takes a few mins to start up. So retry untill you see the "Azure Cosmos DB Emulator" page.

There will be a certification error form the selfsigned certificate.
Export the Cosmos DB certificate and save to a local file.
Then open crt file and install to your Trusted Root Certificate Authorities folder "on this pc".

# API Endpoints

From the Aspire page follow the URL to get to the Open API documentation
![image](https://github.com/user-attachments/assets/bdd4ae4c-4ffc-4499-bce4-aa1c1f3e7876)

From here you eill be able to see and access the complete suite of API end points:
![image](https://github.com/user-attachments/assets/26efe814-f555-4999-b60e-41ac6b214c30)



