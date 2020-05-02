using AudioClickRepair.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioClickRepair.Processing
{
    interface IRegenerator
    {
        void RestoreFragment(AbstractFragment patched);
    }
}
