namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    interface IRegenerator
    {
        int InputDataSize { get; }

        void RestorePatch(AbstractPatch patch);
    }
}
