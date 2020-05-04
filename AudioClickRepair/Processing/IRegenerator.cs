using AudioClickRepair.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioClickRepair.Processing
{
    interface IRegenerator
    {
        double RestoreFragment(AbstractFragment patched);
    }
}
