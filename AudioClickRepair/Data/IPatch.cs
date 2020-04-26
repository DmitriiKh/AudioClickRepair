namespace AudioClickRepair.Data
{
    public interface IPatch
    {
        int StartPosition { get; }
        // TODO remove this static property
        static int MinimalPredictionError { get; } = 0;

        int EndPosition { get; }

        double GetOutputSample(int position);
    }
}