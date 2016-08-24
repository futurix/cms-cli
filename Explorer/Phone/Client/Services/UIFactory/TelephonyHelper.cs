using System;
using Microsoft.Phone.Tasks;
using Wave.Common;

namespace Wave.Services
{
    public static class TelephonyHelper
    {
        public const int TextSizeLimit = 4096;
        
        public static void ChoosePhoneNumber(DataEventHandler<string> callback)
        {
            PhoneNumberChooserTask task = new PhoneNumberChooserTask();

            task.Completed += 
                (sender, e) => 
                {
                    if ((callback != null) && (e != null) && !String.IsNullOrEmpty(e.PhoneNumber))
                        callback(null, new DataEventArgs<string>(e.PhoneNumber));
                };

            task.Show();
        }

        public static bool ValidatePhoneNumber(string number)
        {
            bool res = true;
            
            foreach (char letter in number)
            {
                if (Char.IsLetter(letter))
                    res = false;
            }

            return res;
        }

        public static void SendText(string number, string message)
        {
            if (message.Length >= TextSizeLimit)
                return;
            
            SmsComposeTask task = new SmsComposeTask();

            task.To = number ?? String.Empty;
            task.Body = message ?? String.Empty;

            task.Show();
        }

        public static void SendEmail(string to, string subject, string body)
        {
            EmailComposeTask task = new EmailComposeTask();

            task.To = to ?? String.Empty;
            task.Subject = subject ?? String.Empty;
            task.Body = body ?? String.Empty;

            task.Show();
        }
    }
}
