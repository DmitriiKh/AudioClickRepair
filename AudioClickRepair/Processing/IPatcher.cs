namespace AudioClickRepair.Processing
{
    internal interface IPatcher
    {
        double[] GetRange(int start, int length);

        double GetValue(int position);
    }
}