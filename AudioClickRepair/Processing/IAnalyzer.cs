using System.Collections.Immutable;

namespace AudioClickRepair.Processing
{
    public interface IAnalyzer
    {
        int GetInputDataSize();

        double GetDefaultResult();

        double GetResult(double[] inputData);
    }
}
