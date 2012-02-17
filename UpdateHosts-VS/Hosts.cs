using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Win32;

namespace UpdateHosts
{
    /// <summary>
    /// Hosts file class.
    /// </summary>
    public class Hosts
    {
        /// <summary>
        /// Path to hosts file.
        /// </summary>
        private string path;

        /// <summary>
        /// Hosts and comment lines list.
        /// </summary>
        private List<Host> hosts;

        /// <summary>
        /// Initialize hosts with file path.
        /// </summary>
        /// <param name="path">path to hosts file</param>
        public Hosts(string path)
        {
            this.path = path;
            this.hosts = new List<Host>();
        }

        /// <summary>
        /// Parse hosts file (and write to console in DEBUG).
        /// </summary>
        public void parse()
        {
            // If file exists...
            if (File.Exists(path)) {
                string line;
                StreamReader file = new StreamReader(path);

                while ((line = file.ReadLine()) != null)
                {
#if DEBUG
                    Console.WriteLine(line);
#endif
                    hosts.Add(new Host(line));
                }

                file.Close();
            }
        }

        /// <summary>
        /// Save new hosts file.
        /// </summary>
        public void save()
        {
            StreamWriter file = new StreamWriter(path);
            file.Write(get());
            file.Close();
        }

        /// <summary>
        /// Print new hosts file to console.
        /// </summary>
        public void print()
        {
            Console.WriteLine(get());
        }

        /// <summary>
        /// Return new hosts file.
        /// </summary>
        /// <returns>new hosts file</returns>
        private string get()
        {
            string data = "";
            foreach (Host host in hosts)
            {
                data = data + host.get();
            }
            return data;
        }

        /// <summary>
        /// Perform action from request.
        /// </summary>
        /// <param name="request">request</param>
        public void performAction(Request request)
        {
            // Help and Error action can not be performed...
            if ((request.getAction() == Action.Help) || (request.getAction() == Action.Error))
            {
                throw new Exception("Bad action to make");
            }

            // Save request IP address and hostnames...
            string ip = request.getIp();
            List<string> hostnames = request.getHostnames();

            // Run action...
            switch (request.getAction())
            {
                case Action.Add :
                    // Add only hostnames that does not exists in hosts file...
                    List<string> addHostnames = new List<string>();
                    foreach (string hostname in hostnames)
                    {
                        Host[] hostsHostname = getHostsByHostname(hostname);
                        if (hostsHostname.Length > 0)
                        {
                            foreach (Host hostHostname in hostsHostname)
                            {
                                Console.WriteLine("WARNING: hostname \"" + hostname + "\" is already used with IP address \"" + hostHostname.getIp() + "\"");
                            }
                        }

                        if (contains(ip, hostname))
                        {
                            Console.WriteLine("NOT ADDED: \"" + ip + " - " + hostname + "\" combination already exists in hosts file");
                        }
                        else
                        {
                            addHostnames.Add(hostname);
                        }
                    }

                    // Add new host...
                    hosts.Add(new Host(ip, addHostnames, request.getComment()));

                    break;
                case Action.Remove :
                    // Write non existing combination to console...
                    foreach (string hostname in hostnames)
                    {
                        if (!contains(ip, hostname))
                        {
                            Console.WriteLine("NOT REMOVED: \"" + ip + " - " + hostname + "\" combination not exists in hosts file");
                        }
                    }

                    // Get hosts and removed their hostnames...
                    foreach (Host host in getHostsByIp(ip))
                    {
                        foreach (string hostname in hostnames)
                        {
                            if (host.hasHostname(hostname))
                            {
                                host.removeHostname(hostname);
                            }
                        }
                    }

                    break;
                case Action.Test :
                    // Determine if IP address and hostname exists in hosts file...
                    foreach (string hostname in hostnames)
                    {
                        if (contains(ip, hostname))
                        {
                            Console.WriteLine("EXISTS: \"" + ip + " - " + hostname + "\" combination exists in hosts file");
                        }
                        else
                        {
                            Console.WriteLine("NOT EXISTS: \"" + ip + " - " + hostname + "\" combination not exists in hosts file");
                        }
                    }

                    break;
            }

            // Save (print on DEBUG) while Add or Remove action...
            if ((request.getAction() == Action.Add) || (request.getAction() == Action.Remove))
            {
#if DEBUG
                print();
#else
                save();
#endif
            }
        }

        /// <summary>
        /// Get hosts with IP address.
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>hosts array</returns>
        private Host[] getHostsByIp(string ip)
        {
            List<Host> hostsIp = new List<Host>();

            foreach (Host host in hosts)
            {
                if (host.isIp(ip))
                {
                    hostsIp.Add(host);
                }
            }

            return hostsIp.ToArray();
        }

        /// <summary>
        /// Get hosts that contains hostname.
        /// </summary>
        /// <param name="hostname">hostname</param>
        /// <returns>hosts array</returns>
        private Host[] getHostsByHostname(string hostname)
        {
            List<Host> hostsHostname = new List<Host>();

            foreach (Host host in hosts)
            {
                if (host.hasHostname(hostname))
                {
                    hostsHostname.Add(host);
                }
            }

            return hostsHostname.ToArray();
        }

