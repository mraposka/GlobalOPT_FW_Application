using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace GlobalOPT_FW_Application
{
    internal class Program
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        static bool softwareInstallation = false;
        static string strCmdText;
        static string tag = "fw";
        static string name = "opt";
        static string gitRepoURL = "https://github.com/mraposka/DockerOPT";
        static string gitRepoName = gitRepoURL.Split('/').Last();
        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string winDir = Path.GetPathRoot(System.Environment.GetEnvironmentVariable("WINDIR"));
        static string gitRepoClonePath = desktopPath + "\\GlobalOPTFW\\" + gitRepoName + "\\globalopt";
        static string dockerFilePath = desktopPath + "\\GlobalOPTFW\\" + gitRepoName;
        static string textFilePath = desktopPath + "\\result.txt";
        static string algorithm = "", iteration = "", threshold = "", depth = "", num_matrices = "", bfi = "";
        static void Main(string[] args)
        {
            //<summary>
            //---To-do list---
            // 
            //command succesful check  
            //parametreler algoritmaya göre istenecek
            //  
            //---To-do list---
            //</summary> 

            Echo("Welcome to the GlobalOPT");
            Thread.Sleep(1000);
            Echo("Checking required softwares...");
            bool isDockerRunning = false;//Is docker running variable
            //Is docker running checking
            Process[] pname = Process.GetProcessesByName("Docker Desktop");
            if (pname.Length == 0)
            {
                pname = Process.GetProcessesByName("Docker Desktop Backend");
                if (pname.Length == 0)
                {
                    KillDocker(); Echo("Docker is not running or installed!");
                }
                else
                { isDockerRunning = true; Echo("Docker is running!"); }
            }
            else
            {
                isDockerRunning = true; Echo("Docker is running!");
            }
            //Is docker running checking
            InstallRequiredSoftwares();//Install docker and git if its not installed
            try
            {
                File.Delete("gitinstaller.ps1");//Delete Git Installer shell script for cleaning
                File.Delete("dockerinstaller.ps1");//Delete Docker Installer shell script for cleaning
            }
            catch { }
            if (softwareInstallation)//If any software installed
                Echo("Checking completed. System now has all required softwares");
            else
                Echo("Checking completed. System has all required softwares");
            Echo("Docker starting.");
            if (!isDockerRunning)//is docker not running start it
            {
                StartDocker();
                Thread.Sleep(10000);//docker engine will startup in 10s(idk why)
            }
            Echo("Docker started.");
            ClearAndUpdate();//Clear old builds and update repo if need
            Echo("Insert Parameters");
            //Taking params
            Console.WriteLine("Enter 0 For Skip All Parameters");
            Console.WriteLine("Leave Empty For Skip Parameter");
            Console.Write("Select an Algorithm: ");
            algorithm = Console.ReadLine();
            if (algorithm != "0")//If 0 selected for algorithm it will use default settings(on git repo) or if you have your params from last time it will use it
            {
                Console.Write("Number of Iteration: ");
                iteration = Console.ReadLine();
                Console.Write("Threshold Value: ");
                threshold = Console.ReadLine();
                Console.Write("Depth Value: ");
                depth = Console.ReadLine();
                Console.Write("Number of Matrices: ");
                num_matrices = Console.ReadLine();
                Console.Write("BFI Value: ");
                bfi = Console.ReadLine();
                Echo("Parameters saved");
            }
            else
            {
                Echo("Last parameters will be used");
            }
            //Taking params
            if (!Directory.Exists(gitRepoClonePath)) GitClone();//if repo does not exists clone it
            CreateDockerImage();//create docker image with git repo
            RunDockerBuild();//run the docker image
            Console.ReadKey();
        }
        static void RunDockerBuild()//run the docker image
        {
            //Runs created docker build
            Echo("Executing...");
            Thread.Sleep(100);
            strCmdText = "/C @echo off &  cd " + gitRepoClonePath + " & docker run " + tag + ":" + name;
            string output = RunCommand();
            Echo("Execution completed!");
            //Runs created docker build
            //Saving results in result.txt file 
            Echo("Saving results...");
            string buildOutput = "------Build Time:" + DateTime.Now + "------\n" + output + "------Build Finished------\n";
            if (!File.Exists(textFilePath))
                File.Create(textFilePath).Close();//Create result.txt file if does not exist
            File.AppendAllText(textFilePath, buildOutput);
            Echo("Result file succesfully written on result file on Desktop\n\nPress any key to exit");
            //Saving results in result.txt file
        }
        static void InstallRequiredSoftwares()//Install docker and git if its not installed
        {
            GitInstall();
            DockerInstall();
        }
        static void GitInstall()//Installing Git via powershell
        {
            Echo("Checking Git...");
            strCmdText = "/C git -v";
            string output = RunCommand();//If git is installed output will be version of git. So it cant be null
            if (string.IsNullOrEmpty(output))
            {
                //Installing Git
                Echo("Git not found. Installing Git...");
                string installCommand = "$git_url = \"https://api.github.com/repos/git-for-windows/git/releases/latest\"\r\n$asset = Invoke-RestMethod -Method Get -Uri $git_url | % assets | where name -like \"*64-bit.exe\"\r\n# download installer\r\n$installer = \"$env:temp\\$($asset.name)\"\r\nInvoke-WebRequest -Uri $asset.browser_download_url -OutFile $installer\r\n# run installer\r\n$git_install_inf = \"<install inf file>\"\r\n$install_args = \"/SP- /VERYSILENT /SUPPRESSMSGBOXES /NOCANCEL /NORESTART /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS /LOADINF=\"\"$git_install_inf\"\"\"\r\nStart-Process -FilePath $installer -ArgumentList $install_args -Wait";
                File.Create("gitinstaller.txt").Close();
                File.WriteAllText("gitinstaller.txt", installCommand);
                File.Move("gitinstaller.txt", Path.ChangeExtension("gitinstaller.txt", ".ps1"));
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"gitinstaller.ps1\"",
                    Verb = "runas",
                    UseShellExecute = false
                };
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                Echo("Git installed");
                softwareInstallation = true;//is any software installed
                Restart();//Restarting program for git commands
                //Installing Git 
            }
            else
                Echo("Git found.");
        }
        static void Restart()//Restart the application
        {
            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }
        static void DockerInstall()//Installing Docker via powershell
        {
            Echo("Checking Docker...");
            strCmdText = "/C docker -v";
            string output = RunCommand();//If docker is installed output will be version of git. So it cant be null
            if (string.IsNullOrEmpty(output))
            {
                //Installing Choco And Docker
                Echo("Docker not found. Installing Docker...");
                //choco uninstalling (for reinstall *temporary solution*) 
                try { Directory.Delete(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\chocolatey"); } catch { }

                //choco uninstalling (for reinstall *temporary solution*)
                string installCommand = "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString(\"https://chocolatey.org/install.ps1\"))\r\nchoco install docker-desktop -y";
                File.Create("dockerinstaller.txt").Close();
                File.WriteAllText("dockerinstaller.txt", installCommand);
                File.Move("dockerinstaller.txt", Path.ChangeExtension("dockerinstaller.txt", ".ps1"));
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"dockerinstaller.ps1\"",
                    Verb = "runas",
                    UseShellExecute = false
                };
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                Echo("Docker installed");
                softwareInstallation = true;//is any software installed
                //Installing Choco And Docker 
            }
            else
                Echo("Docker found.");

        }
        static void ProcKil(string proc)//Kills a process(like notepad.exe)
        {
            foreach (var process in Process.GetProcessesByName(proc))
            {
                process.Kill();
            }
        }
        static void KillDocker()//Kill all docker process
        {
            ProcKil("docker");
            ProcKil("com.docker.vpnkit");
            ProcKil("com.docker.proxy");
            ProcKil("com.docker.backend");
            ProcKil("Docker Desktop");
        }
        static void StartDocker()//Starts docker.backend
        {
            bool procK = false;
            var proc = Process.Start(winDir + @"Program Files\Docker\Docker\resources\com.docker.backend.exe");//Docker backend path (default)
            while (!procK)
            {
                foreach (var process in Process.GetProcessesByName("Docker Desktop"))
                { process.Kill(); procK = true; }

            }
            while (proc.MainWindowHandle == IntPtr.Zero) //note: only works as long as your process actually creates a main window.
                System.Threading.Thread.Sleep(10);
            ShowWindow(proc.MainWindowHandle, 0);//Hide command window for better visualization
        }

        static void CreateDockerImage()//Building docker image with params
        {
            //Building docker image
            Echo("Getting parameters...");
            string oldParameters = File.ReadAllText(gitRepoClonePath + "\\parameter.txt");
            string[] lines = oldParameters.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (algorithm != "0")//Implementing parameters if added
            {
                Echo("Implementing parameters...");
                algorithm = algorithm != "0" ? algorithm : lines[0].Split('=')[1];
                iteration = iteration != string.Empty ? iteration : lines[2].Split('=')[1];
                threshold = threshold != string.Empty ? threshold : lines[3].Split('=')[1];
                depth = depth != string.Empty ? depth : lines[4].Split('=')[1];
                num_matrices = num_matrices != string.Empty ? num_matrices : lines[5].Split('=')[1];
                bfi = bfi != string.Empty ? bfi : lines[6].Split('=')[1];
                ChangeParameters();//Change parameter.txt with inserted params
                Echo("Implementing completed");
            }
            Thread.Sleep(100);
            Echo("Building Docker Image...");
            strCmdText = "/C @echo off &  cd " + dockerFilePath + " & docker build . -t " + tag + ":" + name;
            RunCommand();
            Echo("Docker Image Built Succesfully!");
            //Building docker image  
        }
        static void GitClone()//Cloning git repo on Desktop 
        {
            //Cloning git repo on Desktop 
            Echo("Cloning Git Repository...");
            strCmdText = "/C @echo off &  cd " + desktopPath + " & mkdir GlobalOPTFW & cd GlobalOPTFW & git clone " + gitRepoURL;
            string output = RunCommand();
            Echo(output + "Git Repo Cloned!");
            //Cloning git repo on Desktop
        }

        static void ClearAndUpdate()//Clear old builds and update repo if need
        {
            //Updating repo
            Echo("Clearing old builds and updating repository...");
            if (Directory.Exists(gitRepoClonePath))//if repo exists update repo
            {
                strCmdText = "/C @echo off &  cd " + gitRepoClonePath + " & git pull";
                RunCommand();
                Echo("Git Repo Updated To Latest Version!");
            }
            else
            {
                Echo("Git Repo Not Found!", ConsoleColor.Red);
            }
            //Updating repo 
            //Clearing all docker images 
            Echo("Clearing old docker images...");
            strCmdText = "/C for /F %i in ('docker images -a -q') do docker rmi -f %i";
            RunCommand();
            Echo("All Docker Image Cleared!");
            //Clearing all docker images
            Echo("All Cleared");
        }

        static void ChangeParameters()
        {
            //Changing the parameter.txt
            string parameter = "ALGORITHM=" + algorithm + "\r\nMATRIX_PATH=matrix.txt\r\nITERATION=" + iteration + "\r\nTHRESHOLD=" + threshold + "\r\nDEPTH=" + depth + "\r\nNUM_MATRICES=" + num_matrices + "\r\nBFI=" + bfi + "";
            File.WriteAllText(gitRepoClonePath + "\\parameter.txt", parameter);
            //Changing the parameter.txt
        }

        static string RunCommand()//Function that run commands on cmd.exe
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.Arguments = strCmdText;
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(output);
            Thread.Sleep(100);
            return output;
        }

        static void Echo(string text, ConsoleColor color = ConsoleColor.Green)//function that logs with color on terminal 
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text + "\n");
            Thread.Sleep(100);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
