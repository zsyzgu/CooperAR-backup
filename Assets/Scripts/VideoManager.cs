using UnityEngine;
using System.IO;
using System;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
using System.Net;
using System.Net.Sockets;
#else
using System.Threading;
using System.Net;
using System.Net.Sockets;
#endif

public class VideoManager : MonoBehaviour {
    static VideoManager videoManager = null;
    static int MAX_STUDENTS = StudentSpawner.MAX_STUDENTS;
    
    const int PORT = 8888;
    const int BUFFER_LEN = 1048576;

    private byte[][] videos;
    private byte[] buffer;
    
    static public bool getFrame(int id, out Texture2D texture) {
        if (videoManager.videos[id] != null) {
            texture = new Texture2D(640, 480, TextureFormat.RGB24, false, false);
            texture.LoadImage(videoManager.videos[id]);
            return true;
        } else {
            texture = null;
            return false;
        }
    }

    void Awake() {
        if (videoManager == null) {
            videoManager = this;
        }

        videos = new byte[MAX_STUDENTS][];
        buffer = new byte[BUFFER_LEN];

        startThread();
    }

#if WINDOWS_UWP
    private Task mainTask;
    
    void startThread() {
        mainTask = new Task(serverThread);
        mainTask.Start();
    }

    void OnApplicationQuit() {
        mainTask = null;
    }

    string getIP() {
        foreach (HostName localHostName in NetworkInformation.GetHostNames()) {
            if (localHostName.IPInformation != null) {
                if (localHostName.Type == HostNameType.Ipv4) {
                    return localHostName.ToString();
                }
            }
        }
        return "127.0.0.1";
    }

    private async void serverThread() {
        StreamSocketListener listener = new StreamSocketListener();
        listener.ConnectionReceived += connectionReceived;
        await listener.BindEndpointAsync(new HostName(getIP()), "" + PORT);
    }

    private async void connectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args) {
        Stream stream = args.Socket.InputStream.AsStreamForRead();

        while (mainTask != null) {
            int len = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (len == 0) {
                break;
            }
            videos[0] = new byte[len];
            Array.Copy(buffer, videos[0], len);
        }
    }
#else
    private Thread mainThread;

    void startThread() {
        string ipAddress = Network.player.ipAddress;
        mainThread = new Thread(() => serverThread(ipAddress));
        mainThread.Start();
	}

    void OnApplicationQuit() {
        mainThread = null;
    }

    private void serverThread(string ipAddress) {
        IPAddress serverIP = IPAddress.Parse(ipAddress);
        TcpListener listener = new TcpListener(serverIP, PORT);
        listener.Start();
        while (mainThread != null) {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(() => msgThread(client));
            thread.Start();
        }
    }

    private void msgThread(TcpClient client) {
        NetworkStream networkStream = client.GetStream();

        while (mainThread != null) {
            int len = networkStream.Read(buffer, 0, buffer.Length);
            if (len == 0) {
                break;
            }
            videos[0] = new byte[len];
            Array.Copy(buffer, videos[0], len);
        }

        client.Close();
    }
#endif
}
