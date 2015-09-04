using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server
{
    // IPアドレス
    public static string host = "localhost";
    //static IPAddress localAddr = IPAddress.Parse("localhost");
    // ポート番号
    static Int32 port = 22154;

    static TcpListener listener = null;
    static TcpClient client;
    static NetworkStream stream;

    // サーバから受け取るデータ
    static byte[] resBytes = new byte[256];

    static Thread thread;
    static Encoding encoding = Encoding.UTF8;

    public static event EventHandler ReciveStream;
    public static event EventHandler<ReciveEventArgs> ReceiveMessage;
    public static event EventHandler OnDisconnect;

    public static bool IsConnected
    {
        get
        {
            if (client == null) return false;
            else return client.Connected;
        }
    }

    // サーバ起動
    public static void Start()
    {
        Console.WriteLine("Listen Start");
        var ts = new ThreadStart(socketConnectThread);
        thread = new Thread(ts);
        thread.Start();
    }


    static void socketConnectThread()
    {
        try
        {
            listener = new TcpListener(/*localAddr , */port);
            listener.Start();
            client = listener.AcceptTcpClient();
            stream = client.GetStream();

            if (ReciveStream != null) ReciveStream(null, EventArgs.Empty);

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }

        // 別スレッドで無限ループ
        while (true)
        {
            try
            {
                string resMessage;
                do
                {
                    resMessage = Server.ReadM();

                    if (resMessage == null)
                    {
                        Console.WriteLine("Disconnect");
                        if (OnDisconnect != null) OnDisconnect(null, EventArgs.Empty);
                        return;
                    }
                } while (stream.DataAvailable);
                if (ReceiveMessage != null) ReceiveMessage(null, new ReciveEventArgs(resMessage));
                resMessage = "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void Read()
    {
        MemoryStream memoryStream = new MemoryStream();
        int size = 0;
        int resize = 0;
        int flag = 0;
        int cnt = 0;
        while (true)
        {
            Console.WriteLine("受信中");
            size = stream.Read(resBytes, 0, resBytes.Length);
            if (size < 255 && cnt > 2) flag = 1;
            Console.WriteLine("{0}回目", cnt);
            resize = resize + size;
            Console.WriteLine("{0}byte受信おわり",resize);
            Console.WriteLine("memoryStreamに書き込み");
            memoryStream.Write(resBytes, 0, size);
            cnt++;
            if (flag == 1)
            {
                Console.WriteLine("return");
                break;
            }
        }
        /*byte[]にそのまま入れる*/
        Console.WriteLine("memoryStreamをClose");
        //ToArrayはMemoryStreamをCloseした時以外は正しく動作しない模様。
        memoryStream.Close();
        int bytes = 256 * cnt;
        byte[] file = new byte[bytes];
        file = memoryStream.ToArray();
        Console.WriteLine("書き込み開始");
        Write(file , "sendfile.jpg");
    }

    // クライアントにメッセージ送信
    public static void WriteLine(string message)
    {
        if (!Server.IsConnected)
        {
            Console.WriteLine("Network is Not Connected");
            return;
        }
        
        message += "\n";
        byte[] bytes = encoding.GetBytes(message);
        stream.Write(bytes, 0, bytes.Length);
        Console.WriteLine(message);
        message = "";
    }

    public static void WriteFile(string filename)
    {
        byte[] sendBytes = File.ReadAllBytes(filename);
        stream.Write(sendBytes, 0, sendBytes.Length);
    }

    public static void Write(byte[] nakami , string filename)
    {
        File.WriteAllBytes(filename , nakami);
        Console.WriteLine("書き込み終了。");
    }

    public static string ReadM()
    {
        MemoryStream memoryStream = new MemoryStream();
        int size = stream.Read(resBytes, 0, resBytes.Length);
        if (size == 0) return null;

        memoryStream.Write(resBytes, 0, size);
        memoryStream.Close();
        return encoding.GetString(memoryStream.ToArray());
    }

    // 破棄
    public static void Dispose()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        if (thread.IsAlive)
        {
            thread.Abort();
            thread.Join();
        }
        if (listener != null) listener.Stop();
    }
}