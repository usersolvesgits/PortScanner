/*
 *  TODOS:
 *      -MainWindow.xaml.cs:
 *          -OnClosing()
 *      -Socket:
 *          - Connect()
 */


using PortScanner.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Util = PortScanner.Core.Utils.Util;


namespace PortScanner {
    public partial class MainWindow : Window {
        const string urlSviluppatore = "https://github.com/usersolvesgits";
        const string urlAzienda = "https://www.sirius.to.it/";
        ObservableCollection<TCP_Socket> listaSockets;
        public MainWindow() {
            InitializeComponent();
            listaSockets = new();
            dtgScansioni.ItemsSource = listaSockets;
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

            // TODO -> mostrare e modificare in base se la scansione ha finito o meno
            MessageBoxResult result = MessageBox.Show(
                "Attenzione, sei sicuro di voler uscire?",
                "Attenzione!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.No) {
                e.Cancel = true;
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

        private void Scan_AvviaScansione(object sender, RoutedEventArgs e) {
            //TODO -> logica inizio scansione
        }
        private void Scan_EsportaJSON(object sender, RoutedEventArgs e) {
            //TODO -> logica esporto JSON
        }
        private void Scan_EsportaCSV(object sender, RoutedEventArgs e) {
            //TODO -> logica esporto CSV
        }
    }
}