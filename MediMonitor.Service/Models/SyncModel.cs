using System;
using System.Collections.Generic;
using System.Text;

namespace MediMonitor.Service.Models
{
    internal class SyncModel
    {
        public int PatientId { get; set; }

        public IEnumerable<Survey> Surveys { get; set; }

        public DateTime? LastSync { get; set; }
    }
}
