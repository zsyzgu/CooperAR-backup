using UnityEngine;
using System.Net;
using System.IO;
using System.Text;
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
        StreamWriter writer = new StreamWriter(networkStream);

        CvCapture capture = CvCapture.FromCamera(captureID);
        while (true) {
            Mat mat = new Mat(capture.QueryFrame());
            byte[] imageData = mat.ToBytes(".jpg");
            writer.WriteLine(Encoding.UTF8.GetString(imageData));
            networkStream.Flush();
            Thread.Sleep(10);
        }
    }
}
