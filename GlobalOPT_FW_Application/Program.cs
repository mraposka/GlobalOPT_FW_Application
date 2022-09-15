using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GlobalOPT_FW_Application
{
    internal class Program
    {
        static string strCmdText;
        static string tag = "fw";
        static string name = "opt";
        static string gitRepoURL = "https://github.com/mraposka/DockerOPT";
        static string gitRepoName = gitRepoURL.Split('/').Last();
        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string textFilePath = desktopPath + "\\result.txt";

        static void Main(string[] args)
        {
            //<summary>
            //---To-do list---
            // 
            //docker and git install with winget or selse function 
            //succesful check
            //version control for clearing
            //  
            //---To-do list---
            //</summary>

            ClearAndUpdate();
            if (!Directory.Exists(desktopPath + "\\GlobalOPTFW\\" + gitRepoName)) GitClone();//if repo does not exists clone it
            CreateDockerImage();
            RunDockerBuild();
            Console.ReadKey();
        }
        static void RunDockerBuild()
        {
            //Runs created docker build
            Echo("Executing...");
            Thread.Sleep(100);
            strCmdText = "/C @echo off &  cd " + desktopPath + "\\GlobalOPTFW\\" + gitRepoName + " & docker run " + tag + ":" + name;
            string output=RunCommand(); 
            Echo("Execution completed!");
            //Runs created docker build
            //Saving results in result.txt file 
            Echo("Saving results...");
            string buildOutput = "------Build Time:" + DateTime.Now + "------\n" + output + "------Build Finished------\n";
            if (!File.Exists(textFilePath))
            { FileStream fs = File.Create(textFilePath); fs.Close(); }//Create result.txt file if does not exist
            File.AppendAllText(textFilePath, buildOutput);
            Echo("Result file succesfully written on result file on Desktop\n\nPress any key to exit");
            //Saving results in result.txt file
        }
        static void CreateDockerImage()
        {
            //Building docker image 
            Echo("Building Docker Image...");
            strCmdText = "/C @echo off &  cd " + desktopPath + "\\GlobalOPTFW\\" + gitRepoName + " & docker build . -t " + tag + ":" + name;
            RunCommand(); 
            Echo("Docker Image Built Succesfully!");
            //Building docker image 
        }
        static void GitClone()
        {
            //Cloning git repo on Desktop 
            Echo("Cloning Git Repository...");
            strCmdText = "/C @echo off &  cd " + desktopPath + " & mkdir GlobalOPTFW & cd GlobalOPTFW & git clone " + gitRepoURL;
            string output = RunCommand();
            Echo(output + "Git Repo Cloned!");
            //Cloning git repo on Desktop
        }

        static void ClearAndUpdate()
        {
            //Updating repo
            Echo("Clearing old builds and updating repository...");
            if (Directory.Exists(desktopPath + "\\GlobalOPTFW\\" + gitRepoName))//if repo exists update repo
            {
                strCmdText = "/C @echo off &  cd " + desktopPath + "\\GlobalOPTFW\\" + gitRepoName + " & git pull";
                RunCommand();
                Echo("Git Repo Updated To Latest Version!");
            }
            else
            {
                Error("Git Repo Not Found!");
            }
            //Updating repo 
            //Clearing all docker images
            strCmdText = "/C @echo off &  for /F %i in ('docker images -a -q') do docker rmi -f %i";
            RunCommand();
            Echo("All Docker Image Cleared!");
            //Clearing all docker images
            Echo("All Cleared");
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
        static void Echo(string text)//function that logs on terminal 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text + "\n");
            Thread.Sleep(100);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Error(string text)//function that logs on terminal 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text + "\n");
            Thread.Sleep(100);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
