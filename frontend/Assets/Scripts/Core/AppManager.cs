using UnityEngine;
using UnityEngine.UI;

/**
 * The central controller that connects the Network and Input layers 
 * without them knowing about each other.
 */
public class AppManager : MonoBehaviour
{
    [SerializeField] private InputHandler inputHandler;

    [SerializeField] private Text uiText;
    
    // use of the Interface here
    private INetworkService networkService;

    private void Start()
    {
        // Get the network service attached to this object (or another one)
        // This is where Dependency Injection happens!
        networkService = GetComponent<INetworkService>();

        if (networkService != null)
        {
            // Subscribe to network events
            networkService.OnConnected += () => Debug.Log("[CORE] Connected to server!");
            networkService.OnMessageReceived += OnNetworkMessageReceived;
            networkService.OnError += (err) => Debug.LogError($"[CORE] Network Error: {err}");

            // Initiate the connection
            Debug.Log("[CORE] Attempting to connect...");
            networkService.Connect("ws://localhost:8080");
        }
        else
        {
            Debug.LogError("[CORE] No INetworkService found! Please attach WebSocketService to this GameObject.");
        }

        if (inputHandler != null)
        {
            inputHandler.OnUserAction += HandleUserAction;
            inputHandler.OnInvalidInput += HandleInvalidInput;
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
                if (uiText != null) uiText.text = command.payload;
                // Enable the input handler to wait for user interaction
                inputHandler.SetInputActive(true);
                break;
            
            case "SHOW_TIMEOUT_MESSAGE":
                Debug.Log($"[CORE] Server says: {command.payload}");
                if (uiText != null) uiText.text = command.payload;
                break;
            
            case "SHOW_GOODBYE_MESSAGE":
                Debug.Log($"[CORE] Server says: {command.payload}");
                if (uiText != null) uiText.text = command.payload;
                break;

            case "SHOW_ACK_MESSAGE":
                Debug.Log($"[CORE] Server says: {command.payload}");
                if (uiText != null) uiText.text = command.payload;
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

        if (uiText != null) uiText.text = "Sending response to server...";
        
        // Create the response object
        ClientResponse response = new ClientResponse
        {
            type = "HANDSHAKE_RESPONSE",
            payload = "Hello Server, I am here !"
        };

        // Serialize to JSON and send it back to the server
        string jsonResponse = JsonUtility.ToJson(response);
        networkService?.SendMessage(jsonResponse);
    }

    /**
     * Triggered when the user presses the wrong key.
     */
    private void HandleInvalidInput(string errorMessage)
    {
        Debug.LogWarning($"[CORE] {errorMessage}");
        if (uiText != null) uiText.text = errorMessage;
    }

    private void OnDestroy()
    {
        // unsubscribe from events 
        if (inputHandler != null)
        {
            inputHandler.OnUserAction -= HandleUserAction;
            inputHandler.OnInvalidInput -= HandleInvalidInput;
        }
    }
}