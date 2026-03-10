/*
 *  TODOS:
 *      -MainWindow.xaml.cs:
 *          -OnClosing()
 *          -Scan_AvviaScansione()
 *          -Scan_EsportaCSV()
 *          -Scan_EsportaJSON()
 *      -Socket:
 *          - Connect()
 */


using PortScanner.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using Util = PortScanner.Core.Utils.Util;


namespace PortScanner {
    public partial class MainWindow : Window {
        const string urlSviluppatore = "https://github.com/usersolvesgits";
        const string urlAzienda = "https://www.sirius.to.it/";

        ObservableCollection<TCP_Socket> listaSockets;
        string target = String.Empty;
        int rangePortMin,
            rangePortMax;
        bool scansioneAttiva = false;

        public MainWindow() {
            InitializeComponent();
            listaSockets = new();
            dtgScansioni.ItemsSource = listaSockets;
            txtIPAddress.Focus();
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

        private void Scan_AvviaScansione(object sender, RoutedEventArgs e) {
            listaSockets.Clear();

            target = txtIPAddress.Text.Trim().ToLower();

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
                MessageBox.Show("Errore: Errore nel tentativo di conversione della porta massima, assicurarsi che il valore sia numerico!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPortMax.Clear();
                txtPortMax.Focus();
                return;
            }

            scansioneAttiva = true;
            for (int currentPortNum = rangePortMin; currentPortNum <= rangePortMax; currentPortNum++) {
                TCP_Socket socket = new(target, currentPortNum);
                socket.Connect();
                listaSockets.Add(socket);
            }
            scansioneAttiva = false;
        }
        private void Scan_EsportaJSON(object sender, RoutedEventArgs e) {
            //TODO -> logica esporto JSON
        }
        private void Scan_EsportaCSV(object sender, RoutedEventArgs e) {
            //TODO -> logica esporto CSV
        }

        private void FAQ_PortScanner(object sender, RoutedEventArgs e) {
            MessageBox.Show("Un port scanner è uno strumento usato in informatica e networking per analizzare le porte di rete di un dispositivo (come un computer o un server) per vedere quali sono aperte, chiuse o filtrate.",
                            "INFO",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void FAQ_Sirius(object sender, RoutedEventArgs e) {
            //TODO -> aggiungere sub-finestra per informazioni su sirius
        }
    }
}