using NetworkProgrammingP47.Dal;
using NetworkProgrammingP47.Models;
using NetworkProgrammingP47.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkProgrammingP47
{
    internal class UserService
    {
        private DataAccessor dataAccessor;
        private UserEntity? currentUser;
        public void Run()
        {
            try { dataAccessor = new DataAccessor();  } 
            catch { return; }
            while (true)
            {
                if (currentUser == null)
                {
                    // Unauthorized menu
                    ShowUnauthorizedMenu();
                }
                else
                {
                    // Authorized menu
                    ShowAuthorizedMenu();
                }
            }
        }
        private void ShowUnauthorizedMenu()
        {
            Console.WriteLine(
                "\nСервіс роботи з користувачами:\n" +
                "1: реєстрація\n" +
                "2: автентифікація(вхід)\n" +
                "3: забув пароль\n" +
                "i: інсталювати таблиці БД\n" +
                "0: вихід"
            );
            var keyInfo = Console.ReadKey();
            Console.WriteLine();

            switch (keyInfo.KeyChar)
            {
                case '0': Environment.Exit(0); break;
                case '1': SignUp(); break;
                case '2': SignIn(); break;
                case '3': ForgotPassword(); break;
                case 'i': try { dataAccessor.InstallTables(); } catch { return; } break;
                default: Console.WriteLine("\nВибір не розпізнано\n"); break;
            }
        }

        private void ShowAuthorizedMenu()
        {
            Console.WriteLine(
                $"\nВітаємо, {currentUser!.Name}!\n" +
                "1: перегляд персональних даних (кабінет)\n" +
                "2: змінити пароль\n" +
                "3: редагувати дані\n" +
                "0: вихід з акаунту"
            );
            var keyInfo = Console.ReadKey();
            Console.WriteLine();

            switch (keyInfo.KeyChar)
            {
                case '0': currentUser = null; break;
                case '1': ViewProfile(); break;
                case '2': ChangePassword(); break;
                case '3': EditProfile(); break;
                default: Console.WriteLine("\nВибір не розпізнано\n"); break;
            }
        }

        private void ViewProfile()
        {
            Console.WriteLine("\n=== Особистий кабінет ===");
            Console.WriteLine($"Ім'я: {currentUser!.Name}");
            Console.WriteLine($"Email: {currentUser.Email}");
            Console.WriteLine($"Email підтверджено: {(currentUser.ConfirmCode == null ? "Так" : "Ні")}");
            Console.WriteLine($"Дата реєстрації: {currentUser.RegisteredAt}");

            if (currentUser.ConfirmCode != null)
            {
                Console.WriteLine($"\nУ вас не підтверджена пошта. Код підтвердження було надіслано: {currentUser.ConfirmCodeSentAt}");
                Console.Write("Бажаєте підтвердити зараз? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    ConfirmEmailAfterLogin();
                }
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
        }

        private void ConfirmEmailAfterLogin()
        {
            Console.Write("Введіть код підтвердження: ");
            int tries = 3;

            while (tries > 0)
            {
                string code = Console.ReadLine()!;

                if (string.IsNullOrEmpty(code))
                {
                    Console.WriteLine("Пошта лишається непідтвердженою");
                    return;
                }

                if (code == currentUser!.ConfirmCode)
                {
                    try
                    {
                        dataAccessor.ConfirmEmail(currentUser);
                        currentUser.ConfirmCode = null;
                        Console.WriteLine("Email успішно підтверджено!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка підтвердження: {ex.Message}");
                    }
                    return;
                }
                tries--;
                if (tries > 0)
                {
                    Console.Write($"Код невірний. Залишилось спроб: {tries}. Спробуйте ще раз: ");
                }
            }
            Console.WriteLine("Ви вичерпали всі спроби. Пошта лишається непідтвердженою");
        }

        private void ChangePassword()
        {
            Console.WriteLine("\n=== Зміна паролю ===");

            Console.Write("Введіть поточний пароль: ");
            string? currentPassword = InputPassword();

            if (currentPassword == null)
            {
                Console.WriteLine("Зміну паролю скасовано.");
                return;
            }

            var userCheck = dataAccessor.Authenticate(currentUser!.Email, currentPassword);
            if (userCheck == null)
            {
                Console.WriteLine("Поточний пароль невірний. Зміну паролю скасовано.");
                return;
            }

            Console.WriteLine();
            Console.Write("Введіть новий пароль: ");
            string? newPassword = InputPassword();

            if (newPassword == null)
            {
                Console.WriteLine("Зміну паролю скасовано.");
                return;
            }

            if (!Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$"))
            {
                Console.WriteLine("Новий пароль не відповідає вимогам безпеки.");
                Console.WriteLine("Пароль має бути щонайменше 6 символів, " +
                    "серед яких має бути цифра, літера та спецсимвол");
                return;
            }
            Console.WriteLine();
            Console.Write("Підтвердіть новий пароль: ");
            string? confirmPassword = InputPassword();

            if (confirmPassword == null)
            {
                Console.WriteLine("Зміну паролю скасовано.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                Console.WriteLine("Паролі не співпадають. Зміну паролю скасовано.");
                return;
            }

            try
            {
                currentUser.Dk = newPassword;
                dataAccessor.UpdateUser(currentUser);
                Console.WriteLine("Пароль успішно змінено!");

                try
                {
                    EmailService.SendPassworChangeNotification(currentUser.Email);
                }
                catch{}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при зміні паролю: {ex.Message}");
            }
        }

        private void EditProfile()
        {
            Console.WriteLine("\n=== Редагування даних ===");
            Console.WriteLine($"Поточне ім'я: {currentUser!.Name}");
            Console.Write("Введіть нове ім'я (Enter - залишити без змін): ");

            string newName = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Ім'я залишено без змін.");
                return;
            }

            if (newName.Length < 2)
            {
                Console.WriteLine("Ім'я має містити щонайменше 2 символи.");
                return;
            }

            try
            {
                string oldName = currentUser.Name;
                currentUser.Name = newName;
                dataAccessor.UpdateUser(currentUser);
                Console.WriteLine($"Ім'я успішно змінено з '{oldName}' на '{newName}'!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при редагуванні даних: {ex.Message}");
            }
        }

        private void ForgotPassword()
        {
            Console.Write("Введіть ваш E-mail:");
            String email = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("E-mail не може бути порожнім. Процес відновлення скасовано.");
                return;
            }
            Console.Write("Введіть ваше ім'я, вказане при реєстрації: ");
            String name = Console.ReadLine()!;
            String? newPassword;
            try
            {
                newPassword = dataAccessor.ResetPassword(email, name);
            }
            catch
            {
                Console.WriteLine("Виникла помилка, процес зупинено");
                return;

            }
            if(newPassword != null)
            {
                EmailService.SendNewPassword(email, newPassword);
                Console.WriteLine("Sent");
            }
            Console.WriteLine("Якщо ви ввелі дані правильно, то на вашу пошту надіслано новий пароль");
        }

        private void SignIn()
        {
            String email;
            Console.Write("Введіть E-mail: ");
            email = Console.ReadLine()!;
            
            Console.Write("Введіть пароль (символи не будуть зображатись, ESC - повтор): ");
            String? password;
            do
            {
                Console.WriteLine();
                Console.Write("> ");
                password = InputPassword();
            } while (password == null);

            //Console.WriteLine(password);
            Console.WriteLine();
            UserEntity? userEntity = dataAccessor.Authenticate(email, password);
            if (userEntity == null)
            {
                Console.WriteLine("У вході відмовлено");
                return;
            }
            currentUser = userEntity;
            try
            {
                EmailService.SendLoginNotification(userEntity.Email, DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося надіслати сповіщення: {ex.Message}");
            }

            Console.WriteLine($"Ви успішно увійшли як {userEntity.Name}");
            if (userEntity.ConfirmCode != null)
            {
                Console.WriteLine(
                    $"У вас не підтверджена пошта, {userEntity.ConfirmCodeSentAt} " +
                    $"вам на пошту було надіслано код");
                Console.Write("Введіть його для підтвердження: ");
                int tries = 3;
                String code;
                while(true)
                {
                    tries -= 1;
                    if (tries < 0)
                    {
                        Console.WriteLine("Ви вичерпали всі спроби, пошта лишається непідтвердженою");
                        return;
                    }
                    Console.Write("Введіть код (Enter - вихід): ");
                    code = Console.ReadLine()!;
                    if (code ==  "")
                    {
                        Console.WriteLine("Пошта лишається непідтвердженою");
                        return;
                    }
                    if (code == userEntity.ConfirmCode)
                    {
                        try { 
                            dataAccessor.ConfirmEmail(userEntity); 
                            userEntity.ConfirmCode = null;
                        } 
                        catch { return; }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Код не прийнято");
                    }
                }
            }
        }

        private String? InputPassword()
        {
            StringBuilder sb = new();
            ConsoleKeyInfo keyInfo;
            while (true)
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape) return null;
                if (keyInfo.Key == ConsoleKey.Enter) break;
                if (keyInfo.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                else
                {
                    sb.Append(keyInfo.KeyChar);
                }
            }
            return sb.ToString();
        }

        private void SignUp()
        {
            Console.WriteLine("\nРеєстрація нового користувача");
            String email = "";
            while (true) 
            {
                Console.Write("Введіть E-mail: ");
                email = Console.ReadLine()!;

                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("E-mail не може бути порожнім. Реєстрацію скасовано.");
                    return; 
                }
                if (!Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
                {
                    Console.WriteLine("E-mail не відповідає формату, відкоригуйте");
                    return;
                }
                try
                {
                    if (dataAccessor.IsEmailUsed(email))
                    {
                        Console.WriteLine("Цей E-mail вже зареєстрований! Використайте інший.");
                        continue;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка перевірки: {ex.Message}");
                    continue;
                }

            }
            Console.Write("Створіть пароль: ");
            String? password = "";
            while (true)
            {
                password = InputPassword();
                if (password == null)
                {
                    Console.WriteLine("Реєстрацію скасовано.");
                    return;
                }
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
