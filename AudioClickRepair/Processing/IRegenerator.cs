namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    interface IRegenerator
    {
        double RestoreFragment(AbstractFragment patched);
    }
}
