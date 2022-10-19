using System;
using MediMonitor.Service.Interfaces;
using SQLite;

namespace MediMonitor.Service.Models
{
	public class AppVersion : IEntity
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Version { get; set; }
	}
}

