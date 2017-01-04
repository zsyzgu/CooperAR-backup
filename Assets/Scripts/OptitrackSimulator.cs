using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if WINDOWS_UWP
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

public class OptitrackSimulator : MonoBehaviour {
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
        await listener.BindEndpointAsync(new HostName("192.168.1.154"), "" + PORT);
    }

    private async void connectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args) {
        Stream stream = args.Socket.OutputStream.AsStreamForWrite();
        StreamWriter writer = new StreamWriter(stream);

        while (mainTask != null) {
            await writer.WriteLineAsync("begin");
            await writer.WriteLineAsync(getRbMessage());
            await writer.WriteLineAsync("end");
            await writer.FlushAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }
    }
#else
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
