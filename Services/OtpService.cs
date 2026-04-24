using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammingP47.Services
{
    internal class OtpService
    {
        private static readonly Random random = new();
        public static String ConfirmCode(int length = 6)
        {
            ArgumentOutOfRangeException
                .ThrowIfLessThan<int>(length, 1);

            //var sb = new StringBuilder(length);
            //for (int i = 0; i < length; i++)
            //{
            //    sb.Append(random.Next(0, 10));
            //}
            //
            //return sb.ToString();
            return String.Join("", 
                Enumerable.Range(0, length).Select(_ => random.Next(0, 10)));
        }

        public static String TempPassword(int length = 6)
        {
            ArgumentOutOfRangeException
                .ThrowIfLessThan<int>(length, 1);
            //var sb = new StringBuilder(length);
            //for (int i = 0; i < length; i++)
            //{
            //    sb.Append((char)random.Next(33, 127));
            //}   
            //return sb.ToString();

            return String.Join("",
                Enumerable.Range(0, length).Select(_ => (char)random.Next(33, 127)));
        }
    }
}
