using System;
using MediMonitor.Service.Interfaces;
using SQLite;

namespace MediMonitor.Service.Models
{
	public class Survey : IEntity
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		/// <summary>
        /// The Id of the patient (from web app)
        /// </summary>
		public int PatientId { get; set; }

		/// <summary>
        /// The DateTime the Survey was entered.
        /// </summary>
		public DateTime DateTime { get; set; }

		/// <summary>
        /// The DateTime the Survey was modified.
        /// </summary>
		public DateTime? ModifiedDateTime { get; set; }

		/// <summary>
		/// The DateTime the Survey was synced.
		/// </summary>
		public DateTime? SyncDateTime { get; set; }

		/// <summary>
        /// The score the user entered.
        /// </summary>
		public int? Score { get; set; }

		/// <summary>
        /// The user of the survey
        /// </summary>
		public int UserId { get; set; }
	}
}

