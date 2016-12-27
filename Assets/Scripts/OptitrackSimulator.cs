using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class OptitrackSimulator : MonoBehaviour {
    const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8520;

    private Thread mainThread;
    
	void Awake() {
        mainThread = new Thread(run);
        mainThread.Start();
	}

    void OnApplicationQuit() {
        if (mainThread.IsAlive) {
            mainThread.Abort();
        }
    }

    void run() {
        IPAddress serverIP = IPAddress.Parse(IP_ADDRESS);
        TcpListener listener = new TcpListener(serverIP, PORT);
        listener.Start();
        TcpClient client = listener.AcceptTcpClient();
        StreamWriter sw = new StreamWriter(client.GetStream());

        float ry = 0f;
        while (true) {
            sw.WriteLine("begin");
            int id = 0;
            float x = 0f;
            float y = 0f;
            float z = 1f;
            float rx = 0f;
            ry = ry + 1f;
            float rz = 0f;
            sw.WriteLine("rb " + id.ToString() + " " + x.ToString() + " " + y.ToString() + " " + z.ToString() + " " + rx.ToString() + " " + ry.ToString() + " " + rz.ToString());
            sw.WriteLine("end");
            sw.Flush();
            Thread.Sleep(10);
        }
    }
}
