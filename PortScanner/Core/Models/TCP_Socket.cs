using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace PortScanner.Core.Models {
    public class TCP_Socket {
        private int _numPorta;
        private bool _statoPorta;

        /// <summary>
        /// Dizionario dove il numero della porta corrisponde ad un servizio noto.
        /// </summary>
        public static readonly IReadOnlyDictionary<int, string> ServiziConosciuti = new Dictionary<int, string>() {
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
            { 194, "IRC" },
            { 389, "LDAP" },
            { 443, "HTTPS" },
            { 445, "SMB" },
            { 465, "SMTPS" },
            { 500, "ISAKMP" },
            { 514, "Syslog" }, { 515, "LPD" },
            { 520, "RIP" },
            { 587, "SMTP Submission" },
            { 636, "LDAPS" },
            { 989, "FTPS Data" },
            { 990, "FTPS" }, { 993, "IMAPS" }, { 995, "POP3S" },
            { 1433, "SQL Server" },
            { 1521, "Oracle DB" },
            { 2049, "NFS" },
            { 2082, "cPanel" }, { 2083, "cPanel SSL" }, { 2086, "WHM" }, { 2087, "WHM SSL" },
            { 2181, "Zookeeper" },
            { 2375, "Docker" }, { 2376, "Docker SSL" },
            { 2483, "Oracle SSL" }, { 2484, "Oracle TCPS" },
            { 3000, "Dev Server" },
            { 3306, "MySQL" },
            { 3389, "RDP" },
            { 3690, "Subversion" },
            { 4000, "Dev Server" },
            { 4444, "Metasploit" },
            { 4567, "Sinatra" },
            { 5000, "Flask Dev Server" },
            { 5060, "SIP" }, { 5061, "SIP TLS" },
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
            { 9999, "Debug/Dev" },
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
            private set {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort) {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Il valore inserito deve essere un numero compreso tra {IPEndPoint.MinPort} e {IPEndPoint.MaxPort}!");
                } else {
                    if (ServiziConosciuti.TryGetValue(value, out string Servizio)) {
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
        /// Se la stringa è "Chiusa", allora la porta è chiusa o c'è un problema nella connessione verso tale porta.
        /// </summary>
        public string StatoPorta { get; private set; }

        /// <summary>
        /// Indica lo stato di connessione di una porta come valore booleano.
        /// Se impostato su <see langword="true"/> la porta è aperta.
        /// Se impostato su <see langword="false"/> la porta è chiusa.
        /// </summary>
        public bool IsOpen {
            get { return _statoPorta; }
            private set {
                if (value) {
                    _statoPorta = true;
                    StatoPorta = "Aperta";
                } else {
                    _statoPorta = false;
                    StatoPorta = "Chiusa";
                }
            }
        }

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
        /// <c><see cref="IsOpen"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>.
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
        /// <c><see cref="IsOpen"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>
        /// oppure
        /// <c><see cref="IPAddress">,<see cref="IsOpen"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>
        /// se <paramref name="ShowIPAddress"/> è <see langword="true"/>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, char separatore, bool ShowIPAddress) {
            if (ShowIPAddress) {
                return $"{socket.IPAddress?.ToString()}{separatore}{socket.StatoPorta}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
            }
            return $"{socket.StatoPorta}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
        }

        /// <summary>
        /// Formatta le informazioni di un oggetto <see cref="TCP_Socket"/> in una stringa JSON.
        /// </summary>
        /// <param name="socket">Un oggetto <see cref="TCP_Socket"/> da convertire in formato JSON.</param>
        /// <returns>Una stringa contenente la rappresentazione JSON del <see cref="TCP_Socket"/> specificato.</returns>
        public static string ToJSON(TCP_Socket socket) {
            return JsonSerializer.Serialize(socket);
        }

        /// <summary>
        /// Metodo usato per controllare lo stato delle porte.
        /// </summary>
        public void Connect() {
            using (TcpClient TcpC = new()) {
                try {
                    TcpC.Connect(IPAddress, NumeroPorta);
                    IsOpen = true;
                } catch (SocketException ex) {
                    Debug.WriteLine($"Porta numero {NumeroPorta} non raggiungibile!\n{ex}");
                    IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Metodo usato per controllare lo stato delle porte.
        /// </summary>
        public void Connect(int timeout) {
            using (TcpClient TcpC = new()) {
                try {
                    if (TcpC.ConnectAsync(IPAddress, NumeroPorta).Wait(timeout)) {
                        IsOpen = true;
                    } else {
                        IsOpen = false;
                    }
                } catch (SocketException ex) {
                    Debug.WriteLine($"Porta numero {NumeroPorta} non raggiungibile!\n{ex}");
                    IsOpen = false;
                }
            }
        }
    }
}