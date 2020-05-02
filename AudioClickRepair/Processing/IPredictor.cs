namespace AudioClickRepair.Processing
{
    public interface IPredictor
    {
        int InputDataSize => 0;

        double GetForward(double[] samples, int index);
        double GetBackward(double[] samples, int index);
    }
}
