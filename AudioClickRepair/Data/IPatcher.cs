namespace AudioClickRepair.Data
{
    internal interface IPatcher
    {
        double[] GetRange(int start, int length);

        double GetValue(int position);
    }
}