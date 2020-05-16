namespace AudioClickRepair.Processing
{
    public interface IPatcher
    {
        double[] GetRange(int start, int length, Data.AbstractPatch anotherPatch = null);

        double GetValue(int position);
    }
}