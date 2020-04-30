namespace AudioClickRepair.Data
{
    internal interface IPatcher
    {
        double[] GetRange(int start, int length);
    }
}