namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    public interface IDetector
    {
        double GetErrorLevel(int position, AbstractPatch anotherPatch = null);
    }
}