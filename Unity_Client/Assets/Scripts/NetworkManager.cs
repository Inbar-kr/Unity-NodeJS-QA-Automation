using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/**
 * Data model for the JSON commands sent by the server.
 * [Serializable] allows Unity's JsonUtility to map JSON keys to class fields.
 */
[Serializable]
public class ServerCommand
{
    public string id;
    public string action;
}

public class NetworkManager : MonoBehaviour
{
    [Header("Server Connection")]
    [SerializeField] private string serverUrl = "ws://127.0.0.1:8080/";

    private ClientWebSocket ws;
    public LogManager logManager;

    [Header("Ad Server Crystals")] 
    public GameObject crystalBlue;   
    public GameObject crystalGreen;  
    public GameObject crystalRed;    
    public GameObject crystalYellow; 
    public GameObject crystalPurple; 

    /**
     * Sends a raw string message to the server asynchronously.
     * @param message - The string to be sent (an acknowledgment).
     */
    public async Task SendMessageToServer(string message)
    {
        if (ws.State != WebSocketState.Open) return;
        
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    async void Start() 
    {
        // Safety check: The Dispatcher must exist to handle UI updates from the background thread
        if (UnityMainThreadDispatcher.Instance() == null) return;

        ActivateCrystal(crystalBlue); // Initial state
        ws = new ClientWebSocket();
        
        try 
        {
            await ws.ConnectAsync(new Uri(serverUrl), CancellationToken.None);
            
            await SendMessageToServer("Unity_Ready");

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (logManager != null) logManager.WriteLog("System: Connected & Handshake Sent!");
            });

            ReceiveLoop();
        }
        catch (Exception ex) 
        { 
            Debug.LogError($"Connection Error: {ex.Message}"); 
        }
    }

    /**
     * Continuously listens for incoming data from the server.
     * Runs as a background task to prevent freezing the game.
     */
    private async void ReceiveLoop()
    {
        var buffer = new ArraySegment<byte>(new byte[2048]); // Buffer for incoming packets
        
        while (ws.State == WebSocketState.Open)
        {
            try 
            {
                // Wait for a message from the server
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None);
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(() => ProcessCommandSimple(message));
                }
            }
            catch (Exception ex) 
            { 
                Debug.LogWarning($"Receive Error: {ex.Message}"); 
                break; 
            }
        }
    }

    /**
     * Parses the JSON message and triggers the corresponding visual/logical action.
     * @param rawMessage - The JSON string received from the server.
     */
    void ProcessCommandSimple(string rawMessage) 
    {
        try 
        {
            ServerCommand cmd = JsonUtility.FromJson<ServerCommand>(rawMessage);
            if (logManager != null) logManager.WriteLog($"CMD: {cmd.action}");

            switch (cmd.action) 
            {
                case "Ad Delivered": ActivateCrystal(crystalGreen); break;
                case "Timeout Error": ActivateCrystal(crystalRed); break;
                case "Test Mode": ActivateCrystal(crystalPurple); break;
                case "High Latency": ActivateCrystal(crystalYellow); break;
                default: ActivateCrystal(crystalBlue); break;
            }

            _ = SendMessageToServer($"Action Executed: {cmd.action}");
        }
        catch (Exception ex) 
        { 
            Debug.LogError($"Parse Error: {ex.Message}"); 
        }
    }

    void ActivateCrystal(GameObject target)
    {
        GameObject[] crystals = { crystalBlue, crystalGreen, crystalRed, crystalYellow, crystalPurple };
        foreach (var c in crystals) 
        {
            if (c != null) c.SetActive(c == target);
        }
    }

    /**
    * Triggers a new test sequence by re-sending the handshake message.
    * This is called by the UI button.
    */
    public async void RequestNewSequence()
    {
        if (ws != null && ws.State == System.Net.WebSockets.WebSocketState.Open)
        {
            await SendMessageToServer("Unity_Ready");
            if (logManager != null) 
            {
                logManager.WriteLog("<color=cyan>System: New sequence requested...</color>");
            }
        }
        else
        {
            if (logManager != null) logManager.WriteLog("<color=red>Error: Not connected to server!</color>");
        }
    }

    private async void OnDestroy() 
    { 
        if (ws != null && ws.State == WebSocketState.Open) 
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Destroyed", CancellationToken.None);
            ws.Dispose();
        }
    }
}