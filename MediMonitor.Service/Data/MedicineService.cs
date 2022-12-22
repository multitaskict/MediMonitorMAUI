using System;
using System.Globalization;
using System.Threading.Tasks;
using MediMonitor.Service.Models;

namespace MediMonitor.Service.Data
{
	public class MedicineService
	{
        private readonly AppData appData;

        public MedicineService(AppData appData)
		{
            this.appData = appData;
        }

        public async Task<string> GetMedicineNameAsync(int id)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var medicine = await GetByApiIdAsync(id);

            if(culture.ToLowerInvariant().Contains("nl"))
            {
                return medicine.Naam;
            }
            else
            {
                return medicine.EngelseNaam;
            }
        }

        public async Task<Medicijn> GetByApiIdAsync(int id)
        {
            return await appData.TableQuery<Medicijn>().Where(m => m.ApiId != null && m.ApiId == id).FirstOrDefaultAsync();
        }
	}
}

