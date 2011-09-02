﻿using Chocolatey_Explorer.Model;
using Chocolatey_Explorer.Powershell;

namespace Chocolatey_Explorer.Services
{
    public class PackageVersionService
    {
        private readonly IRun _powershellAsync;
        private string _package;
        private PackageVersion _packageVersion;
            
        public delegate void VersionResult(PackageVersion version);
        public event VersionResult VersionChanged;

        public void OnVersionChanged(PackageVersion version)
        {
            var handler = VersionChanged;
            if (handler != null) handler(version);
        }

        public PackageVersionService()
        {
            _powershellAsync = new RunAsync();
            _powershellAsync.OutputChanged += VersionHandler;
            _powershellAsync.RunFinished += RunFinished;
        } 

        public void PackageVersion(string package)
        {
            _packageVersion = new PackageVersion();
            _package = package;
            _powershellAsync.Run("cver " + package + " -source " + Settings.Source);
        }

        private void VersionHandler(string version)
        {
            _packageVersion.Name = _package;
            if (version.StartsWith("found"))
            {
                _packageVersion.CurrentVersion = version.Substring(5).Trim();
            }
            if(version.StartsWith("latest"))
            {
                _packageVersion.Serverversion = version.Substring(6).Trim();
            }
        }

        private void RunFinished()
        {
            OnVersionChanged(_packageVersion);
        }
    }
}