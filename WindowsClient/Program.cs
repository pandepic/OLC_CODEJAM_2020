using System;

namespace WindowsClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new WindowsClient())
                game.Run();
        }
    }
}
