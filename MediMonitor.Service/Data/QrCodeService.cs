using MediMonitor.Service.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MediMonitor.Service.Data
{
    public class QrCodeService
    {
        private readonly AppData appData;

        public QrCodeService(AppData appData)
        {
            this.appData = appData;
        }

        public async Task<QrCode> GetOrCreateQrCodeAsync(string qrCode)
        {
            qrCode = qrCode.Trim();
            var qrCodeEntity = await appData.FirstOrDefaultAsync<QrCode>(x => x.Code == qrCode);

            if(qrCodeEntity == null)
            {
                qrCodeEntity = new QrCode {  Code = qrCode };

                await appData.SaveAsync(qrCodeEntity);
            }

            return qrCodeEntity;
        }
    }
}
