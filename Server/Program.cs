using System;
using System.IO;
using System.Threading;

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

            if (cmd == "sendtxt")
            {
                string sendtxt = Console.ReadLine();
                Server.WriteLine("sendtxt" + sendtxt);
            }
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
        //Console.WriteLine(message);
        int mesleng = message.Length;
        string mode = e.message; //モードの判別準備

        string sendmod = "";
        string sendtxt = "";

        if(message.Length > 7)
        {
            //int mesLength = message.Length;
            sendmod = message.Substring(0, 7);//modeを出すため
            sendtxt = message.Remove(0, 7);//文字列を出すため
        }

        string dlfilename = message.Remove(0, 2);
        string dl = message.Substring(0, 2);
        if (dl == "dl") mode = "dl";

        //Console.WriteLine(mode);
        if (sendmod == "sendtxt") //文字送受信モード
        {
            Console.WriteLine(sendtxt);
        }
        if (mode == "sendfile") //ファイル送信モード
        {
            Server.FileRead("sendfile.jpg",1);//1 = 画像送受信
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
            Server.FileRead("meta.tmp",0);//METADATAを受信保存a
            StreamReader sr = new StreamReader(@"meta.tmp" , System.Text.Encoding.GetEncoding("shift-jis"));
            int cnt = 0;
            while (sr.Peek() > -1)//読み込み
            {
                if(cnt == 0) name = sr.ReadLine();//1行目
                if(cnt == 1) username = sr.ReadLine();//2行目
                if(cnt == 2) date = sr.ReadLine();//3行目
                cnt++;
            }
            string rename = name + ".meta";
            sr.Close();
            File.Move("meta.tmp", rename);//ファイル名の書き換え
            string midi = name + ".midi";
            Server.FileRead(midi,0);//MIDIファイル読み込み
            string unsi = name + ".data";
            Server.FileRead(unsi,0);//運指ファイル読み込み
            //list更新
            Server.UpdateList(name, username, date);
        }
        if (mode == "dl")
        {
            Console.WriteLine(dlfilename);

            Thread.Sleep(500);
            Server.WriteFile(dlfilename + ".meta");
            Thread.Sleep(1000);//無効

            Server.WriteFile(dlfilename + ".midi");
            Console.WriteLine("midi書き込み終了");
            Thread.Sleep(1000);//有効

            Server.WriteFile(dlfilename + ".data");
            Thread.Sleep(1000);//無効
        
            Server.WriteFile(dlfilename + ".data");
            Thread.Sleep(1000);//有効

            Server.WriteFile(dlfilename + ".data");
            Console.WriteLine("data書き込み終了");
            Thread.Sleep(2500);//

            Server.WriteFile(dlfilename + ".meta");

            Server.WriteFile(dlfilename + ".meta");
            Console.WriteLine("meta書き込み終了");

            Console.WriteLine("送信プロセス終了");
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