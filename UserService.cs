using NetworkProgrammingP47.Dal;
using NetworkProgrammingP47.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkProgrammingP47
{
    internal class UserService
    {
        private DataAccessor dataAccessor;
        public void Run()
        {
            try { dataAccessor = new DataAccessor();  } 
            catch { return; }
            while (true)
            {
                Console.WriteLine("Сервіс роботи з користувачами:\n" +
                    "1: реєстрація\n" +
                    "2: автентифікація(вхід)\n" +
                    "3: забув пароль\n" +
                    "i: інсталювати таблиці БД\n" +
                    "0: вихід"
                );
                var keyInfo = Console.ReadKey();
                Console.WriteLine();
                switch(keyInfo.KeyChar)
                {
                    case '0': return;
                    case '1': SignUp(); break;
                    case '2': Console.WriteLine(OtpService.ConfirmCode()); break;
                    case '3': Console.WriteLine(OtpService.TempPassword()); break;
                    case 'i': try { dataAccessor.InstallTables(); } catch { return; } break;

                    default: Console.WriteLine("\nВибір не розпізнано\n"); break;
                }
            }
        } 
        private void SignUp()
        {
            Console.WriteLine("\nРеєстрація нового користувача");
            String email = "";
            while (true) 
            {
                Console.Write("Введіть E-mail: ");
                email = Console.ReadLine()!;
                if (Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("E-mail не відповідає формату, відкоригуйте");
                }
            }
            Console.Write("Створіть пароль: ");
            String password = "";
            while (true)
            {
                password = Console.ReadLine()!;
                if (Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$"))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Пароль має бути щонайменше 6 символів, " +
                        "серед яких має бути цифра, літера та спецсимвол");
                }
            }
            Console.Write("Як до вас звертатися? ");
            String name = Console.ReadLine()!;

            String confirmCode = OtpService.ConfirmCode();
            
            try
            {
                dataAccessor.AddUser(new()
                {
                    Name = name,
                    Email = email,
                    ConfirmCode = confirmCode,
                    Password = password
                });
            }
            catch { return; }
            EmailService.SendConfirmCode(email, confirmCode);

            //Console.Write("Введіть код, надісланий на вашу пошту: ");
            //String code = Console.ReadLine()!;
            Console.WriteLine("Ви успішно зареєстровані. Використовуйте пошту та пароль для входу");
        }
    }
}
