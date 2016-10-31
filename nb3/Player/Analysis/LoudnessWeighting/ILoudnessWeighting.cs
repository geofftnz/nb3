namespace nb3.Player.Analysis.LoudnessWeighting
{
    public interface ILoudnessWeighting
    {
        double this[int n] { get; }
        double this[float n] { get; }
        int Size { get; }
    }
}