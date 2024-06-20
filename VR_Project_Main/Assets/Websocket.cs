using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

[Serializable]
public class Data
{
    public string Platform;
    public string Type;
    public string Message;
    public int ID;
}

[Serializable]
public class PositionData : Data
{
    public float x;
    public float y;
    public float z;
}

public class Websocket : MonoBehaviour
{
    private ClientWebSocket ws;
    private int clientID = 1; // Uniek ID voor de client, wijzig dit indien nodig
    private string platform = "Vr headset"; // Platforminformatie
    public GameObject cube;
    private GameObject spawnedCube;
    [SerializeField] private AudioSource _audioSource;

    async void Start()
    {
        ws = new ClientWebSocket();

        // Verbinding maken met de websocket-server
        await ws.ConnectAsync(new Uri("ws://localhost:8000"), CancellationToken.None);

        // Registreer de client bij de server
        RegisterClient();

        // Start een ontvangende loop
        ReceiveLoop();

        // Testberichten verzenden
        SendMessage(platform, "aangekomen", "Wij hebben uw aanvraag correct ontvangen");
    }

    async void RegisterClient()
    {
        var data = new Data
        {
            Platform = platform,
            Type = "REGISTER",
            Message = "Registratie",
            ID = clientID
        };

        // Convert the Data object to a JSON string
        string jsonString = JsonUtility.ToJson(data);

        // Convert the JSON string to a byte array
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);

        // Send the byte array via WebSocket
        await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    async void ReceiveLoop()
    {
        byte[] buffer = new byte[1024 * 1024];
        while (ws.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("Bericht ontvangen: " + message);

                // Deserialize the JSON string to a Data object
                PositionData receivedData = JsonUtility.FromJson<PositionData>(message);

                // Handle the message based on the Type
                switch (receivedData.Type)
                {
                    case "SPAWN":
                        HandleSpawn(receivedData);
                        break;
                    case "MOVE":
                        HandleMove(receivedData);
                        break;
                    case "DESTROY":
                        HandleDestroy(receivedData);
                        break;
                    case "INTERCOM":
                        HandleIntercom(message);
                        break;
                    default:
                        Debug.LogWarning("Onbekend berichttype ontvangen: " + receivedData.Type);
                        break;
                }
            }
        }
    }

    private void HandleIntercom(string receivedData)
    {
        // Deserialize the JSON string to a Data object
        Data serializedData = JsonUtility.FromJson<Data>(receivedData);
        string message = serializedData.Message;
        Debug.Log(message);

        StartCoroutine(ConvertBase64ToAudioClip(message, _audioSource));
    }

    IEnumerator ConvertBase64ToAudioClip(string base64EncodedString, AudioSource audioSource)
    {
        Debug.Log("Converting base64 to audio");
        var audioBytes = Convert.FromBase64String(base64EncodedString);
        var tempMp3Path = Path.Combine(Application.persistentDataPath, "tmpAudio.mp3");
        var tempWavPath = Path.Combine(Application.persistentDataPath, "tmpAudio.wav");

        File.WriteAllBytes(tempMp3Path, audioBytes);
        Debug.Log("Audio file saved at: " + tempMp3Path);
        File.WriteAllBytes(tempWavPath, audioBytes);
        Debug.Log("Audio file saved at: " + tempWavPath);


        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + tempWavPath, AudioType.WAV);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading audio clip: " + request.error);
        }
        else
        {
            Debug.Log("Playing audio");
            audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
            audioSource.Play();
        }

        // Optionally delete the temporary files
        File.Delete(tempMp3Path);
        File.Delete(tempWavPath);
    }



    void HandleSpawn(PositionData data)
    {
        Debug.Log("Spawning object: " + data.Message);
        switch (data.Message)
        {
            case "Cube":
                spawnedCube = Instantiate(cube, new Vector3(data.x, data.y, data.z), Quaternion.identity);
                break;
            default:
                break;
        }
    }

    void HandleMove(PositionData data)
    {
        Debug.Log("Moving object: " + data.Message);
        if (spawnedCube != null)
        {
            spawnedCube.transform.position = new Vector3(data.x, data.y, data.z);
        }
    }

    void HandleDestroy(PositionData data)
    {
        Debug.Log("Destroying object: " + data.Message);
        if (spawnedCube != null)
        {
            Destroy(spawnedCube);
            spawnedCube = null;
        }
    }

    async void SendMessage(string platform, string type, string message)
    {
        type = type.ToUpper();
        var data = new PositionData
        {
            Platform = platform,
            Type = type,
            Message = message,
            ID = clientID
        };

        string jsonString = JsonUtility.ToJson(data);
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
        await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
        }
    }
}
