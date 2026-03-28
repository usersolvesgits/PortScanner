# Development Log

## Introduzione
Questo documento riporta il progresso dello sviluppo del progetto.  
### Disclaimer
La struttura del progetto è stata pensata per la maggiore scalabilità possibile, per questo potrebbe sembrare esagerata.

---

## Informazioni sul progetto
**Progetto**: Port Scanner  
**Sviluppatore**: Francesco Bianchini  
**Tecnologie utilizzate**: .NET Framework / WPF  
**Data di inizio (dd/MM/yyyy)**: 06/03/2026  

---

## Log di sviluppo

### [06/03/2026]

**Attività svolte**
- Creazione menù dell'interfaccia grafica
- Primi controlli

**Funzionalità implementate**
- Opzioni per uscire dall'applicazione.
- Opzioni per cambiare il tema dell'applicazione (Chiaro, Scuro e Tema di Sistema).
- Aggiunti crediti sviluppatore ed azienda.

### [07/03/2026]

**Attività svolte**
- Creazione UI
- Modifiche della classe TCP_Socket

**Funzionalità implementate**
- Creata l'interfaccia grafica principale dell'applicazione.
- Creati metodi statici della classe TCP_Socket per la formattazione delle informazioni relative alle sockets in formati CSV e JSON.

**Bug Fix**
- Risolto bug che permetteva l'inserimento dall'esterno della proprietà "Servizio" nella classe TCP_Socket.

### [10/03/2026]

**Attività svolte**
- Aggiornamenti UI
- Modifiche della classe TCP_Socket
- Aggiornamenti backend

**Funzionalità implementate**
UI:  
- Aggiunta sezione FAQ.
- Aggiunta opzione per usare lo stile vecchio dell'applicazione.
- Mosse le opzioni di cambio del tema in un menù dedicato.

Backend:  
- Aggiunto loop di scansione di base.
- Modificato il metodo OnClosing() per attivare il messaggio di avvertimento solo quando una scansione non ha finito.

### [12/03/2026]

**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend
- Modifiche della classe TCP_Socket

**Funzionalità implementate**
UI:
- Aggiunte nuove sezioni nel FAQ.
- Modificata sezione di esportazione dati scansione per implementare la scelta del separatore per la formattazione in CSV.

Backend:
- Aggiunta funzionalità per salvare lo scan in formato CSV.
- Aggiunte schermate per la sezione FAQ.

### [16/03/2026]

**Attività svolte**
- Bug fixing
- Aggiornamenti UI
- Aggiornamenti backend

**Funzionalità implementate**
UI:
- Aggiunto pulsante per terminare la scansione prematuramente.
- Aggiunta una scrollbar per la navigazione dell'interfaccia.

