/*
 *  TODOS:
 *      GENERAL:
 *          -~Permettere scansioni UDP~
 *          -Prima release:
 *              -Aggiungere "watermark"
 *              -Aggiungere il counter della versione attuale dell'applicazione
 *      -TCP_Socket:
 *          -Sistemare logica di connessione per un vero controllo tra filtrate e chiuse
 *      -MainWindow.xaml.cs:
 *          -Rifare il metodo di fermata della scansione
 */


using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using AddressFamily = System.Net.Sockets.AddressFamily;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using PortScanner.core.models;
using PortScanner.core.models.sockets;

namespace PortScanner {
    public partial class MainWindow : Window {
        const string URL_SVILUPPATORE = "https://github.com/usersolvesgits";
        const string URL_AZIENDA = "https://www.sirius.to.it/";
        const int INTERVALLO_AGGIORNAMENTO_PROGRESSBAR = 2;

        enum OpzioniFiltri {
            Nessuno,
            PorteAperte,
            PorteChiuse,
            PorteFiltrate,
            ServiziRilevati
        }
        enum OpzioniOrdinamenti {
            Nessuno,
            AperteFiltrateChiuse,
            ChiuseFiltrateAperte
        }
        enum OpzioniTipoScansione {
            TCP,
            UDP
        }
        ObservableCollection<Base_Socket> listaSockets = new();
        DateTime dataScansione;
        IPAddress targetIPAddress;
        int rangePortMin,
            rangePortMax;
        int porteTotali = 0,
            porteAperte = 0,
            porteScansionate = 0;
        long durataScansione_l = 0;
        bool scansioneAttiva = false,
             richiestaFermataScansione = false;
        int timeoutScansione;
        OpzioniTipoScansione tipoScansione = OpzioniTipoScansione.TCP;

