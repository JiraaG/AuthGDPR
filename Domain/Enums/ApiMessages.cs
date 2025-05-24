using System.ComponentModel;

namespace AuthGDPR.Domain.Enums
{
    public enum ApiMessages
    {
        // Sezione Errore
        [Description("Dati inseriti mancanti o non validi. Per favore riprova")]
        ErroreInputDati,

        [Description("Errore generico.")]
        ErroreGenerico,

        // Sezione Successo
        [Description("Operazione completata con successo.")]
        SuccessoInputDati, 
    }
}
