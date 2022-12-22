using MediMonitor.Service.Interfaces;

using SQLite;

namespace MediMonitor.Service.Models
{
    public class Medicijn : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int? ApiId { get; set; }

        public string Naam { get; set; }

        public string EngelseNaam { get; set; }
    }
}
