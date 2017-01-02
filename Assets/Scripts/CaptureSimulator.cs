using UnityEngine;
using System.IO;

#if WINDOWS_UWP
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
#else
using System.Net.Sockets;
using System.Threading;
#endif

public class CaptureSimulator : MonoBehaviour {
    const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8888;

    public int captureID = 0;
    private WebCamTexture webCamTexture;
    private byte[] imageData;

    void Start() {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
    }

    void Update() {
        Texture2D texture2D = new Texture2D(webCamTexture.width, webCamTexture.height);
        texture2D.SetPixels(webCamTexture.GetPixels());
        imageData = texture2D.EncodeToJPG();
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
        StreamSocket socket = new StreamSocket();
        await socket.ConnectAsync(new HostName(IP_ADDRESS), "" + PORT);
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
        TcpClient client = new TcpClient();
        client.Connect(IP_ADDRESS, PORT);

        NetworkStream networkStream = client.GetStream();
        
        while (mainThread != null) {
            if (imageData != null) {
                networkStream.Write(imageData, 0, imageData.Length);
                networkStream.Flush();
            }
            Thread.Sleep(10);
        }

        client.Close();
    }
#endif
}
