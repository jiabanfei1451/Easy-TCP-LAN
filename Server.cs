using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Godot;

namespace NETDNS;
/// <summary>
/// 自制联机库
/// </summary>
public static class Server
{
    /// <summary>
    /// IP
    /// </summary>
    public static string IPs = "192.168.10.19";
    /// <summary>
    /// 端口
    /// </summary>
    public static int Port = 7777;
    /// <summary>
    /// TCP服务器
    /// </summary>
    public static TcpListener TCPServer;
    /// <summary>
    /// 服务器流
    /// </summary>
    public static NetworkStream Server_Stream;
    /// <summary>
    /// 连接数据
    /// </summary>
    public static TcpClient Client;
    /// <summary>
    /// 在线玩家
    /// </summary>
    public static List<TcpClient> Online_Player = new List<TcpClient>();
    /// <summary>
    /// 创建服务端
    /// </summary>
    public static List<string> Currend_Data = new List<string>();
    public static async void Create_Server()
    {
        IPAddress iPAddress = IPAddress.Parse(IPs);
        IPEndPoint iPEnd = new IPEndPoint(iPAddress,Port);
        TcpListener Tcp_Server = new TcpListener(iPEnd);

        try
        {
            Tcp_Server.Start();
            TCPServer = Tcp_Server;
            
            while (true)
            {
                TcpClient Player = await TCPServer.AcceptTcpClientAsync();
                Server_Stream = Player.GetStream();
                GD.Print("进入");
                Handle_Player(Player);
                
            }
            
        }
        catch(Exception EX)
        {
            GD.Print(EX.Message);
        }
        finally
        {
            GD.Print("服务器已关闭");
        }
    }
     /// <summary>
    /// 加入服务器
    /// </summary>
    public static async void Join_Server()
    {
        try
        {
            IPAddress iPAddress = IPAddress.Parse(IPs);
            IPEndPoint iPEnd = new IPEndPoint(iPAddress,Port);
            TcpListener Tcp_Server = new TcpListener(iPEnd);
            Tcp_Server.Start();
        }
        catch
        {
            
        }
        finally
        {
            
        }
        if (TCPServer != null)
        {
            GD.Print("当前为服务端!");
        }else{
        Client = new TcpClient();
        GD.Print("正在加入游戏...");
        try
        {
            await Client.ConnectAsync(IPs,Port);
            GD.Print($"已成功加入游戏IP{IPs}");
            Server_Stream = Client.GetStream();

            while (true){
            byte[] Buffer = new byte[1024];
            var s = await Server_Stream.ReadAsync(Buffer,0,Buffer.Length);
            string str = Encoding.UTF8.GetString(Buffer,0,s);
            GD.Print(str);
            }
        }
        catch(Exception EX)
        {
            GD.PrintErr(EX.Message);
            GD.Print(IPs,Port);
        }
        finally
        {
            if (Client != null){
            Client.Close();
            }
            Client = null;
            if (Server_Stream != null){
            Server_Stream.Close();
            }
            Server_Stream = null;
            GD.Print("结束");
        }
        }
    }
    /// <summary>
    /// 向客户端发送信息
    /// </summary>
    /// <param name="Text"></param>
    public static async void Player_Send_Data(String Text)
    {
        try{
            if (Server_Stream == null){return;}
            if (Online_Player.Count <= 0){
                GD.Print("啊哦,没有客户端连接当前客户端为零");
                return;
            }
            GD.Print($"服务端正在向{Online_Player.Count}个已连接的客户端发送信息:{Text}");
            foreach (TcpClient tcp in Online_Player){
                NetworkStream network = tcp.GetStream();
                await network.WriteAsync(Encoding.UTF8.GetBytes(Text));
            }
        }
        catch(Exception EX)
        {
            GD.Print(EX.Message);
        }
        finally
        {
            
        }
    }
    /// <summary>
    /// 处理客户端
    /// </summary>
    /// <param name="Player"></param>
    static async void Handle_Player(TcpClient Player)
    {
        Online_Player.Add(Player);
        NetworkStream PlayerStream = null;
        try{
            PlayerStream = Player.GetStream();
            while (true)
            {
                byte[] Buffer = new byte[1024];
                int Len = await PlayerStream.ReadAsync(Buffer,0,Buffer.Length);
                string str = Encoding.UTF8.GetString(Buffer,0,Len);
                GD.Print(str);
                Currend_Data.Add(str);
            }
        }
        catch(Exception EX)
        {
            if (EX.ToString().Find("ystem.Net.Sockets.SocketException") != -1)
            {
                GD.Print("一个客户端已断开连接");
            }
            else
            {
                GD.Print(EX);
            }
        }
        finally
        {
            PlayerStream.Close();
            Player.Close();
            lock (Online_Player)
            {
                Online_Player.Remove(Player);
            }
        }
    }
}
