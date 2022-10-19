using System;
using MediMonitor.Service.Interfaces;
using SQLite;

namespace MediMonitor.Service.Models
{
	public class QrCode : IEntity
	{
		public QrCode()
		{
		}

		[PrimaryKey, AutoIncrement]
        public int Id { get; set; }

		public string Code { get; set; }

		public int? UserId { get; set; }
    }
}

