# Business Tasks and Technical Solutions

## 1. Room Management

### Business task
Allow administrators to create, view, update, list, and remove rooms. Each room has a name, capacity, base price, and a set of available services.

### Technical solution
Room management is implemented as an application use case over the `Room` domain entity and `IRoomRepository`.

- Domain rules validate room data.
- Application handlers coordinate domain operations and persistence.
- Repository implementations handle storage.
- REST endpoints expose CRUD operations.
- `Result` objects are used to represent expected application failures such as not-found and conflict cases.

## 2. Search for Available Rooms

### Business task
Allow customers to find rooms that:
- have sufficient capacity;
- are not already booked during the requested period.

### Technical solution
`SearchAvailableRoomsHandler` obtains rooms and overlapping bookings from their respective repositories, filters rooms by capacity, removes rooms with conflicting bookings, and returns `RoomInfo` DTOs.

## 3. Room Booking

### Business task
Allow customers to book a room for a specified period and optionally request services.

A booking must:
- reference an existing room;
- contain only services available in that room;
- satisfy booking domain rules;
- satisfy configured business policies;
- not overlap an existing booking.

### Technical solution
`BookRoomsHandler` coordinates the booking process:

1. Find the requested room.
2. Validate requested services.
3. Create the `Booking` domain entity.
4. Apply all registered `IBookingPolicy` implementations.
5. Check for conflicting bookings.
6. Calculate the price using registered `IPricingPolicy` implementations.
7. Persist the booking.
8. Return booking confirmation information.

This keeps business rules in the domain and allows booking and pricing policies to be added independently.

## 4. Dynamic Booking Pricing

### Business task
Calculate booking prices according to the time of day and requested additional services.

### Technical solution
Pricing is separated from the booking handler through the `IPricingPolicy` abstraction. Individual pricing policies calculate their part of the price, while the booking handler combines their results.

This makes pricing rules replaceable and allows additional pricing policies to be introduced without modifying the booking workflow.

## 5. Business Reports

### Business task
Provide information useful for business decisions, rather than exposing only raw booking data.

### Technical solution
The application provides reporting use cases based on existing rooms and bookings.

Example reports include:

- **Room utilization** — shows how much of the available time each room is booked.
- **Room revenue** — shows how much money was earned with each room.

Reports are implemented as application-level queries that aggregate domain data into dedicated report DTOs.

## 6. Error Handling

### Business task
Return meaningful responses when an operation cannot be completed.

### Technical solution
Expected failures are represented using the application's `Result` abstraction and typed errors such as:

- `NotFoundError`
- `ConflictError`
- `DomainRulesViolationError`
- `BusinessRulesViolationError`

The API layer translates these application errors into appropriate HTTP responses and `ProblemDetails`.

Unexpected infrastructure failures are allowed to propagate and can be handled by global exception handling/middleware.

---

# Architecture

The application follows a layered architecture described in Uncle Bob's book "Clean architecture" with clean responsibility separation:

 - `Domain` layer - domain entities with constant rules.
 - `Application` layer - application business logic and use cases used by users.
 - `Infrastructure` layer - implementations of different adapters (repositories etc).
 - `Api` layer - endpoints, configurations.  

---

# Code quality

Code base was developped according to "Clean code" principles:
 
 - Meaningful variable, function, class names.
 - Code is well documented.
 - No god objects.