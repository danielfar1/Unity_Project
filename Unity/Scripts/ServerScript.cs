using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using UnityEngine.SceneManagement;


/// Handles TCP socket communication with the game server.
/// Sends and receives JSON messages to synchronize multiplayer state.
public class ServerScript : MonoBehaviour
{
    public static ServerScript instance;

    public TcpClient client;                  // TCP client to connect to server
    public NetworkStream stream;              // Stream used to read/write data
    private Thread listenThread;              // Background thread to listen for incoming server messages
    private const string serverIP = "10.100.102.15";         // Server IP (primary)
    private const string serverIPLaptop = "10.100.102.241";  // Alternative IP for testing
    private const int serverPort = 5000;

    private bool connected = false;
    public MultiplayerManager1 multi;         // Reference to the multiplayer manager in the scene
    public bool opponentConnected = false;
    private StringBuilder receiveBuffer = new StringBuilder(); // Buffer to accumulate partial socket reads

    // Singleton pattern to keep server script persistent
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Assigns MultiplayerManager1 when scene 7 is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 7)
        {
            multi = FindFirstObjectByType<MultiplayerManager1>();

            if (multi != null)
                Debug.Log("MultiplayerManager1 found and assigned.");
            else
                Debug.LogWarning("MultiplayerManager1 not found in scene 7.");
        }
    }

    
    /// Connects to the game server via TCP.
    public void ConnectToServer()
    {
        if (!connected)
        {
            try
            {
                connected = true;
                client = new TcpClient(serverIP, serverPort);
                stream = client.GetStream();
                Debug.Log("Connected to server.");
            }
            catch (Exception e)
            {
                connected = false;
                Debug.LogError($"Connection error: {e.Message}");
            }
        }
    }

    
    /// Starts the listener thread that handles incoming messages.
    public void StartLisener()
    {
        listenThread = new Thread(ListenForGameServerMessages);
        listenThread.Start();
    }

    
    /// Continuously listens for server messages and handles them accordingly.
    /// Also sends local player state updates back to the server.
    void ListenForGameServerMessages()
    {
        float x, y, yball, xball;
        byte[] buffer = new byte[1024];

        try
        {
            while (MultiplayerManager1.instance.gameIsgoing)
            {
                while (client.Connected)
                {
                    // Handle player "kick" action if flagged
                    if (PlayerControllerMultiplayer.instance.kicked)
                    {
                        Kick();
                        PlayerControllerMultiplayer.instance.kicked = false;
                    }
                    else
                    {
                        // Update the server with current player and ball positions
                        MainThreadDispatcher.Run(() =>
                        {
                            x = multi.rbLeft.position.x;
                            y = multi.rbLeft.position.y;
                            xball = multi.ballMove.position.x;
                            yball = multi.ballMove.position.y;

                            if (multi.firstToConnect)
                            {
                                SendPlayerAction("ball", xball, yball);
                            }

                            SendPlayerAction("position", x, y);
                        });
                    }

                    // Read incoming data
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) continue;

                    receiveBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    // Process complete JSON lines (separated by \n)
                    while (true)
                    {
                        string current = receiveBuffer.ToString();
                        int newlineIndex = current.IndexOf('\n');
                        if (newlineIndex == -1) break;

                        string fullMessage = current.Substring(0, newlineIndex).Trim();
                        receiveBuffer.Remove(0, newlineIndex + 1);

                        if (string.IsNullOrWhiteSpace(fullMessage) || fullMessage == "true" || fullMessage == "false")
                            continue;

                        Debug.Log($"Received: {fullMessage}");

                        ServerMessage serverMessage = JsonUtility.FromJson<ServerMessage>(fullMessage);

                        if (serverMessage.action == "opponent_action")
                        {
                            OpponentActionData action = JsonUtility.FromJson<OpponentActionData>(serverMessage.data);
                            HandleOpponentAction(action);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Listen error: {e.Message}");
        }
    }

    
    /// Applies an action received from the opponent to the local game state.
    public void HandleOpponentAction(OpponentActionData action)
    {
        MainThreadDispatcher.Run(() =>
        {
            switch (action.type)
            {
                case "kick":
                    SecondPlayerControllerMultiplayer.instance.Kick();
                    multi.rbRight.position = new Vector2(-action.x, action.y);
                    break;
                case "position":
                    multi.rbRight.position = new Vector2(-action.x, action.y);
                    break;
                case "ball":
                    multi.ballMove.position = new Vector2(-action.x, action.y);
                    break;
            }
        });
    }

    
    /// Sends a 'kick' action to the server.
    public void Kick()
    {
        if (MultiplayerManager1.instance.isFrozen) return;

        MainThreadDispatcher.Run(() =>
        {
            SendPlayerAction("kick", multi.rbLeft.position.x, multi.rbLeft.position.y);
        });
    }

    
    /// Sends any kind of player action (position, kick, ball) to the server.

    public void SendPlayerAction(string type, float x, float y)
    {
        string data_string = $"{{\"type\":\"{type}\",\"x\":\"{x}\",\"y\":\"{y}\"}}";
        string json = JsonUtility.ToJson(new Request("player_action", data_string)) + "\n";
        byte[] data = Encoding.UTF8.GetBytes(json);
        stream.Write(data, 0, data.Length);
    }

    
    /// Sends a string-based request to the server and waits for a true/false or null response.
    public bool? SendRequest(string action, string dataToSend)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client is not connected to the server.");
            return false;
        }

        try
        {
            string request = JsonUtility.ToJson(new Request(action, dataToSend)) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(request);
            stream.Write(data, 0, data.Length);

            byte[] responseData = new byte[1024];
            int bytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = Encoding.UTF8.GetString(responseData, 0, bytesRead).Trim();

            return response == "true" ? true :
                   response == "false" ? false :
                   (bool?)null;
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending request: " + e.Message);
            return false;
        }
    }

    
    /// Sends a request with a User object and expects a boolean response.
    public bool SendRequest(string action, User dataToSend)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client is not connected to the server.");
            return false;
        }

        try
        {
            string userJson = JsonUtility.ToJson(dataToSend);
            string request = JsonUtility.ToJson(new Request(action, userJson)) + "\n";

            byte[] data = Encoding.UTF8.GetBytes(request);
            stream.Write(data, 0, data.Length);

            byte[] responseData = new byte[1024];
            int bytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);

            return bool.Parse(response);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending request: " + e.Message);
            return false;
        }
    }
}


/// Represents the structure of an action received from the opponent.
public class OpponentActionData
{
    public string type;
    public float x;
    public float y;

    public OpponentActionData(string type)
    {
        this.type = type;
    }

    public OpponentActionData(string type, float x, float y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }
}


/// Represents a request that can be serialized to JSON and sent to the server.
/// </summary>
public class Request
{
    public string action;
    public string data;

    public Request(string action, string data)
    {
        this.action = action;
        this.data = data;
    }
}


/// Represents a message received from the server.
/// </summary>
public class ServerMessage
{
    public string action;
    public string data;
}
