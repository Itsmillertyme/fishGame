public struct MinigameResult {
    public MinigameEndReason reason;
    public float land01;
    public float slack01;

    public bool success => reason == MinigameEndReason.Success;
    public CatchInfo? catchInfo;
}