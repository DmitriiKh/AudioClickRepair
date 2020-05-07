namespace AudioClickRepair.Processing
{
    public interface IPredictor
    {
        int InputDataSize { get; }

        double GetForward(double[] samples, int index);

        double GetBackward(double[] samples, int index);
    }
}
