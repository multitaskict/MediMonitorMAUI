using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediMonitor.Service.Models;

namespace MediMonitor.Service.Data
{
	public class IntakeService
	{
        private readonly AppData appData;

        public IntakeService(AppData appData)
		{
            this.appData = appData;
        }

        public async Task<IEnumerable<Innamemoment>> GetIntakesAsync(int medicationId)
        {
            return await appData.ToListAsync<Innamemoment>(x => x.MedicatieId == medicationId);
        }

        public async Task<bool> UpdateNotification(Innamemoment innamemoment, TimeSpan notification)
        {
            innamemoment.Notification = notification;

            return await appData.SaveAsync(innamemoment) == 1;
        }
	}
}