        /// <summary>
        /// Return if hosts contains host with IP address and hostname.
        /// </summary>
        /// <param name="ip">IP hostname</param>
        /// <param name="hostname">hostname</param>
        /// <returns>true if hosts contains host, false otherwise</returns>
        private bool contains(string ip, string hostname)
        {
            foreach (Host host in hosts)
            {
                if (host.isIp(ip) && host.hasHostname(hostname))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Recognize host file path on currecnt Windows version.
        /// </summary>
        /// <returns>Hosts file path</returns>
        public static string getHostsFilePath()
        {
            string hostsFile = "";

            // Try recognise path from registry...
            RegistryKey rk = Registry.LocalMachine;

            try
            {
                RegistryKey dataBasePath = rk.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\");

                hostsFile = (string) dataBasePath.GetValue("DataBasePath");
            }
            catch
            {
                rk.Close();
            }

            // Try recognize from OS version...
            if (hostsFile == "")
            {
                System.OperatingSystem osInfo = System.Environment.OSVersion;

                switch (osInfo.Platform)
                {
                    case System.PlatformID.Win32Windows:
                        hostsFile = @"%WinDir%";
                        break;

                    case System.PlatformID.Win32NT:
                        hostsFile = @"%SystemRoot%\system32\drivers\etc";
                        break;
                }

            }

            // If directory not exists, create it...
            if (!Directory.Exists(hostsFile))
            {
                Directory.CreateDirectory(hostsFile);
            }

            return hostsFile + @"\hosts";
        }
    }

    /// <summary>
    /// Host class.
    /// </summary>
    public class Host
    {
        /// <summary>
        /// Raw data for comments and blank lines.
        /// </summary>
        private string raw;

        /// <summary>
        /// Is host or comment (blank) line.
        /// </summary>
        private bool isHost;

        /// <summary>
        /// Host IP address.
        /// </summary>
        private string ip;

        /// <summary>
        /// Host hostnames.
        /// </summary>
        private List<string> hostnames;

        /// <summary>
        /// Host comment.
        /// </summary>
        private string comment;

        /// <summary>
        /// Initialization constructor.
        /// </summary>
        private Host()
        {
            raw = "";
            isHost = false;
            ip = "";
            hostnames = new List<string>();
            comment = "";
        }

        /// <summary>
        /// Create (and parse) host from line in hosts file.
        /// </summary>
        /// <param name="line">line in hosts file</param>
        public Host(string line) : this()
        {
            // Save line as raw, maybe it is not host...
            raw = line;
            
            line = line.Trim();

            // Determine if it is host line...
            if ((line.Length > 0) && !line.StartsWith("#"))
            {
                isHost = true;

                int commentIndex = line.IndexOf('#');
                string data = "";
                
                // If host has no comment...
                if (commentIndex == -1)
                {
                    data = line;
                }
                else
                {
                    // Split data and comment...
                    data = line.Substring(0, commentIndex);
                    comment = line.Substring(commentIndex + 1);
                }

                // Remove multiple white character from data...
                data = Regex.Replace(data.Trim(), @"(\s)+", " ").ToLower();

                // Split to many parts...
                String[] parts = data.Split(' ');

                // Save IP address...
                if (parts.Length > 0)
                {
                    ip = parts[0];
                }

                // Save hostnames...
                if (parts.Length > 1)
                {
                    for (int i = 1; i < parts.Length; i++)
                    {
                        hostnames.Add(parts[i]);
                    }
                }
                else // No hostname, so write raw line...
                {
                    isHost = false;
                }
            }
        }

        /// <summary>
        /// Create new host from IP address, hostnames list and comment.
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="hostnames">hostnames list</param>
        /// <param name="comment">comment</param>
        public Host(string ip, List<string> hostnames, string comment) : this()
        {
            this.isHost = true;
            this.ip = ip;
            this.hostnames = hostnames;
            this.comment = comment;
        }

        /// <summary>
        /// Determine if host has IP address.
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>true if host has IP address, false otherwise (or this is not host)</returns>
        public bool isIp(string ip)
        {
            if (!isHost)
            {
                return false;
            }

            return this.ip.Equals(ip);
        }

        /// <summary>
        /// Get IP address.
        /// </summary>
        /// <returns>IP address</returns>
        public string getIp()
        {
            return ip;
        }

        /// <summary>
        /// Determine if host contains hostname.
        /// </summary>
        /// <param name="hostname">hostname</param>
        /// <returns>true if host contains hostname, false otherwise (or this is not host)</returns>
        public bool hasHostname(string hostname)
        {
            if (!isHost)
            {
                return false;
            }

            return hostnames.Contains(hostname.ToLower());
        }

        /// <summary>
        /// Remove hostname from host.
        /// </summary>
        /// <param name="hostname">hostname</param>
        /// <returns>true if host was successfully removed, false otherwise (or hostname not found or this is not host)</returns>
        public bool removeHostname(string hostname)
        {
            if (!isHost)
            {
                return false;
            }

            return hostnames.Remove(hostname);
        }

        /// <summary>
        /// Get host string representative (can be blank if no hostname) or raw data while host is comment (or blank) line.
        /// </summary>
        /// <returns>host string representative</returns>
        public string get()
        {
            if (isHost)
            {
                if (hostnames.Count > 0)
                {
                    return ip + " " + String.Join(" ", hostnames.ToArray()) + (comment.Length > 0 ? (" #" + comment) : "") + Environment.NewLine;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return raw + Environment.NewLine;
            }
        }
    }
}
