/*
 *  TODOS:
 *      -MainWindow.xaml.cs:
 *          -Esportazione_JSON()
 *          -Rifare il metodo di fermata della scansione
 */


using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using PortScanner.Core.Models;
using System.Windows.Input;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using Util = PortScanner.Core.Utils.Util;
using System.Net;


namespace PortScanner {
    public partial class MainWindow : Window {
        const string urlSviluppatore = "https://github.com/usersolvesgits";
        const string urlAzienda = "https://www.sirius.to.it/";
        const int intervalloAggiornamentoProgressBar = 2;
        const int MinPort = IPEndPoint.MinPort;
        const int MaxPort = IPEndPoint.MaxPort;

        enum OpzioniFiltri {
            Nessuno,
            PorteAperte,
            PorteChiuse,
            ServiziRilevati,
        }
        enum OpzioniOrdinamenti {
            Nessuno,
            PorteAperteChiuse,
            PorteChiuseAperte,
        }

        ObservableCollection<TCP_Socket> listaSockets = new();
        string target = String.Empty;
        int rangePortMin,
            rangePortMax;
        int porteTotali = 0, porteTotaliPrecedenti = 0,
            porteAperte = 0,
            porteScansionate = 0;
        long durataScansione_l = 0;
        bool scansioneAttiva = false,
             richiestaFermataScansione = false;
        int timeoutScansione;

