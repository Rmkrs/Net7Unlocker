namespace Net7MultiClientUnlocker.Domain
{
    public enum ClientState
    {
        WaitingOnTOS,
        DisplayingTOS,
        WaitingOnMain,
        DisplayingMain,
        ReadyForInteraction,
        WaitingForSizzleKickoff1,
        WaitingForSizzleKickoff2,
        Stopped
    }
}
