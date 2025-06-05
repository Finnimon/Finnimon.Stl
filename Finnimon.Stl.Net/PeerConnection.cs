using System.Net;
using System.Net.Sockets;

namespace Finnimon.Stl.Net;

public class PeerConnection(IPAddress address)
{
    public IPAddress Peer { get; private set; }=address;
    private TcpClient Client { get; }=new TcpClient();
    
}
