using System;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using webshop.Models;

namespace webshop
{
    public class Manager
    {
        public static int SaltLength = 64;
        public static Dictionary<string, User> LoggedInUsers = new Dictionary<string, User>();
        public static Dictionary<User, string> PasswordRecoveryCodes = new Dictionary<User, string>();
        public static Dictionary<User, string> PasswordChangeSalts = new Dictionary<User, string>();

        public static string UserNotEligableMessage = "Nem megfelelő jogkör!";
        public static string UserNotExistingMessage = "A felhasználó nincs bejelentkezve!";

        public static string GenerateSalt()
        {
            Random random = new Random();
            string karakterek = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string salt = "";
            for (int i = 0; i < SaltLength; i++)
            {
                salt += karakterek[random.Next(karakterek.Length)];
            }
            return salt;
        }
        public static string CreateSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static bool CheckPermission(string token, int requiredPermissionLevel)
        {
            if (LoggedInUsers.ContainsKey(token) && LoggedInUsers[token].PermissionLevel >= requiredPermissionLevel)
                return true;
            else
                return false;
        }

        public static async Task SendEmail(string mailAddressTo, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("email");
            mail.To.Add(mailAddressTo);
            mail.Subject = subject;
            mail.Body = body;
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("email", "password");
            smtpServer.EnableSsl = true;
            await smtpServer.SendMailAsync(mail);
        }

        public static string GenerateOrderNumber()
        {
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string orderNumber = "";
            for (int i = 0; i < 8; i++)
            {
                orderNumber += characters[random.Next(characters.Length)];
            }

            return orderNumber;
        }

        public static string GenerateAuthCode()
        {
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string authCode = "";
            for (int i = 0; i < 16; i++)
            {
                authCode += characters[random.Next(characters.Length)];
            }

            return authCode;
        }

        public static bool CheckAuthCode(User user, string authCode)
        {
            if (PasswordRecoveryCodes.ContainsKey(user))
            {
                if (PasswordRecoveryCodes.TryGetValue(user, out string tempAuthCode))
                {
                    if (tempAuthCode == authCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
