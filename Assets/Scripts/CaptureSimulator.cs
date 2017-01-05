using UnityEngine;

#if WINDOWS_UWP
#else
using System.Net.Sockets;
using System.Threading;
#endif

public class CaptureSimulator : MonoBehaviour {
#if WINDOWS_UWP
#else
    const int PORT = 8888;

    public int captureID = 0;
    private WebCamTexture webCamTexture;
    private byte[] imageData;
    private string serverIP;
    private bool confirmed;

    void Start() {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
        serverIP = Network.player.ipAddress;
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
                imageData = null;
            }
        }

        client.Close();
    }
#endif
}
