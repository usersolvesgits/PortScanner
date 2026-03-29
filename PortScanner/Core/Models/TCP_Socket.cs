using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;

namespace PortScanner.Core.Models {
    public class TCP_Socket {
        private int _numPorta;


        /// <summary>
        /// Specifica la prima porta da dove è possibile iniziare una scansione
        /// </summary>
        public const int PrimaPorta = 1;
        /// <summary>
        /// Specifica l'ultima porta da dove è possibile iniziare una scansione
        /// </summary>
        public const int UltimaPorta = 65535;


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
        public string? Hostname { get; private set; }

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
        /// Indica lo stato di connessione di una porta come valore booleano.
        /// Se impostato su <see langword="true"/> la porta è aperta.
        /// Se impostato su <see langword="false"/> la porta è chiusa.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Indica tutti i possibili stati di una porta.<br/>
        /// <see cref="Aperta"/> se la porta risulta essere aperta.<br/>
        /// <see cref="Chiusa"/> se la porta risulta essere chiusa.<br/>
        /// <see cref="Filtrata"/> se la porta risulta essere filtrata da qualche firewall o simili.
        /// </summary>
        public enum StatoPorta {
            Aperta,
            Filtrata,
            Chiusa
        }
        /// <summary>
        /// Indica lo stato della porta prendendo come base le varie opzioni dell' <see langword="enum"/> <see cref="StatoPorta"/>.
        /// </summary>
        public StatoPorta Stato { get; private set; }

        /// <summary>
        /// Indica che tipo di servizio viene usato in quale porta.
        /// </summary>
        public string Servizio { get; private set; }



        /// <summary>
        /// Costruttore della classe <see cref="TCP_Socket"/>.
        /// </summary>
        /// <param name="Hostname">
        /// Stringa rappresentante l'hostname.
        /// </param>
        /// <param name="NumeroPorta">
        /// Indica il numero della porta della socket.
        /// </param>
        public TCP_Socket(string Hostname, int NumeroPorta) {
            this.Hostname = Hostname;
            IPAddress[] addresses = Dns.GetHostAddresses(Hostname);
            IPAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            this.NumeroPorta = NumeroPorta;
        }

        /// <summary>
        /// Costruttore della classe <see cref="TCP_Socket"/>.
        /// </summary>
        /// <param name="IPAddress">
        /// <see cref="System.Net.IPAddress"/> del destinatario.
        /// </param>
        /// <param name="NumeroPorta">
        /// Indica il numero della porta della socket.
        /// </param>
        public TCP_Socket(IPAddress IPAddress, int NumeroPorta) {
            this.IPAddress = IPAddress;
            this.NumeroPorta = NumeroPorta;
        }


        public override string ToString() {
            return $"Indirizzo IP destinatario: {IPAddress?.ToString() ?? Hostname};\t" +
                   $"Numero della porta: {NumeroPorta};\t" +
                   $"Stato della porta: {Stato};\t" +
                   $"Servizio: {Servizio}.";
        }


        /// <summary>
        /// Verifica se la stringa fornita rappresenta una porta valida.
        /// La porta deve essere un numero compreso tra <see cref="PrimaPorta"/> e <see cref="UltimaPorta"/>.
        /// </summary>
        /// <param name="port">
        /// Stringa che rappresenta il numero di porta da controllare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> se la porta è numerica e rientra nell'intervallo valido delle porte TCP/UDP;
        /// <see langword="false"/> se la stringa è vuota, non numerica o fuori dall'intervallo consentito.
        /// </returns>
        public static bool CheckValidPort(string port) {
            if (string.IsNullOrWhiteSpace(port)) {
                return false;
            }

            int portInt;
            try {
                portInt = int.Parse(port);
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                MessageBox.Show("Attenzione: Assicurarsi che il valore inserito sia numerico!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return false;
            }

            if (portInt < PrimaPorta || portInt > UltimaPorta) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restituisce la rappresentazione della porta in formato CSV.
        /// </summary>
        /// <param name="separatore">
        /// Carattere utilizzato come separatore tra i campi del file.
        /// </param>
        /// <returns>
        /// Una stringa che rappresenta lo stato della porta nel formato:
        /// <c><see cref="Stato"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, char separatore) {
            return $"{socket.Stato}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
        }

        /// <summary>
        /// Restituisce la rappresentazione della porta in formato CSV.
        /// </summary>
        /// <param name="separatore">
        /// Stringa utilizzata come separatore tra i campi del file.
        /// </param>
        /// <returns>
        /// Una stringa che rappresenta lo stato della porta nel formato:
        /// <c><see cref="Stato"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, string separatore) {
            return $"{socket.Stato}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
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
        /// <c><see cref="Stato"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>
        /// oppure
        /// <c><see cref="IPAddress">,<see cref="Stato"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>
        /// se <paramref name="ShowIPAddress"/> è <see langword="true"/>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, char separatore, bool ShowIPAddress) {
            if (ShowIPAddress) {
                return $"{socket.IPAddress?.ToString()}{separatore}{socket.Stato}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
            }
            return $"{socket.Stato}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
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
        /// Stringa utilizzata come separatore tra i campi del file.
        /// </param>
        /// <returns>
        /// Una stringa che rappresenta lo stato della porta nel formato:
        /// <c><see cref="Stato"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>
        /// oppure
        /// <c><see cref="IPAddress">,<see cref="Stato"/>,<see cref="NumeroPorta"/>,<see cref="Servizio"/></c>
        /// se <paramref name="ShowIPAddress"/> è <see langword="true"/>.
        /// </returns>
        public static string ToCSV(TCP_Socket socket, string separatore, bool ShowIPAddress) {
            if (ShowIPAddress) {
                return $"{socket.IPAddress?.ToString()}{separatore}{socket.Stato}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
            }
            return $"{socket.Stato}{separatore}{socket.NumeroPorta}{separatore}{socket.Servizio}";
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
            using TcpClient TcpC = new();
            try {
                TcpC.Connect(IPAddress, NumeroPorta);
                Stato = StatoPorta.Aperta;
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
            }
        }

        /// <summary>
        /// Metodo usato per controllare lo stato delle porte.
        /// </summary>
        /// <param name="timeout">
        /// Numero di millisecondi per decretare se una porta è aperta o meno.
        /// </param>
        public void Connect(int timeout) {
            using TcpClient TcpC = new();
            try {
                if (!TcpC.ConnectAsync(IPAddress, NumeroPorta).Wait(timeout)) {
                    Stato = StatoPorta.Filtrata; return;
                } else {
                    Stato = StatoPorta.Aperta;
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
            } catch (SocketException ex) {
                Debug.WriteLine(ex);
                Stato = StatoPorta.Chiusa;
            }
        }
    }
}