using System;
using System.Threading.Tasks;

namespace ZTEModem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "ZTE Modem - Rebooter";
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("----------------------");
            Console.ResetColor();
            Console.WriteLine("> Loading configuration...");

            var donotask = false;
            foreach (string arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg)) continue;
                if (arg.ToLower() == "/f")
                {
                    donotask = true;
                    break;
                }
            }
            var cm = new ConfigManager();

            Console.Write("  Router   : ");
            var router = cm.GetString(ConfigManager.KEY_ROUTER);
            if (string.IsNullOrWhiteSpace(router))
            {
                router = askInput("  Router   : ");
                cm.SetString(ConfigManager.KEY_ROUTER, router);
            }
            else Console.WriteLine(router);

            Console.Write("  Username : ");
            var username = cm.GetString(ConfigManager.KEY_USERNAME);
            if (string.IsNullOrWhiteSpace(username))
            {
                username = askInput("  Username : ");
                cm.SetString(ConfigManager.KEY_USERNAME, username);
            }
            else Console.WriteLine(username);

            Console.Write("  Password : ");
            var password = cm.GetString(ConfigManager.KEY_PASSWORD);
            if (string.IsNullOrWhiteSpace(password))
            {
                password = askInput("  Password : ", true);
                cm.SetString(ConfigManager.KEY_PASSWORD, password);
            }
            else Console.WriteLine(password.ToAsterisk());

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n----------------------");
            Console.ResetColor();
            
            var modem = new Modem(router);

            Console.WriteLine("> Login to router...");
            var login = await modem.Login(username, password);
            Console.WriteLine("  Login Status : " + (login ? "OK" : "FAILED"));
            if (!login) PressAnyKeyToExit();

            Console.WriteLine();
            Console.WriteLine("> Get device info...");
            var device = await modem.GetDeviceInfo();
            Console.WriteLine("  Model              : " + device.Model);
            Console.WriteLine("  Serial Number      : " + device.SerialNumber);
            Console.WriteLine("  Hardware Version   : " + device.HardwareVersion);
            Console.WriteLine("  Software Version   : " + device.SoftwareVersion);
            Console.WriteLine("  Bootloader Version : " + device.BootloaderVersion);
            Console.WriteLine("  Pon Serial Number  : " + device.PonSerialNumber);
            Console.WriteLine("  Batch Number       : " + device.BatchNumber);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("> Reboot the device? [Y/n] : ");
            Console.ResetColor();
            var todo = donotask ? ConsoleKey.Y : Console.ReadKey().Key;
            if (todo == ConsoleKey.Y || todo == ConsoleKey.Enter)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine("> Rebooting device...           ");
                await modem.Reboot();
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.ResetColor();
                Console.WriteLine("> Cancel rebooting the device...");
            }

            modem.Close();

            PressAnyKeyToExit();
        }

        static string askInput(string question, bool protect = false)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (protect)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(question + input.ToAsterisk());
                    }
                    return input;
                }
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(question);
            }
        }

        static void PressAnyKeyToExit()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n----------------------");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
