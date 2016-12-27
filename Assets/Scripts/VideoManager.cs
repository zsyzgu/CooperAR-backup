using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;

public class VideoManager : MonoBehaviour {
    static VideoManager videoManager = null;
    static int MAX_STUDENTS = StudentSpawner.MAX_STUDENTS;

    const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8888;
    const int BUFFER_LEN = 1048576;

    public byte[][] videos = new byte[MAX_STUDENTS][];
    public byte[] buffer = new byte[BUFFER_LEN];
    private Thread mainThread;

    void Awake() {
		if (videoManager == null) {
            videoManager = this;
        }

        mainThread = new Thread(serverThread);
        mainThread.Start();
	}

    void OnApplicationQuit() {
        if (mainThread.IsAlive) {
            mainThread.Abort();
        }
    }

    private void serverThread() {
        IPAddress serverIP = IPAddress.Parse(IP_ADDRESS);
        TcpListener listener = new TcpListener(serverIP, PORT);

        listener.Start();
        
        while (true) {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(() => msgThread(client));
            thread.Start();
        }
    }

    private void msgThread(TcpClient client) {
        NetworkStream networkStream = client.GetStream();

        while (true) {
            byte[] imageInfo = new byte[5];
            int id = networkStream.ReadByte();
            int len = networkStream.Read(buffer, 0, buffer.Length);
            if (len == 0) {
                break;
            }
            videos[id] = new byte[len];
            Array.Copy(buffer, videos[0], len);
        }
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
