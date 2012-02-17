using System;

namespace UpdateHosts
{
    /// <summary>
    /// Main program class.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Application version.
        /// </summary>
        private const string VERSION = "2.0.0";

        /// <summary>
        /// Main program method. Read command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            // Read request from args...
            Request request = new Request(args);

            // Help or Error...
            switch (request.getAction())
            {
                case Action.Help:
                    printHelp();
                    return;
                case Action.Error:
                    Console.WriteLine("Bad parameters used.");
                    Console.WriteLine("");
                    printHelp();
                    return;
            }

            // Read hosts file...
            Hosts hosts = new Hosts(Hosts.getHostsFilePath());
            hosts.parse();

            // Run request...
            hosts.performAction(request);
        }

        /// <summary>
        /// Print informations and help to console.
        /// </summary>
        private static void printHelp()
        {
            Console.WriteLine("UpdateHosts v" + VERSION);
            Console.WriteLine("");
            Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName + " settings ip hostname1 [hostname2 .. hostnameN] [--comment|-c comment]");
            Console.WriteLine("");
            Console.WriteLine("  Settings: --help, -h       show this help");
            Console.WriteLine("            --add, -a        add new host");
            Console.WriteLine("            --remove, -r     remove host");
            Console.WriteLine("            --test, -t       test if host exists");
            Console.WriteLine("");
            Console.WriteLine("  Note: comment is used only while adding new host");
        }
    }
}
