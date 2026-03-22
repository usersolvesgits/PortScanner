using System.Diagnostics;
using System.Net;
using System.Windows;

namespace PortScanner.Core.Utils {
    public static class Util {
        /// <summary>
        /// Apre l'url in input sul browser di default dell'utente.
        /// </summary>
        /// <param name="url">Indirizzo da aprire.</param>
        public static void OpenLink(string url) {
            try {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            } catch (Exception ex) {
                Debug.WriteLine($"Errore nell'aprire l'url '{url}': {ex}");
                MessageBox.Show("Impossibile accedere al sito.",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Verifica se la stringa fornita rappresenta una porta valida.
        /// La porta deve essere un numero compreso tra <see cref="IPEndPoint.MinPort"/> e <see cref="IPEndPoint.MaxPort"/>.
        /// </summary>
        /// <param name="port">
        /// Stringa che rappresenta il numero di porta da controllare.
        /// </param>
        /// <returns>
        /// <c>true</c> se la porta è numerica e rientra nell'intervallo valido delle porte TCP/UDP;
        /// <c>false</c> se la stringa è vuota, non numerica o fuori dall'intervallo consentito.
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

            if (portInt < IPEndPoint.MinPort || portInt > IPEndPoint.MaxPort) {
                return false;
            }

            return true;
        }
    }
}