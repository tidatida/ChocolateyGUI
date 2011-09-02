using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Chocolatey_Explorer.Model;
using Chocolatey_Explorer.Powershell;

namespace Chocolatey_Explorer.Services
{
    public class PackagesService
    {
        private IRun _powershellAsync;
        private IList<string> lines;
 
        public delegate void FinishedDelegate(IList<Package> packages);
        public event FinishedDelegate RunFinshed;

        public void OnRunFinshed(IList<Package> packages)
        {
            FinishedDelegate handler = RunFinshed;
            if (handler != null) handler(packages);
        }

        public PackagesService()
        {
            lines = new List<string>();
            _powershellAsync = new RunAsync();
            _powershellAsync.OutputChanged += OutputChanged;
            _powershellAsync.RunFinished += RunFinished;
        }

        public PackagesService(IRun powershell)
        {
            _powershellAsync = powershell;
        }

        public void ListOfPackages()
        {
            _powershellAsync.Run("clist -source " + Settings.Source);
        }

        public void ListOfInstalledPackages()
        {
            var thread = new Thread(ListOfInstalledPackagsThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private  void ListOfInstalledPackagsThread()
        {
            var folders = System.IO.Directory.GetDirectories("c:/nuget/lib");
            IList<Package> packages = new List<Package>();
            foreach (var folder in folders)
            {
                var folder2 = folder.Split("\\".ToCharArray())[1];
                var name = folder2.Substring(0, folder2.IndexOf("."));
                packages.Add(new Package() { Name = name });
            }
            OnRunFinshed(packages);
        }

        private void OutputChanged(string line)
        {
            lines.Add(line);
        }

        private void RunFinished()
        {
            OnRunFinshed((from result in lines
                    let name = result.Split(" ".ToCharArray()[0])[0]
                    let version = result.Split(" ".ToCharArray()[0])[1]
                    select new Package() { Name = name }).ToList());
        }
        
    }
}