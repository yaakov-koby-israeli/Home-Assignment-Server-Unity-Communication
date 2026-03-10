using System;

/**
 * This interface defines the contract for any network service.
 * By using an interface, the rest of the game doesn't need to know 
 * if we are using WebSockets, HTTP, or a local mock. 
 * This ensures strict Loose Coupling.
 */
public interface INetworkService
{
    // Event triggered when a raw message is received from the server
    event Action<string> OnMessageReceived;

    // Event triggered when the connection is successfully established
    event Action OnConnected;

    // Event triggered when the connection drops or fails
    event Action<string> OnError;

    // Core network actions
    void Connect(string url);
    void SendMessage(string message);
    void Disconnect();
}