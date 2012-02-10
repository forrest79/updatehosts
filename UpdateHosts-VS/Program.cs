using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace UpdateHosts
{
    class Program
    {
        private static string version = "1.0.1";
        private static string file = "updhost.exe";

        static void Main(string[] args)
        {
            string ip = "";
            string hostName = "";
            string comment = "";

            if ((args.Length == 0) || (args[0].ToLower() == "--help") || (args[0].ToLower() == "-h"))
            {
                Console.WriteLine("UpdateHost v" + version);
                Console.WriteLine("");
                Console.WriteLine("  " + file + " [settings] ip host_name [comment]");
                Console.WriteLine("");
                Console.WriteLine("  Settings: --help -h       show this help");
                Console.WriteLine("            --add -a        add new host");
                Console.WriteLine("            --remove -r     remove host");
                Console.WriteLine("");
                Console.WriteLine("  Note: comment is used only while adding new host");
            }
            else if (((args[0].ToLower() == "--add") || (args[0].ToLower() == "-a")) && (args.Length >= 3))
            {
                ip = args[1];
                hostName = args[2];

                if (args.Length >= 4)
                {
                    comment = args[3];
                }

                string file = getHostFile();

                if (existsHost(file, ip, hostName))
                {
                    Console.WriteLine("This host already exists.");
                }
                else
                {
                    StreamWriter swFile = null;

                    bool isEndWithReturn = endWithReturn(file);

                    try
                    {
                        swFile = File.AppendText(file);

                        swFile.WriteLine((isEndWithReturn ? "" : "\n") + ip + " " + hostName + ((comment == "") ? "" : " #" + comment));

                        Console.WriteLine("Host " + ip + " - " + hostName + " was succesfully added");
                    }
                    catch
                    {
                        Console.WriteLine("An error occured while adding host...");
                    }
                    finally
                    {
                        if (swFile != null)
                        {
                            swFile.Close();
                        }
                    }
                }
            }
            else if (((args[0].ToLower() == "--remove") || (args[0].ToLower() == "-r")) && (args.Length >= 3))
            {
                ip = args[1];
                hostName = args[2];

                string file = getHostFile();

                if (existsHost(file, ip, hostName))
                {
                    string line;

                    if (File.Exists(file))
                    {
                        string newHosts = "";

                        StreamReader srFile = null;

                        try
                        {
                            srFile = new StreamReader(file);

                            Regex regex = new Regex(ip + @"[ \t]+" + hostName, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                            while ((line = srFile.ReadLine()) != null)
                            {
                                if (!regex.IsMatch(line))
                                {
                                    newHosts += line + "\r\n";
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("An error occured while removing host...");
                        }
                        finally
                        {
                            if (srFile != null)
                            {
                                srFile.Close();
                            }
                        }

                        if (newHosts != "")
                        {
                            TextWriter twFile = null;

                            try
                            {
                                twFile = new StreamWriter(file);
                                twFile.WriteLine(newHosts.TrimEnd());

                                Console.WriteLine("Host " + ip + " - " + hostName + " was succesfully removed");
                            }
                            catch
                            {
                                Console.WriteLine("An error occured while removing host...");
                            }
                            finally
                            {
                                if (twFile != null)
                                {
                                    twFile.Close();
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("An error occured while removing host...");
                    }
                }
                else
                {
                    Console.WriteLine("This host doesn't exists.");
                }
            }
            else
            {
                Console.WriteLine("Bad use... Type '" + file + " --help' or '" + file + " -h' for more information.");
            }
        }

        static string getHostFile()
        {
            string file = "";

            RegistryKey rk = Registry.LocalMachine;

            try
            {
                RegistryKey dataBasePath = rk.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\");

                file = (string)dataBasePath.GetValue("DataBasePath");
            }
            catch
            {
                rk.Close();
            }

            if (file == "")
            {
                System.OperatingSystem osInfo = System.Environment.OSVersion;

                switch (osInfo.Platform)
                {
                    case System.PlatformID.Win32Windows:

                        file = @"%WinDir%";

                        break;

                    case System.PlatformID.Win32NT:

                        file = @"%SystemRoot%\system32\drivers\etc";

                        break;
                }

            }

            if (!Directory.Exists(file))
            {
                Directory.CreateDirectory(file);
            }

            return file + @"\hosts";
        }

        static bool existsHost(string file, string ip, string hostName)
        {
            if (File.Exists(file))
            {
                System.IO.StreamReader rFile = null;
                string fileContent = "";

                try
                {
                    rFile = new System.IO.StreamReader(file);
                    fileContent = rFile.ReadToEnd();
                }
                finally
                {
                    rFile.Close();
                }

                Regex regex = new Regex(ip + @"[ \t]+" + hostName, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (regex.IsMatch(fileContent))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        static bool endWithReturn(string file)
        {
            if (File.Exists(file))
            {
                System.IO.StreamReader rFile = null;
                string fileContent = "";

                try
                {
                    rFile = new System.IO.StreamReader(file);
                    fileContent = rFile.ReadToEnd();
                }
                finally
                {
                    rFile.Close();
                }

                if (fileContent.EndsWith("\n"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
