using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Tracking : MonoBehaviour {
    static Tracking tracking = null;
    static int MAX_STUDENTS = StudentSpawner.MAX_STUDENTS;

    const string IP_ADDRESS = "127.0.0.1";
    const int PORT = 8520;
    
    public class TrackingFrame {
        public bool[] exist;
        public Vector3[] pos;
        public Vector3[] rot;

        public TrackingFrame() {
            exist = new bool[MAX_STUDENTS];
            pos = new Vector3[MAX_STUDENTS];
            rot = new Vector3[MAX_STUDENTS];
        }
    }

    public TrackingFrame currFrame = new TrackingFrame();
    private Thread mainThread;

    void Awake() {
        if (tracking == null) {
            tracking = this;
        }

        mainThread = new Thread(clientThread);
        mainThread.Start();
    }

    void OnApplicationQuit() {
        if (mainThread.IsAlive) {
            mainThread.Abort();
        }
    }

    private void clientThread() {
        IPAddress clientIP = IPAddress.Parse(IP_ADDRESS);
        TcpClient client = new TcpClient();
        client.Connect(IP_ADDRESS, PORT);

        NetworkStream networkStream = client.GetStream();
        StreamReader sr = new StreamReader(networkStream);

        TrackingFrame frame = null;
        while (true) {
            string msg = sr.ReadLine();
            if (msg == null || msg == "exit") {
                break;
            }
            if (msg == "begin") {
                frame = new TrackingFrame();
            }
            if (msg == "end") {
                currFrame = frame;
            }
            if (msg.Split(' ')[0] == "rb") {
                string[] tags = msg.Split(' ');
                int id = int.Parse(tags[1]);
                float x = float.Parse(tags[2]);
                float y = float.Parse(tags[3]);
                float z = float.Parse(tags[4]);
                float rx = float.Parse(tags[5]);
                float ry = float.Parse(tags[6]);
                float rz = float.Parse(tags[7]);
                frame.exist[id] = true;
                frame.pos[id] = new Vector3(x, y, z);
                frame.rot[id] = new Vector3(rx, ry, rz);
            }
        }
    }

    static public bool getTransform(int id, out Vector3 position, out Vector3 rotation) {
        if (0 <= id && id < MAX_STUDENTS && tracking.currFrame.exist[id]) {
            position = tracking.currFrame.pos[id];
            rotation = tracking.currFrame.rot[id];
            return true;
        } else {
            position = Vector3.zero;
            rotation = Vector3.zero;
            return false;
        }
    }
}
