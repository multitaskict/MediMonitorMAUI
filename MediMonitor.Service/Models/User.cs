using System;
using System.Collections.Generic;
using MediMonitor.Service.Interfaces;
using SQLite;

namespace MediMonitor.Service.Models
{
    public class User : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Patient Id from the webapp.
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// The first name of the user.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user.
        /// </summary>
        public string LastName { get; set; }


        public override string ToString()
        {
            return $"{PatientId}: {FirstName} {LastName}";
        }
    }
}

