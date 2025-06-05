using System.Net;
using System.Net.Sockets;

namespace Finnimon.Stl.Net;

public class PeerConnection(IPAddress address)
{
    public IPAddress Peer { get; private set; }=address;
    private TcpClient Client { get; }=new TcpClient();
    

    public void StreamSend(Stl stl) => StlWriter.Write(stl, SendStream(), StlFileFormat.Binary,true);

    private Stream SendStream()
    {
        throw new NotImplementedException();
        _=Peer??throw new Exception("No peer");
        var host=TcpListener.Create(8080);
        // Client.Connect(host.Server.);
    }
}
