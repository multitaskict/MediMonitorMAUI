using MediMonitor.Service.Interfaces;
using MediMonitor.Service.Models.Enums;

using SQLite;

using System;

namespace MediMonitor.Service.Models
{
    public class Innamemoment : IEntity

    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int? ApiId { get; set; }

        public int MedicatieId { get; set; }

        public TimeSpan Tijdstip { get; set; }

        public TimeSpan? Notification { get; set; }

        public decimal InnameHoeveelheid { get; set; }

        public string Opmerking { get; set; }

        public InnamemomentType Type { get; set; }
    }
}