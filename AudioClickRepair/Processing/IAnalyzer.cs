namespace AudioClickRepair.Processing
{
    public interface IAnalyzer
    {
        int GetInputDataSize();

        double GetDefaultResult();

        double GetResult(double[] inputData);
    }
}
