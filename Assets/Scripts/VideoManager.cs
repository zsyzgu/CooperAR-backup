using UnityEngine;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

public class VideoManager : MonoBehaviour {
    static VideoManager videoManager = null;
    static int MAX_STUDENTS = StudentSpawner.MAX_STUDENTS;

    const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8888;
    const int BUFFER_LEN = 1048576;

    public byte[][] videos;
    public byte[] buffer;
    private Thread mainThread;

    void Awake() {
		if (videoManager == null) {
            videoManager = this;
        }

        videos = new byte[MAX_STUDENTS][];
        buffer = new byte[BUFFER_LEN];

        mainThread = new Thread(serverThread);
        mainThread.Start();
	}

    void OnApplicationQuit() {
        mainThread = null;
    }

    private void serverThread() {
        IPAddress serverIP = IPAddress.Parse(IP_ADDRESS);
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

    static public bool getFrame(int id, out Texture2D texture) {
        if (videoManager.videos[id] != null) {
            Mat mat = Mat.FromImageData(videoManager.videos[id], LoadMode.Color);
            texture = new Texture2D(mat.Height, mat.Width, TextureFormat.RGB24, false, false);
            texture.LoadImage(videoManager.videos[id]);
            return true;
        } else {
            texture = null;
            return false;
        }
    }
}
