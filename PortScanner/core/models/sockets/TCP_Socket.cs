using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PortScanner.core.models.sockets {
    public class TCP_Socket : Base_Socket {
        /// <summary>
        /// Costruttore della classe <see cref="TCP_Socket"/>.
        /// </summary>
        /// <param name="Hostname">
        /// Stringa rappresentante l'hostname.
        /// </param>
        /// <param name="NumeroPorta">
        /// Indica il numero della porta della socket.
        /// </param>
        public TCP_Socket(string Hostname, int NumeroPorta)
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
        /// Costruttore della classe <see cref="TCP_Socket"/>.
        /// </summary>
        /// <param name="IPAddress">
        /// <see cref="IPAddress"/> del destinatario.
        /// </param>
        /// <param name="NumeroPorta">
        /// Indica il numero della porta della socket.
        /// </param>
        public TCP_Socket(IPAddress IPAddress, int NumeroPorta)
                         : base(IPAddress, NumeroPorta) {
            this.IPAddress = IPAddress;
            this.NumeroPorta = NumeroPorta;
        }


        /// <summary>
        /// Metodo usato per controllare lo stato delle porte.
        /// </summary>
        public override void Connect() {
            using TcpClient TcpC = new();
            try {
                TcpC.Connect(IPAddress, NumeroPorta);
                Stato = StatoPorta.Aperta;
                IsOpen = true;
            } catch (SocketException ex) {
                Debug.WriteLine(ex);
                switch (ex.SocketErrorCode) {
                    case SocketError.ConnectionRefused:
                        Stato = StatoPorta.Chiusa;
                        break;
                    case SocketError.TimedOut:
                        Stato = StatoPorta.Filtrata;
                        break;
                    default:
                        Stato = StatoPorta.Filtrata;
                        break;
                }
                IsOpen = false;
            }
        }

        /// <summary>
        /// Metodo usato per controllare lo stato delle porte.
        /// </summary>
        /// <param name="timeout">
        /// Numero di millisecondi per decretare se una porta è aperta o meno.
        /// </param>
        public override void Connect(int timeout) {
            using TcpClient TcpC = new();
            try {
                if (!TcpC.ConnectAsync(IPAddress, NumeroPorta).Wait(timeout)) {
                    Stato = StatoPorta.Filtrata;
                    IsOpen = false;
                } else {
                    Stato = StatoPorta.Aperta;
                    IsOpen = true;
                }
            } catch (AggregateException ex) {
                Debug.WriteLine(ex);
                if (ex.InnerException is SocketException) {
                    var eccezione = (SocketException)ex.InnerException;
                    switch (eccezione.SocketErrorCode) {
                        case SocketError.ConnectionRefused:
                            Stato = StatoPorta.Chiusa;
                            break;
                        case SocketError.TimedOut:
                            Stato = StatoPorta.Filtrata;
                            break;
                        default:
                            Stato = StatoPorta.Filtrata;
                            break;
                    }
                } else {
                    Stato = StatoPorta.Filtrata;
                }
                IsOpen = false;
            } catch (SocketException ex) {
                Debug.WriteLine(ex);
                Stato = StatoPorta.Chiusa;
                IsOpen = false;
            }
        }
    }
}