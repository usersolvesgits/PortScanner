using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace PortScanner.Core.Models {
    public class TCP_Socket {
        private int _numPorta;
        private static readonly IReadOnlyDictionary<int, string> _serviziConosciuti = new Dictionary<int, string>() {
            { 20, "FTP Data" }, { 21, "FTP" }, { 22, "SSH" }, { 23, "Telnet" }, { 25, "SMTP" },
            { 53, "DNS" },
            { 67, "DHCP" }, { 68, "DHCP" }, { 69, "TFTP" },
            { 80, "HTTP" },
            { 110, "POP3" }, { 119, "NNTP" },
            { 123, "NTP" },
            { 137, "NetBIOS" }, { 138, "NetBIOS" }, { 139, "NetBIOS" },
            { 143, "IMAP" },
            { 161, "SNMP" },
            { 179, "BGP" },
            { 389, "LDAP" },
            { 443, "HTTPS" },
            { 445, "SMB" },
            { 465, "SMTPS" },
            { 500, "ISAKMP" },
            { 587, "SMTP Submission" },
            { 636, "LDAPS" },
            { 989, "FTPS Data" },
            { 990, "FTPS" }, { 993, "IMAPS" }, { 995, "POP3S" },
            { 1433, "SQL Server" },
            { 1521, "Oracle DB" },
            { 2049, "NFS" },
            { 2082, "cPanel" }, { 2083, "cPanel SSL" },
            { 2181, "Zookeeper" },
            { 2483, "Oracle SSL" }, { 2484, "Oracle TCPS" },
            { 3000, "Dev Server" },
            { 3306, "MySQL" },
            { 3389, "RDP" },
            { 3690, "Subversion" },
            { 4444, "Metasploit" },
            { 5432, "PostgreSQL" },
            { 5601, "Kibana" },
            { 5672, "RabbitMQ" },
            { 5900, "VNC" },
            { 5985, "WinRM" }, { 5986, "WinRM HTTPS" },
            { 6379, "Redis" },
            { 6667, "IRC" },
            { 7001, "WebLogic" }, { 7002, "WebLogic SSL" },
            { 8000, "HTTP Alt" }, { 8008, "HTTP Alt" },
            { 8080, "HTTP Proxy" }, { 8081, "HTTP Alt" },
            { 8443, "HTTPS Alt" },
            { 9000, "SonarQube" },
            { 9042, "Cassandra" },
            { 9092, "Kafka" },
            { 9200, "Elasticsearch" },
            { 9418, "Git" },
            { 27017, "MongoDB" }
        };

        /// <summary>
        /// Indica l'hostname del destinatario.
        /// </summary>
        public string Hostname { get; private set; }

        /// <summary>
        /// Indica l'indirizzo IP del destinatario.
        /// </summary>
        public IPAddress IPAddress { get; private set; }

        /// <summary>
        /// Indica il numero della porta.
        /// </summary>
        public int NumeroPorta {
            get { return _numPorta; }
            set {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort) {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Il valore inserito deve essere un numero compreso tra {IPEndPoint.MinPort} e {IPEndPoint.MaxPort}!");
                } else {
                    if (_serviziConosciuti.TryGetValue(value, out string Servizio)) {
                        this.Servizio = Servizio;
                    } else {
                        this.Servizio = "Sconosciuto";
                    }
                    _numPorta = value;
                }
            }
        }

        /// <summary>
        /// Indica lo stato di connessione della porta.
        /// Se la stringa è "Aperta", allora la porta è aperta.
        /// Se la stringa è "Chiusa", allora la porta è chiusa.
        /// </summary>
        public string StatoPorta { get; private set; }

        /// <summary>
        /// Indica che tipo di servizio viene usato in quale porta.
        /// </summary>
        public string Servizio { get; private set; }

        public TCP_Socket(string Hostname, int NumeroPorta) {
            this.Hostname = Hostname;
            IPAddress[] addresses = Dns.GetHostAddresses(Hostname);
            this.IPAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            this.NumeroPorta = NumeroPorta;
        }

        public TCP_Socket(IPAddress IPAddress, int NumeroPorta) {
            this.IPAddress = IPAddress;
            this.NumeroPorta = NumeroPorta;
        }

        public override string ToString() {
            return $"Indirizzo IP destinatario: {IPAddress?.ToString() ?? Hostname};\t" +
                   $"Numero della porta: {NumeroPorta};\t" +
                   $"Stato della porta: {StatoPorta};\t" +
                   $"Servizio: {Servizio}.";
        }

        /// <summary>
        /// Restituisce la rappresentazione della porta in formato CSV.
        /// </summary>
        /// <param name="separatore">
        /// Carattere utilizzato come separatore tra i campi del file.
        /// </param>
        /// <returns>
        /// Una stringa che rappresenta lo stato della porta nel formato:
        /// <c>IsOpen,NumeroPorta,Servizio</c>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, char separatore) {
            return $"{socket.StatoPorta}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
        }

        /// <summary>
        /// Restituisce la rappresentazione della porta in formato CSV 
        /// con la possibilità di includere anche l'indirizzo IP.
        /// </summary>
        /// <param name="ShowIPAddress">
        /// Se <see langword="true"/>, la stringa risultante includerà anche 
        /// l'indirizzo IP del destinatario come primo campo.
        /// </param>
        /// <param name="separatore">
        /// Carattere utilizzato come separatore tra i campi del file.
        /// </param>
        /// <returns>
        /// Una stringa che rappresenta lo stato della porta nel formato:
        /// <c>IsOpen,NumeroPorta,Servizio</c>
        /// oppure
        /// <c>IPAddres,IsOpen,NumeroPorta,Servizio</c>
        /// se <paramref name="ShowIPAddress"/> è <see langword="true"/>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, char separatore, bool ShowIPAddress) {
            if (ShowIPAddress) {
                return $"{socket.IPAddress?.ToString()}{separatore}{socket.StatoPorta}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
            }
            return $"{socket.StatoPorta}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
        }

        /// <summary>
        /// Formatta le informazioni di un oggetto TCP_Socket in una stringa JSON.
        /// </summary>
        /// <param name="socket">Un oggetto TCP_Socket da convertire in formato JSON.</param>
        /// <returns>Una stringa contenente la rappresentazione JSON del TCP_Socket specificato.</returns>
        public static string ToJSON(TCP_Socket socket) {
            JsonSerializerOptions opzioni = new() {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(socket, opzioni);
        }

        /// <summary>
        /// Metodo usato per controllare lo stato delle porte.
        /// </summary>
        public void Connect() {
            using (TcpClient TcpC = new()) {
                try {
                    TcpC.Connect(IPAddress, NumeroPorta);
                    StatoPorta = "Aperta";
                }  catch (SocketException ex) {
                    Debug.WriteLine($"Porta numero {NumeroPorta} non raggiungibile!\n{ex}");
                    StatoPorta = "Chiusa";
                }
            }
        }
    }
}