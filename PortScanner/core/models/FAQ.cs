using System.Windows;

namespace PortScanner.core.models {
    public static class FAQ {
        public const string INFO_PORTSCANNER = "Un port scanner è uno strumento usato in informatica e networking per analizzare le porte di rete di un dispositivo (come un computer o un server) per vedere quali sono aperte, chiuse o filtrate.";
        public const string INFO_SIRIUS = "Sirius è stata fondata nel 2000 dalla collaborazione tra l’incubatore del Politecnico di Torino e un team di esperti, con l’obiettivo di sviluppare software avanzati per la gestione delle centrali elettriche e della trasmissione di energia.\n\n" +
                                          "I fondatori vantavano esperienza nel settore dell’automazione energetica sin dagli anni ’90, collaborando con aziende di rilievo.\n\n" +
                                          "Nel tempo, l’azienda ha fornito soluzioni a importanti operatori elettrotecnici e, dal 2006, ha integrato le proprie tecnologie nei sistemi VireoX per l’analisi e il controllo remoto degli impianti di energia rinnovabile.";
        public const string FUNZIONAMENTO_PORTSCANNER = "Uno scanner di porte controlla lo stato delle porte di un dispositivo in rete inviando richieste a diverse porte di un indirizzo IP. In base alla risposta ricevuta può determinare se una porta è aperta (servizio attivo), chiusa (nessun servizio) o filtrata (bloccata da firewall o sistemi di sicurezza).";
        public const string LEGAL_PORTSCANNER = "L'uso di un port scanner è generalmente legale per analizzare la propria rete, effettuare test autorizzati o per scopi di studio. Tuttavia, scansionare sistemi senza autorizzazione può essere considerato attività sospetta o illegale in alcuni paesi. Utilizza sempre questi strumenti in modo responsabile.";

        public static void ShowWindow(string message) {
            MessageBox.Show(message,
                            "INFO",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
