using System;
using System.IO;

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
        if (mode == "") mode = e.message; //モードの判別準備
        Console.WriteLine(mode);
        if (mode == "test") //文字送受信モード
        {
            Console.WriteLine(mode);
        }
        if (mode == "sendfile") //ファイル送信モード
        {
            Server.FileRead("sendfile.jpg");
        }
        if (mode == "list")//リスト送信
        {
            Server.WriteFile("List.lst");
        }
        if (mode == "up")//MIDIMETA運指を受け取る
        {
            string name = "";
            string username = "";
            string date = "";
            Server.FileRead("meta.tmp");//METADATAを受信保存
            StreamReader sr = new StreamReader(@"meta.tmp");
            int cnt = 0;
            while (sr.Peek() > -1)//読み込み
            {
                if(cnt == 0) name = sr.ReadLine();//1行目
                if(cnt == 1) username = sr.ReadLine();//2行目
                if(cnt == 2) date = sr.ReadLine();//3行目
                cnt++;
            }
            Console.WriteLine(name, username, date);
            string rename = name + ".meta";
            File.Move("meta.tmp", rename);//ファイル名の書き換え
            string midi = name + ".midi";
            Server.FileRead(midi);//MIDIファイル読み込み
            string unsi = name + ".data";
            Server.FileRead(unsi);//運指ファイル読み込み
            //list更新
            Server.UpdateList(name, username, date);
        }
        if (mode == "dl")
        {
            string filename = Server.ReadM();
            Server.WriteFile(filename + ".meta");
            Server.WriteFile(filename + ".midi");
            Server.WriteFile(filename + ".data");
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