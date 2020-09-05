using System;
using System.IO;

namespace WindowsClient
{
    class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            try
            {
#endif
            using (var game = new WindowsClient())
            {
                game.Run();
            }
#if RELEASE
            }
            catch (Exception ex)
            {
                using (var fs = File.OpenWrite("CrashLog.txt"))
                using (var s = new StreamWriter(fs))
                {
                    s.WriteLine(ex.ToString());
                }
            }
#endif
        }
    }
}
