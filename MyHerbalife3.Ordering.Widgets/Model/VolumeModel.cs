using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class VolumeModel
    {
        /// <summary>
        ///     1 based voluem month number (1..12)
        /// </summary>
        public int VolumeMonth { get; set; }

        /// <summary>
        /// Month name based on the VolumeMonth value
        /// </summary>
        public string MonthName { get; set; }

        public bool IsCurrentMonth { get; set; }

        /// <summary>
        ///     Localized header text
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        ///     Personal Purchased Voluem
        /// </summary>
        public decimal PPV { get; set; }

        /// <summary>
        /// </summary>
        public decimal DV { get; set; }

        /// <summary>
        ///     Total Voluem
        /// </summary>
        public decimal TV { get; set; }

        /// <summary>
        ///     Personal Voluem
        /// </summary>
        public decimal PV { get; set; }

        /// <summary>
        ///     Group Voluem
        /// </summary>
        public decimal GV { get; set; }

        /// <summary>
        ///     Volume to display in header - computed according to business rules
        /// </summary>
        public decimal HeaderVolume { get; set; }

        /// <summary>
        /// PPV label text
        /// </summary>
        public string PPVText { get; set; }

        /// <summary>
        /// DV label text
        /// </summary>
        public string DVText { get; set; }

        /// <summary>
        /// TV label text
        /// </summary>
        public string TVText { get; set; }

        /// <summary>
        /// PV label text
        /// </summary>
        public string PVText { get; set; }

        /// <summary>
        /// GV label text
        /// </summary>
        public string GVText { get; set; }
    }
}