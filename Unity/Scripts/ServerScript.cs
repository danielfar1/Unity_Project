using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public class ServerScript : MonoBehaviour
{
    public static ServerScript instance;
    public TcpClient client;
    public NetworkStream stream;
    private Thread listenThread;
    private const string serverIP = "10.100.102.15"; 
    private const string serverIPLaptop = "10.100.102.241"; 
    private const int serverPort = 5000;
    private bool connected = false;
    public MultiplayerManager1 multi;
    public bool opponentConnected = false;
    private StringBuilder receiveBuffer = new StringBuilder();

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
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 7)
        {
            multi = FindFirstObjectByType<MultiplayerManager1>();

            if (multi != null)
            {
                Debug.Log("MultiplayerManager1 found and assigned.");
            }
            else
            {
                Debug.LogWarning("MultiplayerManager1 not found in scene 7.");
            }
        }
    }
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
    public void StartLisener()
    {

        listenThread = new Thread(ListenForGameServerMessages);
        listenThread.Start();
    }
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
                    if (PlayerControllerMultiplayer.instance.kicked == true)
                    {
                        Kick();
                        PlayerControllerMultiplayer.instance.kicked = false;
                    }
                    else
                    {
                        MainThreadDispatcher.Run(() =>
                        {
                            x = multi.rbLeft.position.x;
                            y = multi.rbLeft.position.y;
                            xball = multi.ballMove.position.x;
                            yball = multi.ballMove.position.y;
                            if(multi.firstToConnect == true)
                            {
                                Debug.Log("not you");
                                SendPlayerAction("ball", xball, yball);
                            }
                            SendPlayerAction("position", x, y);
                            
                        });
                    }
                    // ????? ?????: ?? 1024 ???? ???? ?????
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    // ?? ?? ?????? ?????? – ?????? ?? ?????
                    if (bytesRead <= 0)
                        continue;

                    // ???? ??????? ????? ???? ??????? ?????? ????? ?????
                    receiveBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    // ????? ??????? ??? ?? ??? ???? '\n'
                    while (true)
                    {
                        string current = receiveBuffer.ToString();
                        int newlineIndex = current.IndexOf('\n');

                        // ?? ??? ?? ??? ???? – ????? ???? ??????
                        if (newlineIndex == -1)
                            break;

                        // ????? ????? ??? ???? ???? ?????
                        string fullMessage = current.Substring(0, newlineIndex).Trim();
                        Debug.Log(newlineIndex + " - " + current + " current buffer");
                        Debug.Log(fullMessage + " fullMessage");

                        // ???? ?????? ??????
                        receiveBuffer.Remove(0, newlineIndex + 1);

                        // ????? ?? ?????? ???????? ?????? ??? true/false
                        if (fullMessage == "true" || fullMessage == "false")
                            continue;

                        // ?? ??????? ???? ??? ???? – ???????
                        if (!string.IsNullOrWhiteSpace(fullMessage))
                        {
                            Debug.Log($"Received: {fullMessage}");

                            // ????? JSON ?????? ????? ?????
                            ServerMessage serverMessage = JsonUtility.FromJson<ServerMessage>(fullMessage);

                            // ?? ?????? ??? ?? ???? – ?????? ????? ????????
                            if (serverMessage.action == "opponent_action")
                            {
                                OpponentActionData action = JsonUtility.FromJson<OpponentActionData>(serverMessage.data);
                                HandleOpponentAction(action);
                            }
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

        public void HandleOpponentAction(OpponentActionData action)
    {
        if (action.type == "kick")
        {
            MainThreadDispatcher.Run(() =>
            {
                SecondPlayerControllerMultiplayer.instance.Kick();
                multi.rbRight.position = new Vector2(-action.x, action.y);
            });
        }
        if(action.type == "position")
        {
            MainThreadDispatcher.Run(() => {
                multi.rbRight.position = new Vector2(-action.x, action.y);
            });
        }
        if(action.type == "ball")
        {
            MainThreadDispatcher.Run(() => {
                multi.ballMove.position = new Vector2(-action.x, action.y);
            });
        }    
    }
    public void Kick()
    {
        if (MultiplayerManager1.instance.isFrozen)
            return;
        MainThreadDispatcher.Run(() => {
            SendPlayerAction("kick", multi.rbLeft.position.x, multi.rbLeft.position.y);
        });
    }
    
    public void SendPlayerAction(string type,float x,float y)
    {

        string data_string = $"{{\"type\":\"{type}\",\"x\":\"{x}\",\"y\":\"{y}\"}}";
        string json = JsonUtility.ToJson(new Request("player_action",data_string)) + "\n";
        byte[] data = Encoding.UTF8.GetBytes(json);
        stream.Write(data, 0, data.Length);
    }

    
    public bool? SendRequest(string action, string dataToSend)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client is not connected to the server.");
            return false;
        }
        try
        {
            Debug.Log(action);
            // Create the request
            string request = JsonUtility.ToJson(new Request(action, dataToSend))+ "\n";
            Debug.Log("request " + request);

            // Send the request to the server
            byte[] data = Encoding.UTF8.GetBytes(request);
            stream.Write(data, 0, data.Length);

            // Receive the response from the server
            byte[] responseData = new byte[1024];
            int bytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);
            response = response.Trim();
            if(response != "true" && response != "false")
                return null;
            else
                return bool.Parse(response);
        }

        catch (Exception e)
        {
            Debug.LogError("Error sending request: " + e.Message);
            return false;
        }

    }
    public bool SendRequest(string action, User dataToSend)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client is not connected to the server.");
            return false;
        }
        try
        {
            // Create the request
            string userJson = JsonUtility.ToJson(dataToSend);
            string request = JsonUtility.ToJson(new Request(action, userJson))+ "\n";

            // Send the request to the server
            byte[] data = Encoding.UTF8.GetBytes(request);
            stream.Write(data, 0, data.Length);

            // Receive the response from the server
            byte[] responseData = new byte[1024];
            int bytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);
            bool result = bool.Parse(response);
            return result;
        }

        catch (Exception e)
        {
            Debug.LogError("Error sending request: " + e.Message);
            return false;
        }
    }
    


}
public class OpponentActionData
{
    public string type;
    public float x;
    public float y;
    public OpponentActionData(string type)
    {
        this.type = type;
    }
    public OpponentActionData(string type, float x,float y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }
}
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
public class ServerMessage
{
    public string action;
    public string data;
}
