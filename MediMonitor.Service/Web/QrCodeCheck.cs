using MediMonitor.Service.Exceptions;
using MediMonitor.Service.Models;

using System;
using System.Linq;

namespace MediMonitor.Service.Web
{
    public class QrCodeCheck
    {
        public QrCodeCheck(string qrCode)
            : this()
        {
            if (qrCode.Length != 20)
                throw new QrCodeException("QrLengthEx", "Must be 20 characters");

            var qrCodeParts = qrCode.Split('-');

            if (qrCodeParts.Length != 3 ||
                qrCodeParts[0].Length != 9 ||
                qrCodeParts[1].Length != 5 ||
                qrCodeParts[2].Length != 4)
                throw new ArgumentException("QrFormatEx", "Invalid qr code format");

            UserKey = qrCodeParts[0];
            MedicationKey = qrCodeParts[1];
            RandomKey = qrCodeParts[2].Substring(0, 3);
            CheckCode = int.Parse(qrCodeParts[2].Substring(3, 1));
        }

        public QrCodeCheck(string userKey, string medicationKey)
            : this()
        {
            UserKey = userKey;
            MedicationKey = medicationKey;

            RandomKey = $"C{new Random().Next(1, 99):00}";
            CheckCode = GetCheckCode();
        }

        public QrCodeCheck(string userKey, string medicationKey, string randomKey)
            : this()
        {
            UserKey = userKey;
            MedicationKey = medicationKey;
            RandomKey = randomKey;

            CheckCode = GetCheckCode();
        }

        public QrCodeCheck(QrCode qrCode)
            : this(qrCode.Code)
        {

        }

        public QrCodeCheck()
        {

        }

        public string UserKey { get; set; }

        public string MedicationKey { get; set; }

        public string RandomKey { get; set; }

        public int CheckCode { get; set; }

        public bool IsValid()
        {
            return CheckCode == GetCheckCode();
        }

        private int GetCheckCode()
        {
            if (UserKey.Length != 9)
                throw new QrCodeException("QrFormatEx", "Patient key must be 9 characters");

            if (MedicationKey.Length != 5)
                throw new QrCodeException("QrFormatEx", "Medication key must be 5 characters");

            if (RandomKey.Length != 3)
                throw new QrCodeException("QrFormatEx", "Random key must be 3 characters");

            //Calculate the check number
            //P12345678
            var userKeyNumber = UserKey.Substring(1).ToCharArray().Select(n => char.GetNumericValue(n)).Sum();
            //M1234
            var medKeyNumber = MedicationKey.Substring(1).ToCharArray().Select(n => char.GetNumericValue(n)).Sum();
            //C123
            var ranndomKeyNumber = RandomKey.Substring(1, 2).ToCharArray().Select(n => char.GetNumericValue(n)).Sum();

            if (!UserKey.StartsWith("A") && userKeyNumber + medKeyNumber + ranndomKeyNumber == 0)
                throw new QrCodeException("QrFormatEx", "Code cannot be empty");

            return (int)(userKeyNumber + medKeyNumber + ranndomKeyNumber) % 10;
        }

        public override string ToString()
        {
            return $"{UserKey}-{MedicationKey}-{RandomKey}{CheckCode}";
        }
    }
}
