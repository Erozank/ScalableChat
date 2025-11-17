# ScalableChat: A High-Performance, Microservices-Based Chat Application

![Architecture Diagram](/images/Diagram.png)

ScalableChat is a modern, real-time chat application built with a focus on high scalability and performance. It leverages a microservices architecture orchestrated by Aspire to deliver a seamless chat experience capable of handling numerous concurrent users across multiple devices.

## 🚀 Key Features

-   **User Registration & Login**: Securely create an account with a nickname and password.
-   **Friend Management**: Add and manage friends to build your network.
-   **1-on-1 Chatting**: Create private chat rooms with your friends.
-   **Real-Time Messaging**: Send and receive messages instantly with low latency.
-   **Multi-Device Support**: Users can stay connected from multiple devices simultaneously.
-   **High Scalability**: Designed from the ground up to scale horizontally, handling increased load by adding more server instances.

## 🛠 Technology Stack

This project is built using a modern and powerful technology stack:

-   **Orchestration**: .NET Aspire
-   **Frontend**: React
-   **Backend API**: .NET 8
-   **Real-Time Communication**: SignalR
-   **Reverse Proxy**: YARP (Yet Another Reverse Proxy)
-   **Databases**:
    -   **ScyllaDB**: A high-performance NoSQL database for persistent data (users, chats, messages).
    -   **Redis**: In-memory data store for caching and managing user presence.
-   **Message Broker**: Apache Kafka
-   **Containerization**: Docker

## 📈 Architecture & Scalability

The core strength of ScalableChat lies in its distributed architecture, which is designed to scale efficiently. The system is broken down into several independent services that communicate through well-defined interfaces.

### How Scalability is Achieved

1.  **Service Replication**: The SignalR service, which handles real-time communication, is configured with multiple replicas. This allows the system to distribute connection load across several servers.

2.  **User Presence with Redis**: Redis is used as a high-speed cache to manage user presence. It tracks which devices a user is connected from and maps each device connection to a specific SignalR server instance. This allows the system to know exactly where to send messages for any given user, regardless of how many devices they are using.

3.  **Inter-Service Communication with Kafka**: Kafka serves as the message broker for communication between the different SignalR server instances. To ensure message delivery across all instances, each SignalR server has its own dedicated Kafka topic. This decouples the servers and allows them to scale independently.

### Message Flow Example

When User A sends a message to User B, the following process occurs:

1.  The SignalR server receiving the message from User A's device queries Redis to find all active connections for both User A and User B.
2.  The message is then published to the Kafka topics corresponding to the SignalR servers hosting User A's *other* connected devices (to sync the chat).
3.  Simultaneously, the message is also published to the Kafka topics for all SignalR servers hosting User B's connected devices.
4.  Each SignalR server consumes messages from its own topic and forwards them to the correct client connections via SignalR, ensuring the message is delivered to all relevant endpoints in real-time.

This architecture ensures that the system can handle a large number of concurrent users and messages without a single point of failure.

## 📁 Project Structure

The solution is organized into the following main projects:

-   `ScalableChat.AppHost`: The .NET Aspire orchestrator that defines and runs the entire application stack.
-   `ScalableChat.Api`: The RESTful API for user management, friends, and chat data.
-   `ScalableChat.SignalR`: The SignalR hub for real-time communication.
-   `ScalableChat.Ui`: The React frontend application.

## 🏃‍♂️ Getting Started

To run this project locally, you need to have the following prerequisites installed:

-   [.NET 10 SDK](https://dotnet.microsoft.com/es-es/download/dotnet/10.0)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
-   [Node.js](https://nodejs.org/) and npm

### Steps to Run

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/Erozank/ScalableChat.git
    cd ScalableChat
    ```

2.  **Navigate to the AppHost project directory**:
    ```bash
    cd src/ScalableChat.AppHost 
    ```
    *(Note: Adjust the path based on your actual repository structure)*

3.  **Run the Aspire orchestrator**:
    ```bash
    dotnet run
    ```

This command will start the .NET Aspire orchestrator, which will automatically build, configure, and launch all the necessary services (React app, API servers, SignalR servers, Redis, Kafka, ScyllaDB) as defined in the `Program.cs` file. You can monitor the status of all services in the Aspire dashboard, which will open in your browser.

![Aspire Graph](/images/aspire-ScalableChat.png)

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

