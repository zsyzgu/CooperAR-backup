using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Text;
using Windows.Storage.Streams;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
#endif

public class OptitrackSimulator : MonoBehaviour {
    const string IP_ADDRESS = "127.0.0.1";
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
        StreamSocketListener listener = new StreamSocketListener();
        listener.ConnectionReceived += connectionReceived;
        await listener.BindServiceNameAsync("" + PORT);
    }

    private async void connectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args) {
        DataWriter writer = new DataWriter(args.Socket.OutputStream);
        
        while (mainTask != null) {
            writeLine(writer, "begin");
            writeLine(writer, getRbMessage());
            writeLine(writer, "end");
            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }
    }

    private async void writeLine(DataWriter writer, string msg) {
        writer.MeasureString(msg);
        writer.WriteString(msg);
        await writer.StoreAsync();
        await writer.FlushAsync();
        writer.DetachStream();
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
        IPAddress serverIP = IPAddress.Parse(IP_ADDRESS);
        TcpListener listener = new TcpListener(serverIP, PORT);
        listener.Start();
        TcpClient client = listener.AcceptTcpClient();
        StreamWriter sw = new StreamWriter(client.GetStream());

        float ry = 0f;
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
