using System.Collections.Generic;

namespace UpdateHosts
{
    /// <summary>
    /// Action enumerations.
    /// </summary>
    public enum Action
    {
        /// <summary>
        /// Add host.
        /// </summary>
        Add,
        /// <summary>
        /// Remove host.
        /// </summary>
        Remove,
        /// <summary>
        /// Test if host exists.
        /// </summary>
        Test,
        /// <summary>
        /// Show info and help text.
        /// </summary>
        Help,
        /// <summary>
        /// Error in request.
        /// </summary>
        Error
    }

    /// <summary>
    /// Read and parse user request.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// IP address.
        /// </summary>
        private string ip;

        /// <summary>
        /// List of hostnames.
        /// </summary>
        private List<string> hostnames;

        /// <summary>
        /// Comment.
        /// </summary>
        private string comment;

        /// <summary>
        /// Action to do.
        /// </summary>
        private Action action;

        /// <summary>
        /// Create new request.
        /// </summary>
        /// <param name="args">user arguments</param>
        public Request(string[] args)
        {
            // Defaults...
            this.action = Action.Error;
            this.comment = "";

            // Initialization...
            this.hostnames = new List<string>();

            // Resolve action...
            if (args.Length > 0) {
                switch (args[0].Trim().ToLower())
                {
                    case "--help":
                    case "-h":
                        this.action = Action.Help;
                        break;

                    case "--add":
                    case "-a":
                        this.action = Action.Add;
                        break;

                    case "--remove":
                    case "-r":
                        this.action = Action.Remove;
                        break;

                    case "--test":
                    case "-t":
                        this.action = Action.Test;
                        break;

                }
            }

            // No need to do anything else while help or error...
            if (this.action == Action.Help || this.action == Action.Error) {
                return;
            }

            // Must have min 3 arguments...
            if (args.Length < 3) {
                this.action = Action.Error;
                return;
            }

            // Get IP address
            this.ip = args[1].Trim().ToLower();

            // Read hosts and comment
            bool readComment = false;
            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].Trim();

                if (readComment)
                {
                    this.comment = arg;
                    break;
                }
                else if (arg.ToLower() == "--comment" || arg.ToLower() == "-c")
                {
                    readComment = true;
                    continue;
                }
                else if (arg.StartsWith("-"))
                {
                    this.action = Action.Error;
                    return;
                }
                else
                {
                    arg = arg.ToLower();
                    if (!this.hostnames.Contains(arg))
                    {
                        this.hostnames.Add(arg);
                    }
                }
            }
        }

        /// <summary>
        /// Get action.
        /// </summary>
        /// <returns>Action</returns>
        public Action getAction()
        {
            return this.action;
        }

        /// <summary>
        /// Get IP address.
        /// </summary>
        /// <returns>IP address</returns>
        public string getIp()
        {
            return this.ip;
        }

        /// <summary>
        /// Get hostnames list.
        /// </summary>
        /// <returns>hostnames list</returns>
        public List<string> getHostnames()
        {
            return this.hostnames;
        }

        /// <summary>
        /// Get comment.
        /// </summary>
        /// <returns>comment</returns>
        public string getComment()
        {
            return this.comment;
        }
    }
}
