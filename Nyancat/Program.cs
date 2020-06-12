using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nyancat
{
    public class Program
    {
        private const string RAINBOW_TAIL = ",,>>&&&+++###==;;;,,";
        private static readonly ManualResetEvent _shutdownBlock = new ManualResetEvent(false);
        public static int Main(string[] args)
        {
            try
            {
                var running = true;
                var options = Options.Parse(args);

                if (options.ShowVersion)
                {
                    Console.WriteLine(GetVersion());
                    return 0;
                }

                if (options.ShowHelp)
                {
                    Console.WriteLine($"{GetName()}: {GetVersion()}");
                    Console.WriteLine("  Terminal nyancat runner");
                    Console.WriteLine();
                    Console.WriteLine($"Usage:");
                    Console.WriteLine($"  {GetName()} [options]");
                    Console.WriteLine();
                    Console.WriteLine("Options:");
                    Console.WriteLine("  -i, --intro                Show the introduction / about information at startup.");
                    Console.WriteLine("  -n, --no-counter           Do not display the timer.");
                    Console.WriteLine("  -t, --no-title             Do not set the titlebar text.");
                    Console.WriteLine("  -f, --frames=VALUE         Display the requested number of frames, then quit.");
                    Console.WriteLine("  -v, --version              Show version information.");
                    Console.WriteLine("  -?, -h, --help             Show help information");

                    return 0;
                }

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    running = false;
                    _shutdownBlock.WaitOne();
                };

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    running = false;
                };

                var console = new ConsoleGraphics(buffered: true);

                if (options.ShowTitle) console.Title = "Nyanyanyanyanyanyanya...";

                console
                    .HideCursor()
                    .ResetCursor()
                    .Flush();

                var defaultSleep = GetDefaultSleep();
                var startTime = Environment.TickCount64;
                var renderTime = startTime;

                var showIntro = options.ShowIntro;
                var scene = showIntro ? new IntroScene() : (IScene)new NyancatScene(frames: options.Frames, showCounter: options.ShowCounter);

                while (running)
                {
                    console.ResetCursor();

                    if (!scene.Update(console.Width, console.Height))
                    {
                        if (showIntro)
                        {
                            scene = new NyancatScene(frames: options.Frames, showCounter: options.ShowCounter);
                            scene.Update(console.Width, console.Height);
                            showIntro = false;
                        }
                        else
                        {

                            running = false;
                        }
                    }

                    scene.Render(ref console);

                    console.Flush();

                    var elapsed = Environment.TickCount64 - renderTime;
                    var sleep = (defaultSleep * 2) - elapsed;
                    Thread.Sleep((int)Math.Clamp(sleep, 0L, defaultSleep));
                    renderTime = Environment.TickCount64;
                }

                console.Dispose();
                _shutdownBlock.Set();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("Try `nyancat --help' for more information.");
                return 1;
            }
        }

        private static long GetDefaultSleep() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 60 : 90;

        private static string GetVersion() => "v1.3.0";

        private static string GetName() => "nyancat";
    }
}
