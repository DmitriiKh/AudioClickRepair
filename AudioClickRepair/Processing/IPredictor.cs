namespace AudioClickRepair.Processing
{
    public interface IPredictor
    {
        int InputDataSize { get; }

        double GetForward(double[] samples);

        double GetBackward(double[] samples);
    }
}