        public MainWindow() {
            InitializeComponent();
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
        private void PortScanner_ShortCuts(object sender, KeyEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                switch (e.Key) {
                    case Key.Q:
                        Esci(null, null);
                        e.Handled = true;
                        break;
                    case Key.R:
                        Scan_AvviaScansione(null, null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        Scan_FermaScansione(null, null);
                        e.Handled = true;
                        break;
                    case Key.L:
                        Tema_Chiaro(null, null);
                        e.Handled = true;
                        break;
                    case Key.D:
                        Tema_Scuro(null, null);
                        e.Handled = true;
                        break;
                }
            }
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt)) {
                switch (e.Key) {
                    case Key.A:
                        Crediti_Azienda(null, null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        Crediti_Sviluppatore(null, null);
                        e.Handled = true;
                        break;
                    case Key.C:
                        Esportazione_CSV(null, null);
                        e.Handled = true;
                        break;
                    case Key.J:
                        Esportazione_JSON(null, null);
                        e.Handled = true;
                        break;
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
        /// Metodo contenente la logica principale per il loop di scansione.
        /// </summary>
        private void Scan_Scansione() {
            porteTotali = rangePortMax - rangePortMin + 1;
            Dispatcher.Invoke(() => {
                txtTotalePorte.Text = txtTotalePorte.Text.Replace(porteTotaliPrecedenti.ToString(), porteTotali.ToString());
                txtDurata.Text = txtDurata.Text.Replace(durataScansione_l.ToString(), "0");
                txtPorteAperte.Text = txtPorteAperte.Text.Replace(porteAperte.ToString(), "0");
                prbProgressoScan.Value = 0;
            });
            porteScansionate = 0;
            porteAperte = 0;
            int porteApertePrecedente = 0;
            porteTotaliPrecedenti = porteTotali;
            scansioneAttiva = true;

            Stopwatch durataScansione = new();
            durataScansione.Start();

            for (int currentPortNum = rangePortMin; currentPortNum <= rangePortMax; currentPortNum++) {
                TCP_Socket socket = new(target, currentPortNum);
                try {
                    socket.Connect(timeoutScansione);
                    if (socket.IsOpen) {
                        porteAperte++;
                    }
                    porteScansionate++;
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                    durataScansione.Stop();
                    Dispatcher.Invoke(() => {
                        txtDurata.Text = txtDurata.Text.Replace(durataScansione_l.ToString(), "0");
                        txtPorteAperte.Text = txtPorteAperte.Text.Replace(porteAperte.ToString(), "0");
                        prbProgressoScan.Value = 0;
                    });
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

                Dispatcher.BeginInvoke(() => {
                    listaSockets.Add(socket);
                    txtPorteAperte.Text = txtPorteAperte.Text.Replace(porteApertePrecedente.ToString(), porteAperte.ToString());
                });

                if (socket.IsOpen) {
                    porteApertePrecedente++;
                }

                double progresso = (double)porteScansionate / porteTotali * 100;
                if (porteScansionate % intervalloAggiornamentoProgressBar == 0 ||
                    porteScansionate == porteTotali) {
                    Dispatcher.Invoke(() => {
                        prbProgressoScan.Value = progresso;
                    });
                }
            }
            durataScansione.Stop();
            durataScansione_l = durataScansione.ElapsedMilliseconds;
            Dispatcher.Invoke(() => {
                txtDurata.Text = txtDurata.Text.Replace("0", durataScansione_l.ToString());
            });
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
            if (!Util.CheckValidPort(txtPortMin.Text)) {
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
                MessageBox.Show("Errore: Errore nel tentativo di conversione della porta minima, assicurarsi che il valore sia numerico!", 
                                "Errore", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                txtPortMin.Clear();
                txtPortMin.Focus();
                return;
            }

            if (rangePortMin < MinPort) {
                MessageBox.Show($"Attenzione: Inserire un numero di porta maggiore o uguale a {MinPort}!", 
                                "Attenzione", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning);
                txtPortMin.Clear();
                txtPortMin.Focus();
                return;
            }

            if (!Util.CheckValidPort(txtPortMax.Text)) {
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

            if (rangePortMax > MaxPort) {
                MessageBox.Show($"Attenzione: Inserire un numero di porta maggiore di {MaxPort}!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPortMax.Clear();
                txtPortMax.Focus();
                return;
            }

            if (rangePortMin > rangePortMax) {
                MessageBox.Show("Errore: Inserisci un intervallo di porte valido: porta minima a sinistra e porta massima a destra (es. 1000 – 2000).",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
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
                MessageBox.Show("Attenzione: Inserire un timeout in millisecondi valido come numero intero!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtTimeout.Clear();
                txtTimeout.Focus();
                return;
            }
            if (timeoutScansione <= 0) {
                MessageBox.Show("Attenzione: Inserire un valore maggiore di 0!",
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

        private void Esportazione_CSV(object sender, RoutedEventArgs e) {
            if (dtgScansioni.Items.Count == 0) {
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
                Filter = "CSV File (*.csv)|*.csv",
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
                    for (int i = 0; i < dtgScansioni.Items.Count; i++) {
                        var socket = dtgScansioni.Items[i] as TCP_Socket;
                        if (i == 0) {
                            writer.WriteLine($"Target: {socket.IPAddress?.ToString()}");
                            writer.WriteLine($"Stato della porta{separatoreCSV}Numero della porta{separatoreCSV}Servizio rilevato");
                        }
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
        private void Esportazione_JSON(object sender, RoutedEventArgs e) {
            //TODO -> logica esportazione JSON
        }

        private void OpzioniScansione_CambioFiltro(object sender, SelectionChangedEventArgs e) {
            if (listaSockets.Count == 0) {
                return;
            }

            switch ((OpzioniFiltri)cmbFiltri.SelectedIndex) {
                case OpzioniFiltri.Nessuno:
                    dtgScansioni.ItemsSource = listaSockets;
                    break;
                case OpzioniFiltri.PorteAperte:
                    dtgScansioni.ItemsSource = listaSockets;
                    ObservableCollection<TCP_Socket> listaPorteAperte = new();
                    foreach (TCP_Socket socket in listaSockets) {
                        if (socket.IsOpen) {
                            listaPorteAperte.Add(socket);
                        }
                    }
                    dtgScansioni.ItemsSource = listaPorteAperte;
                    break;
                case OpzioniFiltri.PorteChiuse:
                    ObservableCollection<TCP_Socket> listaPorteChiuse = new();
                    foreach (TCP_Socket socket in listaSockets) {
                        if (!socket.IsOpen) {
                            listaPorteChiuse.Add(socket);
                        }
                    }
                    dtgScansioni.ItemsSource = listaPorteChiuse;
                    break;
                case OpzioniFiltri.ServiziRilevati:
                    ObservableCollection<TCP_Socket> listaServiziRilevati = new();
                    foreach (TCP_Socket socket in listaSockets) {
                        if (TCP_Socket.ServiziConosciuti.ContainsKey(socket.NumeroPorta)) {
                            listaServiziRilevati.Add(socket);
                        }
                    }
                    dtgScansioni.ItemsSource = listaServiziRilevati;
                    break;
            }
        }
        private void OpzioniScansione_CambioOrdinamento(object sender, SelectionChangedEventArgs e) {
            if (listaSockets.Count == 0) {
                return;
            }

            switch ((OpzioniOrdinamenti)cmbOrdina.SelectedIndex) {
                case OpzioniOrdinamenti.Nessuno:
                    listaSockets = new(listaSockets.OrderBy(s => s.NumeroPorta));
                    break;
                case OpzioniOrdinamenti.PorteAperteChiuse:
                    listaSockets = new(listaSockets.OrderByDescending(s => s.IsOpen));
                    break;
                case OpzioniOrdinamenti.PorteChiuseAperte:
                    listaSockets = new(listaSockets.OrderBy(s => s.IsOpen));
                    break;
            }
            dtgScansioni.ItemsSource = listaSockets;
        }
        private void OpzioniScansione_MouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
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