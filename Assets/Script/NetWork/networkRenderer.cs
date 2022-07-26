
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using UnityEngine;


public class networkRenderer: MonoBehaviour
{
    public async void Start()
    {
        await NetWork.RunServerAsync();
    }

}

public static class NetWork
{
    public static Vector3 severSamplePos = new Vector3(0, 0, 0);
    public static int severSampleRotation = 0;

    public static int serverSampleSpeed = 0;

    public static IChannel bootstrapChannel;



    public static async Task RunServerAsync()
    {
        var bossGroup = new MultithreadEventLoopGroup();

        var cliantBootstrap = new Bootstrap();
        cliantBootstrap
        .Group(bossGroup)
        .Channel<TcpSocketChannel>()
        .Option(ChannelOption.TcpNodelay, true)
        .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
        {
            IChannelPipeline pipeline = channel.Pipeline;
            pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
            
            pipeline.AddLast(new StringEncoder());
            pipeline.AddLast("echo", new EchoClientHandler());
        }));

        bootstrapChannel = await cliantBootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 29100));

    }

    public static void send(List<byte> packet)
    {
        NetWork.bootstrapChannel.WriteAndFlushAsync(Encoding.UTF8.GetString(packet.ToArray()));
    }

    //�� ����
    public static void sendNetworkSrtingData(string str)
    {
        List<byte> packet = new List<byte> { 0x01, 0x01 };
        packet.AddRange(Encoding.UTF8.GetBytes(str));
        NetWork.bootstrapChannel.WriteAndFlushAsync(packet);
    }

}

public class NetWorkHandler:  SimpleChannelInboundHandler<string>{
    protected override void ChannelRead0(IChannelHandlerContext contex, string msg) => Debug.Log(msg);
    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        
        Debug.Log("Exception: " + exception);
        context.CloseAsync();
    }
}

 public class EchoClientHandler : ChannelHandlerAdapter
{
    readonly IByteBuffer initialMessage;

    public EchoClientHandler()
    {
        this.initialMessage = Unpooled.Buffer(8007);
        byte[] messageBytes = Encoding.UTF8.GetBytes("0000");
        this.initialMessage.WriteBytes(messageBytes);
    }

    public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var byteBuffer = message as IByteBuffer;
        if (byteBuffer != null)
        {
            //Debug.Log("Received from server: " + byteBuffer.ToString(Encoding.UTF8).Length);
            //Debug.Log(byteBuffer.ToString(Encoding.UTF8).Substring(2));

            string serverMessage=byteBuffer.ToString(Encoding.UTF8).Substring(2);
            
            int msgIndex=0;

            if((msgIndex=serverMessage.IndexOf("UD{"))>-1)
            {
                string userListMsg=serverMessage.Substring(msgIndex+3,serverMessage.Length-msgIndex-4);
                string[] userListData=userListMsg.Split('{','}');

                for(int i=0;i<userListData.Length-1;i++)
                {

                    NetWork.severSamplePos=new Vector3(-float.Parse(userListData[1].Split(',')[0]),float.Parse(userListData[1].Split(',')[1])/10,-float.Parse(userListData[1].Split(',')[2]));
                    NetWork.serverSampleSpeed=int.Parse(userListData[1].Split(',')[3]);
                    NetWork.severSampleRotation=(int)(int.Parse(userListData[1].Split(',')[4])*1.4f);
                }

            }
        }
        //context.WriteAsync(message);
    }

    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Debug.Log("Exception: " + exception);
        context.CloseAsync();
    }

}
