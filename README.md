# Booking System Web API - An example Web API project

## Tech Stack, Patterns & Dev practices
- **.Net 8**: Uses the latest (at the time of creation) .Net framework.
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

## Why?

I chanced upon a debate on Reddit concerning a take-home project that many deemed overly ambitious:
- https://www.reddit.com/r/dotnet/comments/1841x0f/does_this_takehome_project_look_okay/

![image](https://github.com/user-attachments/assets/5a33b9d5-42c6-4ef7-8ee4-2a83ee484737)

Contrary to the crowd, it piqued my interest as a perfect little project to exhibit a fully working clean Web API project, built with a contemporary Tech Stack and Clean Architectural patterns.

## Implementation
This is the class structure designed to implement the solution for the challange
![image](https://github.com/user-attachments/assets/db26be4a-a4d5-4203-a6b8-efc17ae6bb76)

## APIs End points
This is the list of APIs providing the solution
![image](https://github.com/user-attachments/assets/26efe814-f555-4999-b60e-41ac6b214c30)

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

From the Aspire page follow the URL to get to the Open API documentation
![image](https://github.com/user-attachments/assets/bdd4ae4c-4ffc-4499-bce4-aa1c1f3e7876)

From here you eill be able to see and access the complete suite of API end points



# Example requests & responses
Example requests & responses can be seen by running the [APITesterApp](https://github.com/MrCull/Web-API-Example-Booking-System/tree/main/APITesterApp) project which performs these actions which covers setting up theater chains, managing movies, theaters, showtimes, and bookings.

## 1. HTTP Client Configuration

- **Initialize HTTP Client**: Configures an HTTP client with an HTTP logging handler to capture and debug HTTP requests and responses. The API base address is set, and the timeout is established.

## 2. Theater Chain Management

### Create a Theater Chain
- **POST Request**: Sends a POST request to create a new theater chain named "Odeon".

## 3. Movie Management

### Add a New Movie
- **POST Request**: Adds "The Matrix" to the "Odeon" theater chain via a POST request.

### Update a Movie
- **PUT Request**: Updates the movie details if the initial add was successful via a PUT request.

## 4. Theater and Screen Setup

### Create a Theater
- **POST Request**: Attempts to add a new theater "Loughborough Max" to the chain via a POST request.

### Add Screen and Seats
- **POST Request**: Configures a screen with predefined seats in the new theater via a POST request.

## 5. Showtime Management

### Create a Showtime
- **POST Request**: Schedules a new showtime for "The Matrix" at the specified screen via a POST request.

## 6. Reservation and Booking Process

### Make a Reservation
- **POST Request**: Reserves specific seats for a showtime via a POST request.

### Confirm a Booking
- **PUT Request**: Sends a PUT request to confirm the reservation and complete the booking process.



# Contributions?
Engagement is  welcome. 
If you have any feedback or wish to discuss any aspect of this project, please feel free to reach out.
Feel free to submit Issues or Pull Requests with any suggestions.

