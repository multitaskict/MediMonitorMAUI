using System;
using MediMonitor.Service.Interfaces;
using SQLite;

namespace MediMonitor.Service.Models
{
	public class Sync : IEntity
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		/// <summary>
        /// The datetime the data was synced.
        /// </summary>
		public DateTime SyncDateTime { get; set; }

		/// <summary>
        /// The result of the sync (JSON format)
        /// </summary>
		public string Result { get; set; }

		/// <summary>
        /// true if sync was succesful, otherwise false.
        /// </summary>
		public bool Success { get; set; }

		/// <summary>
        /// Sync for specific user.
        /// </summary>
		public int UserId { get; set; }
	}
}

