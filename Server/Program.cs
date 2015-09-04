using System;

class Program
{
    static void Main(string[] args)
    {
        // 接続できたら発生するイベントの登録
        Server.ReciveStream += Server_ReceiveStream;

        // メッセージを受け取った時に発生するイベントの登録
        Server.ReceiveMessage += Server_ReceiveMessage;

        // 接続切れたのを探知
        Server.OnDisconnect += Server_OnDisconnet;

        // サーバを起動
        Server.Start();

        while (true)
        {
            var cmd = Console.ReadLine();

            if (cmd == "send") Server.WriteLine("test");
            if (cmd == "sendfile")
            {
                Server.WriteLine("sendfile");
                Server.WriteFile("hoge.jpg");
            }
            if (cmd == "quit") break;
        }

        Server.Dispose();
    }

    static void Server_ReceiveStream(object sender, EventArgs e)
    {
        Console.WriteLine("クライアントと接続");
    }
    
    static void Server_ReceiveMessage(object sender, ReciveEventArgs e)
    {
        var message = e.message;
        int mesleng = message.Length;
        string mode = "";
        if (mesleng > 6) mode = "sendfile";
        else mode = "test";
        if (mode == "") mode = e.message; //モードの判別準備

        if (mode == "test") //文字送受信モード
        {
            Console.WriteLine(mode);
        }
        if (mode == "sendfile") //ファイル送信モード
        {
            Server.Read();
        }
    }

    static void Server_OnDisconnet(object sender, EventArgs e)
    {
        Console.WriteLine("OnDisconnect");
    }

}

public class ReciveEventArgs : EventArgs
{
    public string message;
    public ReciveEventArgs(string message)
    {
        this.message = "";
        this.message = message;
    }
}