Backend:
- Aggiunta funzionalità per la terminazione della scansione.
- Aggiornata la logica di scansione e separata in due sezioni distinte (una per i controlli e l'altra per il loop di esecuzione principale), 
  aggiunto un thread apposito per la scansione.
- Aggiunte documentazioni per metodi.

### [18/03/2026]

**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend

**Funzionalità implementate**
UI:
- Aggiunta ProgressBar per rappresentare in maniera visiva i progressi della scansione.
- Modificata la sezione di filtri ed ordinamento.

Backend:
- Aggiunta opzione per selezionare un timeout tra una scansione ed un'altra.
- Aggiunta logica per aggiornare la ProgressBar durante la scansione.

### [22/03/2026]

**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend
- Modifiche della classe TCP_Socket
- Modifiche della classe Util

**Funzionalità implementate**
UI:
- Aggiunti nuovi comandi nel menù opzioni.
- Aggiunte le shortcuts per le azioni del menù.
- Aggiunta nuova modalità di visualizzazione dello stato della porta.

Backend:
- Aggiunte shortcuts per le opzioni nel menù.
- Aggiunta logica iniziale di ordinamento e filtraggio delle porte.
- Aggiunta logica per visualizzare le statistiche di una scansione.
- Modificata la logica dell'esportazione CSV per agire in base a ciò che è presente nella datagrid, e non su quanto presente nella lista di socket.

Util:
- Spostato il metodo "CheckValidPort(string port)" da MainWindow.xaml.cs a Util.cs.

TCP_Socket:
- Aggiunta nuova proprietà booleana "IsOpen".
- Reso il dizionario per l'individuazione di servizi conosciuti pubblico.

**Bug Fix**
- Risolto bug che non permetteva l'esportazione del risultato della scansione.

**Minor Fixes**
- Modificato il tipo di cursore quando si è sopra i bottoni di esportazione.
- Modificata della documentazione nella classe TCP_Socket.
- Rimossa l'opzione per scorrere tra le opzioni di filtraggio ed ordinamento usando la rotellina mouse.
- Modificata la larghezza base dell'applicazione.

### [23/03/2026]
**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend
- Modifiche della classe TCP_Socket

UI:
- Modificato il testo di spiegazione per il timeout e il suo valore di base.

Backend:
- Cambiato il metodo di connessione principale da TCP_Socket.Connect() a TCP_Socket.Connect(int timeout).

TCP_Socket:
- Aggiunte nuove porte nel dizionario dei servizi conosciuti.
- !!!HUGE!!! Aggiunto metodo TCP_Socket.Connect(int timeout) per rendere il processo di connessione più veloce (https://stackoverflow.com/questions/17118632/how-to-set-the-timeout-for-a-tcpclient).

**Bug Fix**
- Fixato bug dove se si impostava un tempo di timeout troppo piccolo (es: 0) si rischiava che le connessioni non registravano un output corretto nell'interfaccia grafica.

**Aggiornamenti Documentazione**
Andando avanti, si aggiungerà (dove è stato necessario) le fonti da dove si sono ottenute le informazioni.

### [25/03/2026]
**Problemi riscontrati**
- 1) Impossibilità di scansionare le porte 0 e 137.
- 2) Il numero di porte aperte non viene visualizzato sull'interfaccia utente correttamente.

**Minor Fixes**
- Aggiunti controlli sull'inserimento del range delle porte.

### [27/03/2026]
**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend
- Modifiche della classe TCP_Socket
- Modifiche struttura del progetto

UI:
- Modificata l'icona dell'applicazione.

Backend:
- Apportate migliorie nella logica del calcolo delle statistiche della scansione.
- Quando si prova a fermare la scansione uscendo dall'applicazione mentre è ancora in uso, viene mandata una richiesta per fermare la scansione.

TCP_Socket:
- Aggiunte nuovi variabili costanti per il numero di porta minima e massima.
- Spostato il metodo "CheckValidPort(string port)" da Util a TCP_Socket.

Struttura del progetto:
- Rimossa la directory e classe "Util" e spostati i suoi contenuti nel MainWindow.xaml.cs e nella classe TCP_Socket.

**Bug Fix**
- Risolto bug n°2 del giorno *25/03/2026*.

### [28/03/2026]
**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend
- Modifiche della classe TCP_Socket

UI:
- Modificata la datagrid per funzionare senza il binding della proprietà "StatoPorta" della classe TCP_Socket.
- Modificato la casella di testo per scrivere il separatore del salvataggio in CSV.

Backend:
- Modificata la logica di controllo per l'inserimento del range delle porte.
- Migliorie per la fermata della scansione.
- Spostato il reset delle statistiche di scansione all'avvio di essa per evitare disguidi sull'interfaccia utente.
- Modificato il metodo principale di conversione in formato CSV per consentire l'uso di una stringa al posto di un singolo carattere come separatore.

TCP_Socket:
- Rimossa la proprietà "StatoPorta".
- Modificato il metodo Connect(int timeout) per evitare interruzzioni anomale durante la scansione.
- Modificata la costante che indica la prima porta disponibile per effettuare la scansione.
- Aggiornati i metodi di conversione in formato CSV.
- Aggiunti nuovi overload per i metodi ToCSV per permettere l'utilizzo di una stringa come separatore.
- Aggiornata ed aggiunta della documentazione dei metodi.

**Bug Fix**
- Risolto bug n°2 del giorno *25/03/2026*.