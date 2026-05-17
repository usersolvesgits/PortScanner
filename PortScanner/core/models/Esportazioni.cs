using PortScanner.core.models.sockets;
using System.IO;

namespace PortScanner.core.models {
    public static class Esportazioni {
        public static void ToCSV(string path, string separatoreCSV, DateTime tempoScansione, long durataScansione_l, List<Base_Socket> listaSockets) {
            using StreamWriter writer = new(path);
            for (int i = 0; i < listaSockets.Count(); i++) {
                var socket = listaSockets[i];
                if (i == 0) {
                    writer.WriteLine($"Scansione avviata: [{tempoScansione.ToString("dd/MM/yyyy HH:mm:ss")}]");
                    writer.WriteLine($"Durata della scansione: [{durataScansione_l}]ms");
                    writer.WriteLine($"Target: {socket.IPAddress?.ToString()}");
                    writer.WriteLine($"Stato della porta{separatoreCSV}Numero della porta{separatoreCSV}Servizio rilevato");
                }
                writer.WriteLine(Base_Socket.ToCSV(socket, separatoreCSV));
            }
        }
        public static void ToJSON() {

        }
    }
}
