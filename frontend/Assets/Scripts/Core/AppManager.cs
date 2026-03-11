using UnityEngine;

/**
 * The central controller that connects the Network and Input layers 
 * without them knowing about each other.
 */
public class AppManager : MonoBehaviour
{
    [SerializeField] private InputHandler inputHandler;
    
    // We use the Interface here, not a concrete WebSocket class!
    // This makes it 100% loosely coupled.
    private INetworkService networkService;

    private void Start()
    {
        // For now, we don't have the real WebSocket implementation, 
        // so this variable will be null. We will assign it in the next step.
        // networkService = new WebSocketNetworkClient(); 

        if (inputHandler != null)
        {
            // Subscribe to the input event
            inputHandler.OnUserAction += HandleUserAction;
        }
    }

    /**
     * Called when a message arrives from the server.
     */
    private void OnNetworkMessageReceived(string rawJson)
    {
        // Parse the incoming command
        ServerCommand command = JsonUtility.FromJson<ServerCommand>(rawJson);

        if (command != null && command.type == "COMMAND")
        {
            ExecuteCommand(command);
        }
    }

    /**
     * Executes the specific command received from the server. 
     */
    private void ExecuteCommand(ServerCommand command)
    {
        switch (command.action)
        {
            case "SHOW_WELCOME_MESSAGE":
                Debug.Log($"[CORE] Server says: {command.payload}");
                // Enable the input handler to wait for user interaction
                inputHandler.SetInputActive(true);
                break;
            default:
                Debug.LogWarning($"[CORE] Unknown action: {command.action}");
                break;
        }
    }

    /**
     * Triggered by the InputHandler's event.
     */
   private void HandleUserAction()
    {
        Debug.Log("[CORE] User interaction detected. Sending response to server...");
        
        // Create the response object
        ClientResponse response = new ClientResponse
        {
            type = "HANDSHAKE_RESPONSE",
            payload = "Hello Server, I am here and clicked!"
        };

        // Serialize to JSON and send it back to the server
        string jsonResponse = JsonUtility.ToJson(response);
        networkService?.SendMessage(jsonResponse);
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events to prevent memory leaks!
        if (inputHandler != null)
        {
            inputHandler.OnUserAction -= HandleUserAction;
        }
    }
}