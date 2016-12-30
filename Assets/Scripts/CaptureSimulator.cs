using UnityEngine;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

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
    private CvCapture capture;

    private byte[] getData() {
        Mat mat = new Mat(capture.QueryFrame());
        return mat.ToBytes(".jpg");
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

        capture = CvCapture.FromCamera(captureID);
        while (mainTask != null) {
            byte[] imageData = getData();
            stream.Write(imageData, 0, imageData.Length);
            stream.Flush();
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

        capture = CvCapture.FromCamera(captureID);
        while (mainThread != null) {
            byte[] imageData = getData();
            networkStream.Write(imageData, 0, imageData.Length);
            networkStream.Flush();
            Thread.Sleep(10);
        }

        client.Close();
    }
#endif
}
