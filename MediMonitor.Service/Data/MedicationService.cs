using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediMonitor.Service.Models;

namespace MediMonitor.Service.Data
{
	public class MedicationService
    {
        private readonly AppData appData;

        public MedicationService(AppData appData)
		{
            this.appData = appData;
        }

        public async Task<IEnumerable<Medicatie>> GetMedicatiesAsync()
        {
            return await appData.TableQuery<Medicatie>().ToArrayAsync();
        }
	}
}

