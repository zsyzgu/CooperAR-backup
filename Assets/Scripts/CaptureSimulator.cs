using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class CaptureSimulator : MonoBehaviour {
    /*const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8888;

    public int captureID = 0;
    Thread mainThread;

    void Awake () {
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
        while (true) {
            networkStream.WriteByte((byte)captureID);
            networkStream.Flush();
            Mat mat = new Mat(capture.QueryFrame());
            byte[] imageData = mat.ToBytes(".jpg");
            networkStream.Write(imageData, 0, imageData.Length);
            networkStream.Flush();
            Thread.Sleep(10);
        }
    }*/
}
