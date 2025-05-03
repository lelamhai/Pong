using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    public static NetworkManager Instance { get { return _instance; } }

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;
    private string playerId;

    private Dictionary<string, GameObject> playerCubes = new Dictionary<string, GameObject>();
    private Queue<string> messageQueue = new Queue<string>();
    private object queueLock = new object();

    public string serverIP = "192.168.1.14";
    public int serverPort = 8888;

    private int expectedPlayers = 0;
    private int receivedPlayers = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, serverPort);
            stream = client.GetStream();
            isConnected = true;

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("Connected to server");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    private void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (isConnected)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Disconnect();
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                string[] messages = data.Split('\n');

                lock (queueLock)
                {
                    foreach (string msg in messages)
                    {
                        if (!string.IsNullOrEmpty(msg))
                        {
                            messageQueue.Enqueue(msg);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Receive error: " + e.Message);
                Disconnect();
                break;
            }
        }
    }

    private void Update()
    {
        if (messageQueue.Count > 0)
        {
            lock (queueLock)
            {
                while (messageQueue.Count > 0)
                {
                    string message = messageQueue.Dequeue();
                    ProcessServerMessage(message);
                }
            }
        }
    }

    private void ProcessServerMessage(string message)
    {
        string[] parts = message.Split(':');
        if (parts.Length < 1) return;

        string command = parts[0];

        switch (command)
        {
            case "ID":
                playerId = parts[1];
                Debug.Log("My player ID: " + playerId);
                break;

            case "COUNT":
                expectedPlayers = int.Parse(parts[1]);
                receivedPlayers = 0;
                Debug.Log($"Expecting {expectedPlayers} existing players");
                break;

            case "NEW":
                if (parts.Length >= 5)
                {
                    string id = parts[1];
                    if (id != playerId && !playerCubes.ContainsKey(id))
                    {
                        float x = float.Parse(parts[2]);
                        float y = float.Parse(parts[3]);
                        float z = float.Parse(parts[4]);

                        UnityMainThreadDispatcher.Instance.Enqueue(() =>
                        {
                            CreatePlayerCube(id, new Vector3(x, y, z));
                            receivedPlayers++;
                        });
                    }
                }
                break;

            case "POS":
                if (parts.Length >= 5)
                {
                    string id = parts[1];
                    if (id != playerId && playerCubes.ContainsKey(id))
                    {
                        float x = float.Parse(parts[2]);
                        float y = float.Parse(parts[3]);
                        float z = float.Parse(parts[4]);

                        UnityMainThreadDispatcher.Instance.Enqueue(() =>
                        {
                            playerCubes[id].transform.position = new Vector3(x, y, z);
                        });
                    }
                }
                break;

            case "DEL":
                string playerToRemove = parts[1];
                if (playerCubes.ContainsKey(playerToRemove))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        Destroy(playerCubes[playerToRemove]);
                        playerCubes.Remove(playerToRemove);
                        Debug.Log($"Removed player {playerToRemove}");
                    });
                }
                break;

            case "DONE":
                Debug.Log($"Finished receiving players. Expected: {expectedPlayers}, Received: {receivedPlayers}");
                break;
        }
    }

    public void SendPosition(Vector3 position)
    {
        if (isConnected && stream != null)
        {
            try
            {
                string message = $"POS:{position.x}:{position.y}:{position.z}\n";
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Send error: " + e.Message);
                Disconnect();
            }
        }
    }

    private void CreatePlayerCube(string id, Vector3 position)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;

        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.material.color = new Color(
            UnityEngine.Random.Range(0.5f, 1f),
            UnityEngine.Random.Range(0.5f, 1f),
            UnityEngine.Random.Range(0.5f, 1f)
        );

        playerCubes.Add(id, cube);
        Debug.Log($"Created cube for player {id} at {position}");
    }

    private void Disconnect()
    {
        isConnected = false;
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("Disconnected from server");
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}