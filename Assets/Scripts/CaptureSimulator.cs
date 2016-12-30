using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

public class CaptureSimulator : MonoBehaviour {
    const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8888;

    public int captureID = 0;
    Thread mainThread;

    void Awake() {
        mainThread = new Thread(run);
        mainThread.Start();
    }

    void OnApplicationQuit() {
        if (mainThread.IsAlive) {
            mainThread.Abort();
        }
    }

    private void run() {
        TcpClient client = new TcpClient();
        client.Connect(IP_ADDRESS, PORT);

        NetworkStream networkStream = client.GetStream();

        CvCapture capture = CvCapture.FromCamera(captureID);
        while (mainThread != null) {
            Mat mat = new Mat(capture.QueryFrame());
            byte[] imageData = mat.ToBytes(".jpg");
            networkStream.Write(imageData, 0, imageData.Length);
            networkStream.Flush();
            Thread.Sleep(10);
        }

        client.Close();
    }
}
