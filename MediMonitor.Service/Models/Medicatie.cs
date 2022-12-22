using MediMonitor.Service.Interfaces;
using MediMonitor.Service.Models.Enums;

using SQLite;

using System;

namespace MediMonitor.Service.Models
{
    public class Medicatie : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int? ApiId { get; set; }

        public int MedicijnId { get; set; }

        public int UserId { get; set; }

        public int AanvraagId { get; set; }

        public int PatientId { get; set; }

        public MedicatieVerpakking MedicatieVerpakking { get; set; }

        public int? AfbouwPeriode { get; set; }
    }
}
