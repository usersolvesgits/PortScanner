/*
 *  TODOS:
 *      -TCP_Socket:
 *          -Cambiare prop "StatoPorta" nella TCP_Socket da string a bool
 *           ed aggiornare metodo di visualizzazione nella datagrid
 *          -Imparare i metodi async per aggiungerne uno
 *      -MainWindow.xaml:
 *          -Aggiungere opzioni
 *      -MainWindow.xaml.cs:
 *          -Scan_EsportaJSON()
 *          -Aggiungere shortcuts per opzioni da aggiungere in xaml
 */


using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using PortScanner.Core.Models;
using Util = PortScanner.Core.Utils.Util;


namespace PortScanner {
    public partial class MainWindow : Window {
        const string urlSviluppatore = "https://github.com/usersolvesgits";
        const string urlAzienda = "https://www.sirius.to.it/";
        const int intervalloAggiornamentoProgressBar = 2;

        ObservableCollection<TCP_Socket> listaSockets;
        string target = String.Empty;
        int rangePortMin,
            rangePortMax;
        int porteTotali = 0,
            porteScansionate = 0;
        bool scansioneAttiva = false,
             richiestaFermataScansione = false;
        int timeoutScansione;

        public MainWindow() {
            InitializeComponent();
            listaSockets = new();
            dtgScansioni.ItemsSource = listaSockets;
            txtIPAddress.Focus();
            txtIPAddress.CaretIndex = txtIPAddress.Text.Length;
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            if (scansioneAttiva) {
                MessageBoxResult result = MessageBox.Show(
                    "Attenzione, sei sicuro di voler uscire?\nLa scansione non ha ancora finito",
                    "Attenzione!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No) {
                    e.Cancel = true;
                }
            }
        }

        private void Esci(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
        }

        private void Crediti_Sviluppatore(object sender, RoutedEventArgs e) {
            Util.OpenLink(urlSviluppatore);
        }
        private void Crediti_Azienda(object sender, RoutedEventArgs e) {
            Util.OpenLink(urlAzienda);
        }

