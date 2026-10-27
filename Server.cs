using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NETDNS;
/// <summary>
/// 自制联机库
/// </summary>
public static class Server
{
    /// <summary>
    /// 客户端流信息
    /// </summary>
    public static Action<String> GetPlayer_Stream;
    /// <summary>
    /// 服务器流信息
    /// </summary>
    public static Action<String> GetServer_Stream;
    /// <summary>
    /// 数据流
    /// </summary>
    public static string Data_Stream = "";
    /// <summary>
    /// IP
    /// </summary>
    public static string IPs = "127.0.0.1";
    /// <summary>
    /// 端口
    /// </summary>
    public static ushort Port = 7777;
    /// <summary>
    /// 校验数据流
    /// </summary>
    public static String Check_character = "Easy-TCP-LAN";
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
        if (!(TCPServer is null))
        {
            NETDNS.Info.Print("当前服务器已被创建,请使用其他端口");
            return;
        }
        IPAddress iPAddress = IPAddress.Parse(IPs);
        IPEndPoint iPEnd = new IPEndPoint(iPAddress,Port);
        TcpListener Tcp_Server = new TcpListener(iPEnd);
        try
        {
            Tcp_Server.Start();
            TCPServer = Tcp_Server;
            NETDNS.Info.Print("服务端已创建");
            while (true)
            {
                TcpClient Player = await TCPServer.AcceptTcpClientAsync();
                Server_Stream = Player.GetStream();
                Handle_Player(Player);
            }
            
        }
        catch(Exception EX)
        {
            NETDNS.Info.Print(EX.Message);
        }
        finally
        {
            try{
                Server_Stream.Close();
            }catch(Exception EX)
            {
                NETDNS.Info.Print($"关闭时出现错误 错误信息:{EX.Message}");
            }
            finally
            {
            }
            Server_Stream = null;
            NETDNS.Info.Print("服务器已关闭");
        }
    }
    /// <summary>
    /// 检测端口
    /// </summary>
    /// <returns></returns>
    public static bool Cheak_Port()
    {
        Socket socket = null;
        bool h = false;
        try
        {
            IPAddress ipa = IPAddress.Parse(IPs);
            IPEndPoint iPEndPoint = new IPEndPoint(ipa,Port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(iPEndPoint);
        }
        catch(SocketException ex)
        {
            NETDNS.Info.Print("计算机端口检测失败 错误信息:",ex);
            h = true;
        }
        finally
        {
            if(socket != null)
            {
                socket.Close();
                socket.Dispose();
                h = false;
            }
        }
        return h;
    }
    /// <summary>
    /// 关闭服务器
    /// </summary>
    public static void Off_Server()
    {
        if (TCPServer != null){
            TCPServer.Stop();
            TCPServer = null;
            foreach (TcpClient client in Online_Player)
            {
                if (client != null){
                    client.Close();
                }
            }
            if (Server_Stream != null){
                Server_Stream.Close();
                Server_Stream = null;
            }
        }
        Online_Player.Clear();
        Data_Stream = "";
    }
    /// <summary>
    /// 加入服务器
    /// </summary>
    public static async void Join_Server()
    {
        if (!(Server_Stream is null))
        {
            NETDNS.Info.Print($"当前进程已连接{IPs}:{Port},请勿重复连接");
            return;
        }
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
            NETDNS.Info.Print("当前为服务端!");
        }else{
        Client = new TcpClient();
        NETDNS.Info.Print("正在加入游戏...");
        try
        {
            await Client.ConnectAsync(IPs,Port);
            NETDNS.Info.Print($"已成功加入游戏IP{IPs}");
            Server_Stream = Client.GetStream();
            Player_Send_Data(Check_character);
            while (Server_Stream != null){
                byte[] Buffer = new byte[1024];
                var s = await Server_Stream.ReadAsync(Buffer,0,Buffer.Length);
                string str = Encoding.UTF8.GetString(Buffer,0,s);
                Data_Stream += str;
                if( str == "")
                {
                    NETDNS.Info.Print("正在断开链接");
                    break;
                }
                try{
                    GetServer_Stream.Invoke(str);
                }catch(Exception EX)
                {
                    NETDNS.Info.Print($"方法调用错误 错误信息:{EX.Message}");
                }finally{}
                NETDNS.Info.Print("客户端已接收:" + str);
                await Task.Delay(100);
            }
        }
        catch(Exception EX)
        {
            NETDNS.Info.Print(EX.Message);
            NETDNS.Info.Print(IPs,Port);
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
            NETDNS.Info.Print("结束");
        }
        }
        Data_Stream = "";
    }
    /// <summary>
    /// 退出服务器
    /// </summary>
    /// <param name="Text"></param>
    public static void Exit_Server()
    {
        if (Client != null)
        {
            NETDNS.Info.Print("退出中");
            Player_Send_Data("<<Exit>>");
            Server_Stream.Close();
            Server_Stream = null;
            Client.Close();
            Client = null;
        }
        else
        {
            NETDNS.Info.Print("当前没有正在连接的服务器流");
        }
    }
    /// <summary>
    /// 客户端发送
    /// </summary>
    /// <param name="Text"></param>
    public static void Player_Send_Data(String Text)
    {
        if (TCPServer != null)
        {
            NETDNS.Info.Print("当前为服务端");
            return;
        }
        try
        {
            if (Server_Stream == null){return;}
            Server_Stream.Write(Encoding.UTF8.GetBytes(Text));
        }catch(Exception EX)
        {
            NETDNS.Info.Print(EX.Message);
        }
        finally
        {
            
        }
    }
    /// <summary>
    /// 向客户端发送信息
    /// </summary>
    /// <param name="Text"></param>
    public static async void Server_Send_Data(String Text)
    {
        try{
            if (Server_Stream == null){return;}
            if (Online_Player.Count <= 0){
                NETDNS.Info.Print("啊哦,没有客户端连接当前客户端为零");
                return;
            }
            NETDNS.Info.Print($"服务端正在向{Online_Player.Count}个已连接的客户端发送信息:{Text}");
            foreach (TcpClient tcp in Online_Player){
                NetworkStream network = tcp.GetStream();
                await network.WriteAsync(Encoding.UTF8.GetBytes(Text));
            }
        }
        catch(Exception EX)
        {
            NETDNS.Info.Print(EX.Message);
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
        IPEndPoint iPEnd = Player.GetStream().Socket.RemoteEndPoint as IPEndPoint;
        if (!(iPEnd is null)){
            NETDNS.Info.Print($"客户端:{iPEnd.Address}");
        }
        NetworkStream PlayerStream = null;
        string str = "";
        bool through = false;
        try{
            PlayerStream = Player.GetStream();
            while (TCPServer != null)
            {
                byte[] Buffer = new byte[1024];
                int Len = await PlayerStream.ReadAsync(Buffer,0,Buffer.Length);
                str = Encoding.UTF8.GetString(Buffer,0,Len);
                try{
                    GetPlayer_Stream.Invoke(str);
                }catch(Exception EX)
                {
                    NETDNS.Info.Print($"方法调用错误 错误信息:{EX.Message}");
                }finally{}

                Data_Stream += str;
                NETDNS.Info.Print("服务端已接收:" + str);
                if (str == Check_character && through == false)
                {
                    Player.GetStream().Write(Encoding.UTF8.GetBytes("校验已通过"));
                    through = true;
                }
                else
                {
                    break;
                }
                if (str == "<<Exit>>")
                {
                    break;
                }
                Currend_Data.Add(str);
            }
        }
        catch(Exception EX)
        {
            NETDNS.Info.Print("一个客户端已断开连接");
            NETDNS.Info.Print(EX);
        }
        finally
        {
            if (through == false)
            {
                NETDNS.Info.Print("校验不通过已主动断开连接");
            }
            if (str == "<<Exit>>")
            {
                NETDNS.Info.Print("客户端主动退出");
            }
            PlayerStream.Close();
            Player.Close();
            lock (Online_Player)
            {
                Online_Player.Remove(Player);
            }
        }
    }
}
