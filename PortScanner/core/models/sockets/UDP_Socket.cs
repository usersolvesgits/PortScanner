using System.Net;
using System.Net.Sockets;

namespace PortScanner.core.models.sockets;

public class UDP_Socket : Base_Socket {
    /// <summary>
    /// Costruttore della classe <see cref="UDP_Socket"/>.
    /// </summary>
    /// <param name="Hostname">
    /// Stringa rappresentante l'hostname.
    /// </param>
    /// <param name="NumeroPorta">
    /// Indica il numero della porta della socket.
    /// </param>
    public UDP_Socket(string Hostname, int NumeroPorta)
                     : base(Hostname, NumeroPorta) {
        this.Hostname = Hostname;
        IPAddress[] indirizziPossibili = Dns.GetHostAddresses(Hostname);
        IPAddress = indirizziPossibili.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        if (IPAddress == null) {
            throw new InvalidOperationException($"Impossibile trarre un indirizzo IP dall'hostname {Hostname}");
        }
        this.NumeroPorta = NumeroPorta;
    }

    /// <summary>
    /// Costruttore della classe <see cref="UDP_Socket"/>.
    /// </summary>
    /// <param name="IPAddress">
    /// <see cref="IPAddress"/> del destinatario.
    /// </param>
    /// <param name="NumeroPorta">
    /// Indica il numero della porta della socket.
    /// </param>
    public UDP_Socket(IPAddress IPAddress, int NumeroPorta)
                     : base(IPAddress, NumeroPorta) {
        this.IPAddress = IPAddress;
        this.NumeroPorta = NumeroPorta;
    }


    public override void Connect() {
        throw new NotImplementedException();
    }

    public override void Connect(int timeout) {
        throw new NotImplementedException();
    }
}