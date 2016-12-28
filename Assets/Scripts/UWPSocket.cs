#if WINDOWS_UWP
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class UWPSocket {
#if WINDOWS_UWP
    StreamSocket socket;
    public async Task connect(string host, int port) {
        HostName hostName;

        using (socket = new StreamSocket()) {
            hostName = new HostName(host);
            
            socket.Control.NoDelay = false;

            try {
                await socket.ConnectAsync(hostName, "" + port);
            }
            catch (Exception exception) {
                switch (SocketError.GetStatus(exception.HResult)) {
                    case SocketErrorStatus.HostNotFound:
                        throw;
                    default:
                        throw;
                }
            }
        }
    }

    public async Task send(string message) {
        DataWriter writer;
        
        using (writer = new DataWriter(socket.OutputStream)) {
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.MeasureString(message);
            writer.WriteString(message);
            
            try {
                await writer.StoreAsync();
            }
            catch (Exception exception) {
                switch (SocketError.GetStatus(exception.HResult)) {
                    case SocketErrorStatus.HostNotFound:
                        throw;
                    default:
                        throw;
                }
            }

            await writer.FlushAsync();
            writer.DetachStream();
        }
    }

    public async Task<String> read() {
        DataReader reader;
        StringBuilder strBuilder;

        using (reader = new DataReader(socket.InputStream)) {
            strBuilder = new StringBuilder();
            
            reader.InputStreamOptions = InputStreamOptions.Partial;
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;
            
            await reader.LoadAsync(256);
            
            while (reader.UnconsumedBufferLength > 0) {
                strBuilder.Append(reader.ReadString(reader.UnconsumedBufferLength));
                await reader.LoadAsync(256);
            }

            reader.DetachStream();
            return strBuilder.ToString();
        }
    }
#endif
}
