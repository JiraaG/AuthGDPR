using System.ComponentModel;

namespace AuthGDPR.Domain.Enums
{
    public enum ApiMessages
    {
        // Sezione Errore
        [Description("Dati inseriti mancanti o non validi. Per favore riprova.")]
        ErroreInputDati,
        [Description("Username già in uso. Per favore riprova,")]
        ErroreUsername,
        [Description("Email già in uso. Per favore eseguui il login oppure inserisci un'altra email.")]
        ErroreEmail,
        [Description("Errore nella registrazione dell'utente.")]
        ErroreRegistrazione,

        [Description("Errore generico.")]
        ErroreGenerico,

        // Sezione Successo
        [Description("Operazione completata con successo.")]
        SuccessoInputDati, 
    }
}
