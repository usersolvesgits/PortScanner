# Development Log

## Introduzione
Questo documento riporta il progresso dello sviluppo del progetto.
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
- Opzioni per uscire dall'applicazione
- Opzioni per cambiare il tema dell'applicazione (Chiaro, Scuro e Tema di Sistema)
- Aggiunti crediti sviluppatore ed azienda

### [07/03/2026]

**Attività svolte**
- Creazione UI
- Modifiche della classe TCP_Socket

**Funzionalità implementate**
- Creata l'interfaccia grafica principale dell'applicazione
- Creati metodi statici della classe TCP_Socket per la formattazione delle informazioni relative alle sockets in formati CSV e JSON

**Bug Fix**
- Risolto bug che permetteva l'inserimento dall'esterno della proprietà "Servizio" nella classe TCP_Socket

### [10/03/2026]

**Attività svolte**
- Aggiornamenti UI
- Modifiche della classe TCP_Socket
- Aggiornamenti backend

**Funzionalità implementate**
UI:  
- Aggiunta sezione FAQ
- Aggiunta opzione per usare lo stile vecchio dell'applicazione
- Mosse le opzioni di cambio del tema in un menù dedicato  

Backend:  
- Aggiunto loop di scansione di base
- Modificato il metodo OnClosing() per attivare il messaggio di avvertimento solo quando una scansione non ha finito.

### [12/03/2026]

**Attività svolte**
- Aggiornamenti UI
- Aggiornamenti backend
- Modifiche della classe TCP_Socket

**Funzionalità implementate**
UI:
- Aggiunte nuove sezioni nel FAQ
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