        public MainWindow() {
            InitializeComponent();
            dtgScansioni.ItemsSource = listaSockets;
            txtIPAddress.Focus();
            txtIPAddress.CaretIndex = txtIPAddress.Text.Length;
        }

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
                } else {
                    //TODO -> migliorie fermata scansione
                    Scan_FermaScansione(null, null);
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
                }
            }
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt)) {
                switch (e.Key) {
                    case Key.L:
                        Tema_Chiaro(null, null);
                        e.Handled = true;
                        break;
                    case Key.D:
                        Tema_Scuro(null, null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        Tema_Sistema(null, null);
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
            Application.Current.Shutdown();
        }

        private void Crediti_Sviluppatore(object sender, RoutedEventArgs e) {
            OpenLink(URL_SVILUPPATORE);
        }
        private void Crediti_Azienda(object sender, RoutedEventArgs e) {
            OpenLink(URL_AZIENDA);
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
            porteScansionate = 0;
            porteAperte = 0;
            int porteApertePrecedente = 0;
            scansioneAttiva = true;
            Stopwatch durataScansione = new();
            durataScansione.Start();
            dataScansione = DateTime.Now;
            double progressoScansione = 0;
            Dispatcher.Invoke(() => {
                cmbTipoScansione.IsEnabled = false;
                prbProgressoScan.Value = progressoScansione;
            });

            for (int currentPortNum = rangePortMin; currentPortNum <= rangePortMax; currentPortNum++) {
                Base_Socket socket;
                string porteApertePrecedente_s = porteApertePrecedente.ToString();
                try {
                    if (tipoScansione == OpzioniTipoScansione.TCP) {
                        socket = new TCP_Socket(targetIPAddress, currentPortNum);
                    } else {
                        socket = new UDP_Socket(targetIPAddress, currentPortNum);
                    }

                    socket.Connect(timeoutScansione);
                    if (socket.IsOpen) {
                        porteAperte++;
                    }

                    Dispatcher.BeginInvoke(() => {
                        listaSockets.Add(socket);
                        txtPorteAperte.Text = txtPorteAperte.Text.Replace(porteApertePrecedente_s, porteAperte.ToString());
                    });

                    if (socket.IsOpen) {
                        porteApertePrecedente++;
                    }

                    porteScansionate++;
                    progressoScansione = (double)porteScansionate / porteTotali * 100;
                    if (porteScansionate % INTERVALLO_AGGIORNAMENTO_PROGRESSBAR == 0 ||
                        porteScansionate == porteTotali) {
                        Dispatcher.Invoke(() => {
                            prbProgressoScan.Value = progressoScansione;
                        });
                    }
                } catch (NotImplementedException e) {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e);
                    scansioneAttiva = false;
                    durataScansione.Stop();
                    durataScansione_l = durataScansione.ElapsedMilliseconds;
                    Dispatcher.Invoke(() => {
                        cmbTipoScansione.IsEnabled = true;
                        prbProgressoScan.Value = progressoScansione;
                        txtDurata.Text = txtDurata.Text.Replace("0", durataScansione_l.ToString());
                        txtPorteAperte.Text = txtPorteAperte.Text.Replace("0", porteAperte.ToString());
                        txtPorteScansionate.Text = txtPorteScansionate.Text.Replace("0", porteScansionate.ToString());
                    });
                    MessageBox.Show("Attenzione: Scansione UDP non ancora implementata, cambiare modalità di scansione!",
                                    "Warning",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex);
                    scansioneAttiva = false;
                    durataScansione.Stop();
                    durataScansione_l = durataScansione.ElapsedMilliseconds;
                    Dispatcher.Invoke(() => {
                        cmbTipoScansione.IsEnabled = true;
                        prbProgressoScan.Value = progressoScansione;
                        txtDurata.Text = txtDurata.Text.Replace("0", durataScansione_l.ToString());
                        txtPorteAperte.Text = txtPorteAperte.Text.Replace("0", porteAperte.ToString());
                        txtPorteScansionate.Text = txtPorteScansionate.Text.Replace("0", porteScansionate.ToString());
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

                    durataScansione.Stop();
                    durataScansione_l = durataScansione.ElapsedMilliseconds;

                    Dispatcher.Invoke(() => {
                        cmbTipoScansione.IsEnabled = true;
                        prbProgressoScan.Value = progressoScansione;
                        txtDurata.Text = txtDurata.Text.Replace("0", durataScansione_l.ToString());
                        txtPorteAperte.Text = txtPorteAperte.Text.Replace("0", porteAperte.ToString());
                        txtPorteScansionate.Text = txtPorteScansionate.Text.Replace("0", porteScansionate.ToString());
                    });
                    return;
                }
            }
            durataScansione.Stop();
            durataScansione_l = durataScansione.ElapsedMilliseconds;
            Dispatcher.Invoke(() => {
                cmbTipoScansione.IsEnabled = true;
                txtDurata.Text = txtDurata.Text.Replace("0", durataScansione_l.ToString());
                txtPorteAperte.Text = txtPorteAperte.Text.Replace("0", porteAperte.ToString());
                txtPorteScansionate.Text = txtPorteScansionate.Text.Replace("0", porteScansionate.ToString());
            });
            scansioneAttiva = false;
        }
        private void Scan_AvviaScansione(object sender, RoutedEventArgs e) {
            listaSockets.Clear();

            Dispatcher.Invoke(() => {
                prbProgressoScan.Value = 0;
                txtDurata.Text = txtDurata.Text.Replace(durataScansione_l.ToString(), "0");
                txtPorteAperte.Text = txtPorteAperte.Text.Replace(porteAperte.ToString(), "0");
                txtPorteScansionate.Text = txtPorteScansionate.Text.Replace(porteScansionate.ToString(), "0");
            });

            if (string.IsNullOrWhiteSpace(txtIPAddress.Text)) {
                MessageBox.Show("Attenzione: Inserire un hostname o indirizzo IP valido!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtIPAddress.Clear();
                txtIPAddress.Focus();
                return;
            }
            string target = txtIPAddress.Text.Trim().ToLower();

            bool check = IPAddress.TryParse(target, out targetIPAddress);
            if (!check) {
                try {
                    targetIPAddress = Dns.GetHostAddresses(target).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                    if (targetIPAddress is null) {
                        MessageBox.Show("Errore: Impossibile trovare un indirizzo IP valido per l'hostname dato!",
                                    "Errore",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                        return;
                    }
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                    MessageBox.Show("Errore: Impossibile trovare un indirizzo IP valido per l'hostname dato!",
                                    "Errore",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }
            }

            if (!Base_Socket.CheckValidPort(txtPortMin.Text)) {
                MessageBox.Show($"Attenzione: Inserire un numero di porta maggiore o uguale a {Base_Socket.PRIMA_PORTA}!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPortMin.Clear();
                txtPortMin.Focus();
                return;
            }
            rangePortMin = int.Parse(txtPortMin.Text);

            if (!Base_Socket.CheckValidPort(txtPortMax.Text)) {
                MessageBox.Show($"Attenzione: Inserire un numero di porta minore o uguale a {Base_Socket.ULTIMA_PORTA}!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPortMax.Clear();
                txtPortMax.Focus();
                return;
            }
            rangePortMax = int.Parse(txtPortMax.Text);

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
            check = int.TryParse(timeoutString, out timeoutScansione);
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
            if (scansioneAttiva) {
                richiestaFermataScansione = true;
            }
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

            string separatoreCSV;
            try {
                separatoreCSV = txtSeparatoreCSV.Text;
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

            string filePath = string.Empty;
            if (dlg.ShowDialog() == true) {
                filePath = dlg.FileName;
            }

            if (string.IsNullOrWhiteSpace(filePath)) {
                return;
            }

            try {
                using StreamWriter writer = new(filePath);
                for (int i = 0; i < dtgScansioni.Items.Count; i++) {
                    var socket = dtgScansioni.Items[i] as Base_Socket;
                    if (i == 0) {
                        writer.WriteLine($"Scansione avviata: [{dataScansione.ToString("dd/MM/yyyy HH:mm:ss")}]");
                        writer.WriteLine($"Durata della scansione: [{durataScansione_l}]ms");
                        writer.WriteLine($"Target: {socket.IPAddress?.ToString()}");
                        writer.WriteLine($"Stato della porta{separatoreCSV}Numero della porta{separatoreCSV}Servizio rilevato");
                    }
                    writer.WriteLine(Base_Socket.ToCSV(socket, separatoreCSV));
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
            if (dtgScansioni.Items.Count == 0) {
                MessageBox.Show("Attenzione: Nessun elemento da esportare trovato!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog dlg = new() {
                Filter = "JSON Files (*.json)|*json",
                AddExtension = true,
                DefaultExt = ".json"
            };

            string path = string.Empty;
            if (dlg.ShowDialog() == true) {
                path = dlg.FileName;
            }

            if (string.IsNullOrWhiteSpace(path)) {
                MessageBox.Show("ATTENZIONE: Selezionare un percorso file corretto!",
                                "Attenzione",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try {
                List<Base_Socket> listaJSON = new();
                foreach (Base_Socket item in dtgScansioni.Items) {
                    listaJSON.Add((Base_Socket)item);
                }
                var listaJson = listaJSON.Select(socket => new {
                    IPAddress = socket.IPAddress?.ToString(),
                    NumeroPorta = socket.NumeroPorta,
                    Stato = socket.Stato.ToString(),
                    Servizio = Base_Socket.ServiziConosciuti.ContainsKey(socket.NumeroPorta)
                                ? Base_Socket.ServiziConosciuti[socket.NumeroPorta]
                                : "Sconosciuto"
                });

                var exportObject = new {
                    DataScansione = dataScansione.ToString("dd/MM/yyyy HH:mm:ss"),
                    DurataMs = durataScansione_l,
                    Target = targetIPAddress?.ToString(),
                    Risultati = listaJson
                };

                string json = JsonSerializer.Serialize(exportObject, new JsonSerializerOptions {
                    WriteIndented = true
                });

                File.WriteAllText(path, json);
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                MessageBox.Show("Errore durante l'esportazione JSON.",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
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
                    ObservableCollection<Base_Socket> listaPorteAperte = new();
                    foreach (Base_Socket socket in listaSockets) {
                        if (socket.Stato == Base_Socket.StatoPorta.Aperta) {
                            listaPorteAperte.Add(socket);
                        }
                    }
                    dtgScansioni.ItemsSource = listaPorteAperte;
                    break;
                case OpzioniFiltri.PorteChiuse:
                    ObservableCollection<Base_Socket> listaPorteChiuse = new();
                    foreach (Base_Socket socket in listaSockets) {
                        if (socket.Stato == Base_Socket.StatoPorta.Chiusa) {
                            listaPorteChiuse.Add(socket);
                        }
                    }
                    dtgScansioni.ItemsSource = listaPorteChiuse;
                    break;
                case OpzioniFiltri.PorteFiltrate:
                    ObservableCollection<Base_Socket> listaPorteFiltrate = new();
                    foreach (Base_Socket socket in listaSockets) {
                        if (socket.Stato == Base_Socket.StatoPorta.Filtrata) {
                            listaPorteFiltrate.Add(socket);
                        }
                    }
                    dtgScansioni.ItemsSource = listaPorteFiltrate;
                    break;
                case OpzioniFiltri.ServiziRilevati:
                    ObservableCollection<Base_Socket> listaServiziRilevati = new();
                    foreach (Base_Socket socket in listaSockets) {
                        if (Base_Socket.ServiziConosciuti.ContainsKey(socket.NumeroPorta)) {
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
                case OpzioniOrdinamenti.AperteFiltrateChiuse:
                    listaSockets = new(listaSockets.OrderBy(s => s.Stato));
                    break;
                case OpzioniOrdinamenti.ChiuseFiltrateAperte:
                    listaSockets = new(listaSockets.OrderByDescending(s => s.Stato));
                    break;
            }
            dtgScansioni.ItemsSource = listaSockets;
        }
        private void OpzioniScansione_TipoScansione(object sender, SelectionChangedEventArgs e) {
            switch ((OpzioniTipoScansione)cmbTipoScansione.SelectedIndex) {
                case OpzioniTipoScansione.TCP:
                    tipoScansione = OpzioniTipoScansione.TCP;
                    break;
                case OpzioniTipoScansione.UDP:
                    tipoScansione = OpzioniTipoScansione.UDP;
                    break;
            }
        }
        private void OpzioniScansione_MouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }

        private void FAQ_PortScanner(object sender, RoutedEventArgs e) {
            FAQ.ShowWindow(FAQ.INFO_PORTSCANNER);
        }
        private void FAQ_Sirius(object sender, RoutedEventArgs e) {
            FAQ.ShowWindow(FAQ.INFO_SIRIUS);
        }
        private void FAQ_Funzionamento(object sender, RoutedEventArgs e) {
            FAQ.ShowWindow(FAQ.FUNZIONAMENTO_PORTSCANNER);
        }
        private void FAQ_Legale(object sender, RoutedEventArgs e) {
            FAQ.ShowWindow(FAQ.LEGAL_PORTSCANNER);
        }
    }
}