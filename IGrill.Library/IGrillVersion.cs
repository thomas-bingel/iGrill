using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGrillLibrary
{
    /// <summary>
    /// Defines which iGrill version is connected 
    /// </summary>
    public enum IGrillVersion
    {
        /// <summary>
        /// iGrill Mini with 1 probe.
        /// </summary>
        IGrillMini,

        /// <summary>
        /// iGrill 2 with 4 probes.
        /// </summary>
        /// 
        IGrill2,
        /// <summary>
        /// iGrill 3 with 4 probes.
        /// </summary>
        IGrill3,

        /// <summary>
        /// Simulates an iGrill with one probe. A timer sends different temperatures
        /// </summary>
        Simulation
    }
}
