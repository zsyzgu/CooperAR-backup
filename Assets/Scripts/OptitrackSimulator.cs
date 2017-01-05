using UnityEngine;
using System.IO;

#if WINDOWS_UWP
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

public class OptitrackSimulator : MonoBehaviour {
#if WINDOWS_UWP
#else
    const int PORT = 8520;

    private float ry = 0f;

    private string getRbMessage() {
        int id = 0;
        float x = 0f;
        float y = 0f;
        float z = 1f;
        float rx = 0f;
        ry = ry + 1f;
        float rz = 0f;
        return "rb " + id.ToString() + " " + x.ToString() + " " + y.ToString() + " " + z.ToString() + " " + rx.ToString() + " " + ry.ToString() + " " + rz.ToString();
    }

    private Thread mainThread;
    
	void Awake() {
        string ipAddress = Network.player.ipAddress;
        mainThread = new Thread(() => run(ipAddress));
        mainThread.Start();
	}

    void OnApplicationQuit() {
        mainThread = null;
    }

    private void run(string ipAddress) {
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
        StreamWriter sw = new StreamWriter(client.GetStream());

        while (mainThread != null) {
            sw.WriteLine("begin");
            sw.WriteLine(getRbMessage());
            sw.WriteLine("end");
            sw.Flush();
            Thread.Sleep(10);
        }

        client.Close();
    }
#endif
}
