using MediMonitor.Service.Models.Enums;

using System.Collections.Generic;

namespace MediMonitor.Service.ApiModels
{
    public class AppMedicatieModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }

        public int AanvraagId { get; set; }

        public int MedicijnId { get; set; }

        public MedicatieVerpakking MedicatieVerpakking { get; set; }

        public IEnumerable<AppInnamemoment> Innamemomenten { get; set; }

        public int? AfbouwPeriode { get; set; }
    }
}
