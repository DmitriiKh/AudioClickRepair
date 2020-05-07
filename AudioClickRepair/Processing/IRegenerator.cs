namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    interface IRegenerator
    {
        int InputDataSize { get; }

        double RestoreFragment(AbstractFragment patched);
    }
}
