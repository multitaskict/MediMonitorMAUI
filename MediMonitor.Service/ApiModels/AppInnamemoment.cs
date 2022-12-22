using MediMonitor.Service.Models.Enums;

using System;

namespace MediMonitor.Service.ApiModels
{
    public class AppInnamemoment
    {
        public int Id { get; set; }

        public int AanvraagMedicijnId { get; set; }

        public int ReceptId { get; set; }

        public TimeSpan Tijdstip
        {
            get => TimeSpan.FromTicks(TijdstipTicks);
            set => TijdstipTicks = value.Ticks;
        }

        public long TijdstipTicks { get; set; }

        public decimal InnameHoeveelheid { get; set; }

        public string Opmerking { get; set; }

        public InnamemomentType Type { get; set; }
    }
}
