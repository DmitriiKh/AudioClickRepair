namespace AudioClickRepair.Data
{
    public interface IPatch
    {
        int StartPosition { get; }
        // TODO remove this static property
        static int MinimalPredictionError { get; } = 0;

        int GetEndPosition();
        double GetOutputSample(int position);
    }
}