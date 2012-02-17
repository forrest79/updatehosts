UpdateHosts © Jakub Trmota, 2012 (http://forrest79.net)


Small utility for add/remove/test host in hosts file written in C# for Windows 95/98/ME/2000/XP/VISTA.


HOW TO USE:
===========
updhosts settings ip hostname1 [hostname2 .. hostnameN] [--comment|-c comment]

Settings: --help, -h       show this help
          --add, -a        add new host
          --remove, -r     remove host
					--test, -t       test if host exists

Note: comment is used only while adding new host


HISTORY
=======
1.0.0 [2009-02-04] - Firest public version
2.0.0 [2012-02-17] - Code refactoring, more hostnames can be processed at once, test hostnames...


LICENSE
=======
UpdateHosts is distributed under BSD license. See license.txt.


REQUIREMENTS
============
You need .NET Framework 2 to run this application (http://www.microsoft.com/downloads/details.aspx?familyid=0856eacb-4362-4b0d-8edd-aab15c5e04f5&displaylang=en).


https://github.com/forrest79/updatehosts
