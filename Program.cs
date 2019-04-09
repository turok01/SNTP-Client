/*
 * Создано в SharpDevelop.
 * Пользователь: kristina
 * Дата: 08.04.2019
 * Время: 20:34
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Runtime.InteropServices; // для DllImport, StructLayout
using System.Net;
using System.Net.Sockets;

namespace SetDateTime
{

    class Program
    {
        public static DateTime GetNetworkTimeUtc(string ntpServer = "time.windows.com")
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            // NTP работает через UDP и использует порт 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is Blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);
            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            return (new DateTime(1900,1,1,0,0,0,DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

        }
        // Convert From big-endian to little-endian
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME time);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetSystemTime(ref SYSTEMTIME time);
        public static void SetDateTime(DateTime dt)
        {
            SYSTEMTIME sysTime = new SYSTEMTIME();
            dt = dt.ToUniversalTime();
            Console.WriteLine("Обработка указанного времени: {0}", dt);
            sysTime.wYear = (short)dt.Year;
            sysTime.wMonth = (short)dt.Month;
            sysTime.wDayOfWeek = (short)dt.DayOfWeek;
            sysTime.wDay = (short)dt.Day;
            sysTime.wHour = (short)dt.Hour;
            sysTime.wMinute = (short)dt.Minute;
            sysTime.wSecond = (short)dt.Second;
            sysTime.wMilliseconds = (short)dt.Millisecond;
            Console.WriteLine("После обработки указанного времени: {0}:{1}:{2}", sysTime.wHour,sysTime.wMinute,sysTime.wSecond);
            SetSystemTime(ref sysTime);
            Console.WriteLine("Текущее время: {0}",DateTime.Now);
    }
        static void Main()
        {
        	string SNTPServer = "www.belgim.by";
            DateTime date1 = new DateTime();
            DateTime date2;
            Boolean boolexit = false;
            SYSTEMTIME sysTime = new SYSTEMTIME();
            short m, h;
            //GetSystemTime(ref sysTime);
            //Console.WriteLine("{0}:{1}:{2}",sysTime.wHour,sysTime.wMinute,sysTime.wSecond);
            do
            {
                GetSystemTime(ref sysTime);
                date1 = DateTime.Now;
                Console.WriteLine("Текущее время: {0}", date1);
                Console.WriteLine("Текущее время sntp: {0}", GetNetworkTimeUtc(SNTPServer));
                Console.WriteLine("Введите смещение в минутах -1,0,1,n. Для выхода нажмите Enter: ");
                string offset = Console.ReadLine();
                switch (offset)
                {
                    case "":
                        boolexit = true;
                        break;
                    case "n":
                        SetDateTime(GetNetworkTimeUtc(SNTPServer));
                        break;
                    case "-1":
                        date1 = DateTime.Now;
                        date1 = date1.AddMinutes(-1);
                        date1 = date1.ToUniversalTime();
                        h = (short)date1.Hour;
                        m = (short)date1.Minute;
                        sysTime.wHour = h;
                        sysTime.wMinute = m;
                        sysTime.wSecond = (short)date1.Second;
                        SetSystemTime(ref sysTime);
                        boolexit = false;
                        break;
                    case "1":
                        //date1=DateTime.Now;
                        //date1=date1.AddMinutes(1);
                        //date1=date1.ToUniversalTime();
                        date1 = DateTime.Now.AddMinutes(1).ToUniversalTime();
                        h = (short)date1.Hour;
                        m = (short)date1.Minute;
                        sysTime.wHour = h;
                        sysTime.wMinute = m;
                        sysTime.wSecond = (short)date1.Second;
                        SetSystemTime(ref sysTime);
                        boolexit = false;
                        break;
                    default:
                        boolexit = false;
                        break;
                }
            }
            while (!boolexit);
            //Console.WriteLine(date1);

            //Console.ReadKey();

        }

    }


}