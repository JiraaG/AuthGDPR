namespace AuthGDPR.Domain.Enums
{
    public enum ActionType
    {
        //
        Created,
        Updated,
        Deleted,
        Viewed,

        //
        Register,
        ConfirmEmail,
        Login,
        Refresh,
        Logout,

        //
        InternalServerError
    }
}
