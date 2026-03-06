using System.Diagnostics;
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
    }
}