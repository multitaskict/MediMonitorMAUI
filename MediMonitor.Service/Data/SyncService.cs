using MediMonitor.Service.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MediMonitor.Service.Data
{
    public class SyncService
    {
        private readonly AppData appData;
        private readonly User user;

        public SyncService(AppData appData, User user)
        {
            this.appData = appData;
            this.user = user;
        }

        public async Task<Sync> GetLastSync()
        {
            return await appData.TableQuery<Sync>()
                                .Where(s => s.UserId == user.Id)
                                .OrderByDescending(s => s.SyncDateTime)
                                .FirstOrDefaultAsync();
        }
    }
}
