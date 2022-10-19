using System.Collections.Generic;
using System.Threading.Tasks;
using MediMonitor.Service.Models;

using System.Linq;

namespace MediMonitor.Service.Data
{
    public class UserService
	{
        private readonly AppData appData;

        public UserService(AppData appData)
		{
            this.appData = appData;
        }

        /// <summary>
        /// Get a user by specifying the Id.
        /// </summary>
        /// <param name="id">The id of the user you want to get.</param>
        /// <returns>The user.</returns>
        public async Task<User> GetById(int id)
        {
            return await appData.GetByIdAsync<User>(id);            
        }

        /// <summary>
        /// Get the <see cref="User"/> by specifying the <paramref name="patientId"/>.
        /// </summary>
        /// <param name="patientId">The id of the patient.</param>
        /// <returns>The found User, or null if not found.</returns>
        public async Task<User> GetByPatientIdAsync(int patientId)
        {
            var tableQuery = appData.TableQuery<User>().Where(u => u.PatientId == patientId);

            return await tableQuery.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await appData.TableQuery<User>().ToArrayAsync();
        }
    }
}

