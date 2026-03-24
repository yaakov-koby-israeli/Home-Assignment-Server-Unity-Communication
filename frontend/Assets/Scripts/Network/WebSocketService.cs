using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/**
 * Concrete implementation of INetworkService using native WebSockets.
 * Handles background threading safely for Unity.
 */
public class WebSocketService : MonoBehaviour, INetworkService
{
    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action<string> OnError;

    private ClientWebSocket websocket;
    private CancellationTokenSource cancellationTokenSource;
    
    // Thread-safe queues to pass data from background thread to Unity's Main Thread
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    // here we save a real code snipp in the queue (queue work in fifo)
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    private void Update()
    {
        // Process network events on the Unity Main Thread
        // try to see if we have msg from the server and if we have we invoke the event to notify the app manager
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }

        // Process incoming messages on the Unity Main Thread
        while (messageQueue.TryDequeue(out string message))
        {
            OnMessageReceived?.Invoke(message);
        }
    }

    public async void Connect(string url)
    {
        websocket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            // Here we (unity) connect to the server in a background thread
            Uri serverUri = new Uri(url);
            await websocket.ConnectAsync(serverUri, cancellationTokenSource.Token);
            
            mainThreadActions.Enqueue(() => OnConnected?.Invoke());
            
            // Start listening for messages from the server in a background thread
            _ = ReceiveLoop(); 
        }
        catch (Exception ex)
        {
            mainThreadActions.Enqueue(() => OnError?.Invoke(ex.Message));
        }
    }

    public async void SendMessage(string message)
    {
        if (websocket == null || websocket.State != WebSocketState.Open) return;

        byte[] buffer = Encoding.UTF8.GetBytes(message);
        ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

        try
        {
            await websocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            mainThreadActions.Enqueue(() => OnError?.Invoke(ex.Message));
        }
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[1024];

        while (websocket.State == WebSocketState.Open)
        {
            try
            {
                // Here we get the message from the server in a background threadr
                WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Disconnect();
                    break;
                }

                // dcode the msg from bytes tostring
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //we enqueue it to be processed on the main thread
                messageQueue.Enqueue(message); // Safely pass to main thread
            }
            catch (Exception)
            {
                break; // Exit loop on error or disconnect
            }
        }
    }

    public void Disconnect()
    {
        if (websocket != null)
        {
            cancellationTokenSource?.Cancel();
            websocket.Abort();
            websocket.Dispose();
            websocket = null;
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}