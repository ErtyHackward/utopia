using S33M3Engines;
namespace Utopia
{
    static class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "WithDebug") S33M3Engines.D3D.DebugTools.GameConsole.Actif = true;
            }

            using (UtopiaClient main = new UtopiaClient())
            {
                main.Run();
            }
        }
    }
}
