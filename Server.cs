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
    /// <summary>
    /// 创建服务器
    /// </summary>
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
                DEBUG.Info.Print("进入");
                Handle_Player(Player);
                
            }
            
        }
        catch(Exception EX)
        {
            DEBUG.Info.Print(EX.Message);
        }
        finally
        {
            DEBUG.Info.Print("服务器已关闭");
        }
    }
    /// <summary>
    /// 关闭服务器
    /// </summary>
    public static void Off_Server()
    {
        if (TCPServer != null){
            TCPServer.Stop();
            TCPServer = null;
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
            DEBUG.Info.Print("当前为服务端!");
        }else{
        Client = new TcpClient();
        DEBUG.Info.Print("正在加入游戏...");
        try
        {
            await Client.ConnectAsync(IPs,Port);
            DEBUG.Info.Print($"已成功加入游戏IP{IPs}");
            Server_Stream = Client.GetStream();

            while (true){
            byte[] Buffer = new byte[1024];
            var s = await Server_Stream.ReadAsync(Buffer,0,Buffer.Length);
            string str = Encoding.UTF8.GetString(Buffer,0,s);
            DEBUG.Info.Print(str);
            }
        }
        catch(Exception EX)
        {
            DEBUG.Info.PrintErr(EX.Message);
            DEBUG.Info.Print(IPs,Port);
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
            DEBUG.Info.Print("结束");
        }
        }
    }
    /// <summary>
    /// 向客户端发送信息
    /// </summary>
    /// <param name="Text"></param>
    public static void Exit_Server()
    {
        if (Client != null)
        {
            Server_Stream.Close();
            Server_Stream = null;
            Client.Close();
            Client = null;
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
                DEBUG.Info.Print("啊哦,没有客户端连接当前客户端为零");
                return;
            }
            DEBUG.Info.Print($"服务端正在向{Online_Player.Count}个已连接的客户端发送信息:{Text}");
            foreach (TcpClient tcp in Online_Player){
                NetworkStream network = tcp.GetStream();
                await network.WriteAsync(Encoding.UTF8.GetBytes(Text));
            }
        }
        catch(Exception EX)
        {
            DEBUG.Info.Print(EX.Message);
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
                DEBUG.Info.Print(str);
                Currend_Data.Add(str);
            }
        }
        catch(Exception EX)
        {
            if (EX.ToString().Find("ystem.Net.Sockets.SocketException") != -1)
            {
                DEBUG.Info.Print("一个客户端已断开连接");
            }
            else
            {
                DEBUG.Info.Print(EX);
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
