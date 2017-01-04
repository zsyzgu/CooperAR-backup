using UnityEngine;
using System.IO;
using System;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
#else
using System.Net.Sockets;
using System.Threading;
#endif

public class CaptureSimulator : MonoBehaviour {
    const int PORT = 8888;

    public int captureID = 0;
    private WebCamTexture webCamTexture;
    private byte[] imageData;
    private string serverIP;
    private bool confirmed;

    void Start() {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
#if WINDOWS_UWP
#else
        serverIP = Network.player.ipAddress;
#endif
    }

    void Update() {
        Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);
        texture.SetPixels(webCamTexture.GetPixels());
        imageData = texture.EncodeToJPG();
        Destroy(texture);
        
    }

    void OnGUI() {
        if (!confirmed) {
            serverIP = GUI.TextArea(new Rect(0, 0, 300, 100), serverIP);
            if (GUI.Button(new Rect(300, 0, 100, 100), "confirm")) {
                confirmed = true;
            }
        }
    }

#if WINDOWS_UWP
    private Task mainTask;

    void Awake() {
        mainTask = new Task(run);
        mainTask.Start();
    }

    void OnApplicationQuit() {
        mainTask = null;
    }

    private async void run() {
        while (!confirmed) {
            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }

        StreamSocket socket = new StreamSocket();
        await socket.ConnectAsync(new HostName(serverIP), "" + PORT);
        Stream stream = socket.InputStream.AsStreamForRead();
    
        while (mainTask != null) {
            if (imageData != null) {
                stream.Write(imageData, 0, imageData.Length);
                stream.Flush();
            }
            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }
    }
#else
    private Thread mainThread;

    void Awake() {
        mainThread = new Thread(run);
        mainThread.Start();
    }

    void OnApplicationQuit() {
        mainThread = null;
    }

    private void run() {
        while (!confirmed) {
            Thread.Sleep(10);
        }

        TcpClient client = new TcpClient();
        client.Connect(serverIP, PORT);
        NetworkStream networkStream = client.GetStream();

        while (mainThread != null) {
            if (imageData != null) {
                networkStream.Write(imageData, 0, imageData.Length);
                networkStream.Flush();
                networkStream.ReadByte();
            }
            Thread.Sleep(10);
        }

        client.Close();
    }
#endif
}
