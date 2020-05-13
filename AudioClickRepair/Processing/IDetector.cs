namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    public interface IDetector
    {
        int InputDataSize { get; }

        double GetErrorLevel(int position, AbstractPatch anotherPatch = null);
    }
}