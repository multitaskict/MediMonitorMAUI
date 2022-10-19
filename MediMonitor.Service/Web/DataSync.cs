using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediMonitor.Service.Models;

namespace MediMonitor.Service.Web
{
	public class DataSync
	{
        private readonly Connection connection;
        private readonly MedicijnVerstrekking medicijnVerstrekking;

        public DataSync(Connection connection, MedicijnVerstrekking medicijnVerstrekking)
		{
            this.connection = connection;
            this.medicijnVerstrekking = medicijnVerstrekking;
        }

        public async Task<Sync> SyncAsync(User user, IEnumerable<Survey> surveys)
        {
            throw new NotImplementedException();
        }
	}
}