        private void Tema_Chiaro(object sender, RoutedEventArgs e) {
#pragma warning disable WPF0001
            Application.Current.ThemeMode = ThemeMode.Light;
#pragma warning restore WPF0001
        }
        private void Tema_Scuro(object sender, RoutedEventArgs e) {
#pragma warning disable WPF0001
            Application.Current.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001
        }
        private void Tema_Sistema(object sender, RoutedEventArgs e) {
#pragma warning disable WPF0001
            Application.Current.ThemeMode = ThemeMode.System;
#pragma warning restore WPF0001
        }
        private void Tema_Vecchio(object sender, RoutedEventArgs e) {
#pragma warning disable WPF0001
            Application.Current.ThemeMode = ThemeMode.None;
#pragma warning restore WPF0001
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
        private bool CheckValidPort(string port) {
            if (string.IsNullOrWhiteSpace(port)) {
                return false;
            }

            int portInt = -1;
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

        /// <summary>
        /// Metodo contenente la logica principale per il loop di scansione.
        /// </summary>
        private void Scan_Scansione() {
            porteTotali = rangePortMax - rangePortMin + 1;
            porteScansionate = 0;
            Dispatcher.Invoke(() => 
                prbProgressoScan.Value = 0
            );
            scansioneAttiva = true;

            for (int currentPortNum = rangePortMin; currentPortNum <= rangePortMax; currentPortNum++) {
                TCP_Socket socket = new(target, currentPortNum);
                try {
                    socket.Connect();
                    Thread.Sleep(timeoutScansione);
                    porteScansionate++;
                } catch (ArgumentNullException ex) {
                    Debug.WriteLine(ex);
                    MessageBox.Show("ERRORE: Uno dei campi inseriti è nullo!",
                                    "Errore",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                } catch (ArgumentOutOfRangeException ex) {
                    Debug.WriteLine(ex);
                    MessageBox.Show("ERRORE: Uno dei campi inseriti è al di fuori dei limiti consentiti!",
                                    "Errore",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                    MessageBox.Show("ERRORE: Errore rilevato durante l'esecuzione del programma!",
                                    "Errore",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }
                //TODO -> migliorie fermata scansione
                if (richiestaFermataScansione) {
                    scansioneAttiva = false;
                    richiestaFermataScansione = false;
                    return;
                }

                Dispatcher.Invoke(() =>
                    listaSockets.Add(socket)
                );

                double progresso = (double)porteScansionate / porteTotali * 100;
                if (porteScansionate % intervalloAggiornamentoProgressBar == 0 ||
                    porteScansionate == porteTotali) {
                    Dispatcher.Invoke(() =>
                        prbProgressoScan.Value = progresso
                    );
                }
            }
            scansioneAttiva = false;
        }
        private void Scan_AvviaScansione(object sender, RoutedEventArgs e) {
            listaSockets.Clear();

            //////HOSTNAME / IP-ADDRESS//////
            if (string.IsNullOrWhiteSpace(txtIPAddress.Text)) {
                MessageBox.Show("Attenzione: Inserire un hostname o indirizzo IP valido!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtIPAddress.Clear();
                txtIPAddress.Focus();
                return;
            }
            target = txtIPAddress.Text.Trim().ToLower();

            //////PORTE//////
            if (!CheckValidPort(txtPortMin.Text)) {
                MessageBox.Show("Attenzione: Inserire un numero di porta valido!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPortMin.Clear();
                txtPortMin.Focus();
                return;
            }
            try {
                rangePortMin = int.Parse(txtPortMin.Text);
            } catch (Exception ex) {
                Debug.WriteLine($"Errore nella conversione della porta minima: {ex}");
                MessageBox.Show("Errore: Errore nel tentativo di conversione della porta minima, assicurarsi che il valore sia numerico!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPortMin.Clear();
                txtPortMin.Focus();
                return;
            }

            if (!CheckValidPort(txtPortMax.Text)) {
                MessageBox.Show("Attenzione: Inserire un numero di porta valido!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPortMax.Clear();
                txtPortMax.Focus();
                return;
            }
            try {
                rangePortMax = int.Parse(txtPortMax.Text);
            } catch (Exception ex) {
                Debug.WriteLine($"Errore nella conversione della porta massima: {ex}");
                MessageBox.Show("Errore: Errore nel tentativo di conversione della porta massima, assicurarsi che il valore sia numerico!",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                txtPortMax.Clear();
                txtPortMax.Focus();
                return;
            }

            if (rangePortMin > rangePortMax) {
                MessageBox.Show("Errore: Inserisci un intervallo di porte valido: porta minima a sinistra e porta massima a destra (es. 1000 – 2000).",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPortMin.Clear();
                txtPortMax.Clear();
                txtPortMin.Focus();
                return;
            }

            //////TIMEOUT//////
            string timeoutString = txtTimeout.Text.Trim();
            if (string.IsNullOrWhiteSpace(timeoutString)) {
                MessageBox.Show("Attenzione: Inserire un timeout in millisecondi!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtTimeout.Clear();
                txtTimeout.Focus();
                return;
            }
            bool check = int.TryParse(timeoutString, out timeoutScansione);
            if (!check) {
                MessageBox.Show("Attenzione: Inserire un timeout in millisecondi valido!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtTimeout.Clear();
                txtTimeout.Focus();
                return;
            }

            //////THREAD SCANSIONE//////
            Thread threadScansione;
            try {
                threadScansione = new(new ThreadStart(Scan_Scansione));
                threadScansione.Start();
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                MessageBox.Show("Errore: Errore rilevato durante l'avvio della scansione!",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
        }
        private void Scan_FermaScansione(object sender, RoutedEventArgs e) {
            //TODO -> migliorie fermata scansione
            richiestaFermataScansione = true;
        }
        private void Scan_EsportaCSV(object sender, RoutedEventArgs e) {
            if (listaSockets.Count == 0) {
                MessageBox.Show("Attenzione: Nessun elemento da esportare trovato!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSeparatoreCSV.Text)) {
                MessageBox.Show("Attenzione: Inserire un separatore per la formattazione in csv!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (txtSeparatoreCSV.Text.Length > 1) {
                MessageBox.Show("Attenzione: Il separatore per la formattazione CSV deve essere un singolo carattere!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            char separatoreCSV;
            try {
                separatoreCSV = char.Parse(txtSeparatoreCSV.Text);
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                MessageBox.Show("Errore: Errore durante la conversione del carattere separatore per la formattazione CSV!",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            SaveFileDialog dlg = new() {
                Title = "Esporta",
                Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*",
                DefaultExt = ".csv",
                AddExtension = true
            };

            string filePath = String.Empty;
            if (dlg.ShowDialog() == true) {
                filePath = dlg.FileName;
            }

            if (string.IsNullOrWhiteSpace(filePath)) {
                return;
            }

            try {
                using (StreamWriter writer = new StreamWriter(filePath)) {
                    for (int i = 0; i < listaSockets.Count; i++) {
                        if (i == 0) {
                            writer.WriteLine($"Target: {listaSockets[i].IPAddress?.ToString()}");
                            writer.WriteLine($"Stato della porta{separatoreCSV}Numero della porta{separatoreCSV}Servizio rilevato");
                        }
                        TCP_Socket socket = listaSockets[i];
                        writer.WriteLine(TCP_Socket.ToCSV(socket, separatoreCSV));
                    }
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                MessageBox.Show("Errore: Errore rilevato durante l'esportazione del file.",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
        }
        private void Scan_EsportaJSON(object sender, RoutedEventArgs e) {
            //TODO -> logica esporto JSON
        }

        private void FAQ_PortScanner(object sender, RoutedEventArgs e) {
            MessageBox.Show("Un port scanner è uno strumento usato in informatica e networking per analizzare le porte di rete di un dispositivo (come un computer o un server) per vedere quali sono aperte, chiuse o filtrate.",
                            "INFO",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        private void FAQ_Sirius(object sender, RoutedEventArgs e) {
            MessageBox.Show("Sirius è stata fondata nel 2000 come risultato della collaborazione tra l'incubatore di imprese del Politecnico di Torino e un team di esperti con l'obiettivo di sviluppare sistemi software avanzati per la gestione delle centrali elettriche e la trasmissione di energia.\r\n\r\nI membri fondatori di Sirius avevano già accumulato una notevole esperienza nel settore dell'automazione energetica sin dall'inizio degli anni '90, lavorando a stretto contatto con aziende rinomate del settore. \r\nQuesta competenza collettiva ha costituito la base per la crescita e il successo dell'azienda.\r\n\r\nNel corso degli anni, Sirius ha fornito con successo soluzioni a importanti operatori del mercato elettrotecnico. Dal 2006 abbiamo integrato le nostre soluzioni nei prodotti VireoX, sofisticati sistemi di gestione progettati specificamente per l'analisi e il controllo remoto degli impianti di energia rinnovabile.",
                            "INFO",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        private void FAQ_Funzionamento(object sender, RoutedEventArgs e) {
            MessageBox.Show("Uno scanner di porte controlla lo stato delle porte di un dispositivo in rete inviando richieste a diverse porte di un indirizzo IP. In base alla risposta ricevuta può determinare se una porta è aperta (servizio attivo), chiusa (nessun servizio) o filtrata (bloccata da firewall o sistemi di sicurezza).",
                            "INFO",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        private void FAQ_Legale(object sender, RoutedEventArgs e) {
            MessageBox.Show("L'uso di un port scanner è generalmente legale per analizzare la propria rete, effettuare test autorizzati o per scopi di studio. Tuttavia, scansionare sistemi senza autorizzazione può essere considerato attività sospetta o illegale in alcuni paesi. Utilizza sempre questi strumenti in modo responsabile.",
                            "INFO",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}