using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PortScanner.core.models;

public class InformationWindow : Window {
    private string VersioneApplicazione { get; set; }

    private IPAddress TargetScansione { get; set; }
    private DateTime TempoScansione { get; set; }

    private int PorteTOT { get; set; }
    private int PorteOpen { get; set; }
    private int PorteClosed { get; set; }
    private int PorteFiltered { get; set; }

    private long DurataScansione { get; set; }

    private int NumeroScansioni { get; set; }

    private string Stato { get; set; }

    public InformationWindow(string versione, string stato,
                             DateTime tempoScansione,
                             long durata,
                             IPAddress target,
                             int porteTOT, int porteOpen, int porteClosed, int porteFiltered,
                             int numeroScansioni,
                             Window owner) {
        VersioneApplicazione = versione;
        Stato = stato;

        TargetScansione = target;
        TempoScansione = tempoScansione;

        PorteTOT = porteTOT;
        PorteOpen = porteOpen;
        PorteClosed = porteClosed;
        PorteFiltered = porteFiltered;

        NumeroScansioni = numeroScansioni;

        DurataScansione = durata;

        Owner = owner;
    }

    /// <summary>
    /// Crea una nuova <see cref="Window"/> basata sulle informazioni passate in parametro dal costruttore.
    /// </summary>
    public void CreaFinestra() {
        const int LARGHEZZA_FINESTRA = 500;
        const int ALTEZZA_FINESTRA = 450;
        const string NOME_FINESTRA = "Informazioni";
        const int MARGINE_CONTENUTO = 10;

        Title = NOME_FINESTRA;
        Width = LARGHEZZA_FINESTRA;
        MinWidth = LARGHEZZA_FINESTRA;
        Height = ALTEZZA_FINESTRA;
        MinHeight = ALTEZZA_FINESTRA;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Icon = new BitmapImage(new Uri("pack://application:,,,/core/assets/application_icons/radar.ico"));

        StackPanel stkpnlFinestra = new() {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(MARGINE_CONTENUTO)
        };

        TextBlock titolo = new() {
            Text = "INFORMAZIONI",
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20)
        };

        TextBlock descrizione = new() {
            Text = "All'interno di questa finestra saranno presenti tutte le informazioni relative all'applicazione ed alla scansione!\n" + 
                   "Se alcuni campi risultano essere vuoti o \"-1\" significa che non è stato possibile ricavare tale informazione.",
            TextWrapping = TextWrapping.Wrap,
        };

        Border CreaBordo() => new() {
            Height = 1,
            Background = Brushes.Gray,
            Margin = new Thickness(0, 15, 0, 15)
        };

        TextBlock infoApplicazione = new() {
            Text = $"Versione applicazione: {VersioneApplicazione}\n" + 
                   $"Status: {Stato}",
            Margin = new Thickness(0, 0, 0, 5)
        };

        TextBlock generalInfoScansione = new() {
            Text = $"Scansione avviata: {TempoScansione:dd/MM/yyyy HH:mm:ss}\n" + 
                   $"Durata scansione (ms): {DurataScansione}\n" +
                   $"Target: {TargetScansione}\n" + 
                   $"Numero di scansioni effettuate: {NumeroScansioni}",
            Margin = new Thickness(0, 0, 0, 10)
        };

        TextBlock porte = new() {
            Text = $"Porte totali: {PorteTOT}\n" +
                   $"Porte aperte: {PorteOpen}\n" +
                   $"Porte chiuse: {PorteClosed}\n" +
                   $"Porte filtrate: {PorteFiltered}",
        };

        stkpnlFinestra.Children.Add(titolo);
        stkpnlFinestra.Children.Add(descrizione);
        stkpnlFinestra.Children.Add(CreaBordo());
        stkpnlFinestra.Children.Add(infoApplicazione);
        stkpnlFinestra.Children.Add(CreaBordo());
        stkpnlFinestra.Children.Add(generalInfoScansione);
        stkpnlFinestra.Children.Add(porte);

        Content = stkpnlFinestra;
    }
}