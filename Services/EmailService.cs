using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetworkProgrammingP47.Services
{
    internal class EmailService
    {
       private const String settingsFilename = "smtp_settings.json";
        private static SmtpData? _smtpData;
        public static SmtpData SmtpData
        {
            get
            {
                if (_smtpData == null)
                {
                    if (!File.Exists(settingsFilename))
                    {
                        throw new FileNotFoundException(
                            "Помилка підключення конфігурації smtp_settings.json\n" +
                            "Якщо ви клонували проект, перечитайте README");
                    }

                    var settings = JsonSerializer.Deserialize<JsonElement>(
                        File.ReadAllText(settingsFilename)
                    );

                    var gmailSection = settings.GetProperty("Gmail");
                    _smtpData = new()
                    {
                        Host = gmailSection.GetProperty("Host").GetString()!,
                        Port = gmailSection.GetProperty("Port").GetInt32()!,
                        Email = gmailSection.GetProperty("Email").GetString()!,
                        Key = gmailSection.GetProperty("Key").GetString()!,
                    };
                }
                return _smtpData;
            }
        }

        public static void SendNewPassword(string email, string password)
        {
            SendEmail(email, "Ваш новий пароль",
                $@"<html>
                <h1>Шановний клієнте!</h1>
                <p>Ви запитали новий пароль: <b>{password}</b></p>
                </html>"
            );
        }

        public static void SendLoginNotification(string email, DateTime loginTime)
        {
            SendEmail(email, "Новий вхід до вашого акаунту",
                $@"<html>
                <h1>Шановний клієнте!</h1>
                <p>Зафіксовано новий вхід до вашого акаунту з таким деталями:</p>
                <ul>
                    <li><strong>Дата та час входу:</strong> {loginTime:dd.MM.yyyy HH:mm:ss}</li>
                    <li><strong>Спосіб автентифікації:</strong> пароль</li>
                </ul>
                <p style='color: red;'><strong>Якщо це були не ви, рекомендуємо негайно змінити пароль!</strong></p>
                </html>"
            );
        }

        public static void SendConfirmCode(string email, string code)
        {

            SendEmail(email, "Вітаємо з реєстрацією!",
                $@"<html>
                <h1>Шановний клієнте!</h1>
                <p>Для завершення реєстрації введіть код підтвердження <b>{code}</b></p>
                </html>"
            );
        }

            
        private static void SendEmail(string email, string subject, string body)
        {
            MailMessage mailMessage = new()
            {
                From = new MailAddress(SmtpData.Email, "NP P47", Encoding.UTF8),
                IsBodyHtml = true,
                Subject = subject,
                Body = body,
            };

            mailMessage.To.Add(email);
            using SmtpClient smtpClient = new()
            {
                Host = SmtpData.Host,
                Port = SmtpData.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(SmtpData.Email, SmtpData.Key)
            };

            smtpClient.Send(mailMessage);
        }
    }
    internal class SmtpData
    {
        public String Host { get; set; } = null!;
        public int Port { get; set; }
        public String Email { get; set; } = null!;
        public String Key { get; set; } = null!;
    }
}
