using NetworkProgrammingP47.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetworkProgrammingP47
{
    internal class SmtpDemo
    {
        private const String settingsFilename = "smtp_settings.json";
        public void Run()
        {
            Console.WriteLine("SMTP - Simple Mail Transfer Protocol");
            if (!File.Exists(settingsFilename))
            {
                Console.WriteLine("Помилка підключення конфігурації: 'smtp_settings.json'\n" +
                    "Якщо ви клонували проєкт, перечитайте README.MD");
                return;
            }
            var settings = JsonSerializer.Deserialize<JsonElement>(
                File.ReadAllText(settingsFilename)
            );
            var gmailSection = settings.GetProperty("Gmail");
            String host = gmailSection.GetProperty("Host").GetString()!;
            int port = gmailSection.GetProperty("Port").GetInt32()!;
            String email = gmailSection.GetProperty("Email").GetString()!;
            String key = gmailSection.GetProperty("Key").GetString()!;
            // Console.WriteLine("{0} {1} {2} {3}", host, port, email, key);

            SmtpClient smtpClient = new()
            {
                Host = host,
                Port = port,
                EnableSsl = true,
                Credentials = new NetworkCredential(email, key)
            };
            //smtpClient.Send(email, "dns.lector@ukr.net", "NP-P47", "Hello from student Vova");
            /*MailMessage mailMessage = new()
            {
                From = new MailAddress(email, "NP☛P47", Encoding.UTF8),
                IsBodyHtml = true,
                Body = @"<html>
                <h1>Шановний клієнте!</h1>
                <p>Тільки для вас діє чудова <b style='color:maroon'>пропозиція</b></p>
                <p>Деталі на <a href='https://itstep.org/'>сайті</a></p>
                <a href='https://itstep.org/' style='text-decoration:none; color:snow; background-color: maroon; border-radius: 5px; padding: 7px 10px;font-variant: small-caps'>Зареєструватись</a>
                </html>",
                Subject = "Весняна пропозиція"
            };
            mailMessage.To.Add("azure.spd111.od.0@ukr.net");

            mailMessage.Attachments.Add(
                new Attachment(
                    fileName: "./Attachments/NP.png",
                    mediaType: "image/png"
            ));
            mailMessage.Attachments.Add(
                new Attachment(
                    fileName: "./Attachments/README.pdf",
                    mediaType: "application/pdf"
            ));
            mailMessage.Attachments.Add(
                new Attachment(
                    fileName: "./Attachments/дз.txt",
                    mediaType: "text/plain"
            ));*/

            Random random = new Random();
            String confirmationCode = random.Next(100000, 999999).ToString();
            String activationUrl = $"https://example.com/activate?code={confirmationCode}&email={email}";

            MailMessage mailMessage = new()
            {
                From = new MailAddress(email, "NP P47", Encoding.UTF8),
                IsBodyHtml = true,
                Subject = "ДЗ Дон Володимир",
                Body = $@"<!doctype html>
                          <html>
                            <body style='font-family:Arial,Helvetica,sans-serif; color:#333;'>
                              <h2 style='color:#2a5d84;'>Вітаємо з реєстрацією!</h2>
                              <p>Дякуємо за реєстрацію. Щоб підтвердити електронну пошту, використайте код нижче або натисніть кнопку для автоматичної активації.</p>
                          
                              <div style='margin:20px 0; padding:15px; background:#f6f9fc; display:inline-block; border-radius:6px;'>
                                <div style='font-size:18px; color:#111;'>Код підтвердження:</div>
                                <div style='font-size:28px; font-weight:bold; letter-spacing:3px; margin-top:6px;'>{confirmationCode}</div>
                              </div>
                          
                              <p style='margin-top:20px;'>
                                <a href='{activationUrl}' style='display:inline-block; text-decoration:none; background:#0078d4; color:#fff; padding:12px 18px; border-radius:6px;'>Активувати акаунт</a>
                              </p>
                          
                              <p style='color:#666; font-size:13px; margin-top:24px;'>Якщо кнопка не працює, скопіюйте і вставте посилання у ваш браузер:<br/>{activationUrl}</p>
                          
                              <hr/>
                              <p style='font-size:12px; color:#888;'>Це автоматичне повідомлення. Будь ласка, не відповідайте на нього.</p>
                            </body>
                          </html>"
            };
            mailMessage.To.Add("dns.lector@ukr.net");

            smtpClient.Send(mailMessage);
            smtpClient.Dispose();
        }
    }
}
