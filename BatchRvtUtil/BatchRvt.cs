﻿//
// Revit Batch Processor
//
// Copyright (c) 2017  Daniel Rumery, BVN
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace BatchRvtUtil
{
    public static class BatchRvt
    {
        public const string SCRIPTS_FOLDER_NAME = "Scripts";

        public enum CentralFileOpenOption { Detach = 0, CreateNewLocal = 1 }
        public enum RevitSessionOption { UseSeparateSessionPerFile = 0, UseSameSessionForFilesOfSameVersion = 1}
        public enum RevitProcessingOption { BatchRevitFileProcessing = 0, SingleRevitTaskProcessing = 1 }
        public enum RevitFileProcessingOption { UseFileRevitVersionIfAvailable = 0, UseSpecificRevitVersion = 1 }

        private const string SCRIPT_DATA_FOLDER_NAME = "BatchRvt";

        private static readonly Dictionary<RevitVersion.SupportedRevitVersion, string> BATCHRVT_ADDIN_FILENAMES =
            new Dictionary<RevitVersion.SupportedRevitVersion, string>()
            {
                { RevitVersion.SupportedRevitVersion.Revit2015, "BatchRvtAddin2015.addin" },
                { RevitVersion.SupportedRevitVersion.Revit2016, "BatchRvtAddin2016.addin" },
                { RevitVersion.SupportedRevitVersion.Revit2017, "BatchRvtAddin2017.addin" },
                { RevitVersion.SupportedRevitVersion.Revit2018, "BatchRvtAddin2018.addin" },
            };

        private static string ConstructCommandLineArguments(IEnumerable<KeyValuePair<string, string>> arguments)
        {
            return string.Join(" ", arguments.Select(arg => "--" + arg.Key + " " + arg.Value));
        }

        public static Process StartBatchRvt(
                string settingsFilePath,
                string logFolderPath = null,
                string sessionId = null
            )
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var batchRvtOptions = new Dictionary<string, string>() {
                    { CommandSettings.SETTINGS_FILE_PATH_OPTION, settingsFilePath },
                };

            if (!string.IsNullOrWhiteSpace(logFolderPath))
            {
                batchRvtOptions[CommandSettings.LOG_FOLDER_PATH_OPTION] = logFolderPath;
            }

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                batchRvtOptions[CommandSettings.SESSION_ID_OPTION] = sessionId;
            }

            var psi = new ProcessStartInfo(Path.Combine(baseDirectory, "BatchRvt.exe"));
            
            psi.UseShellExecute = false;
            psi.WorkingDirectory = baseDirectory;
            psi.Arguments = ConstructCommandLineArguments(batchRvtOptions);
            psi.RedirectStandardInput = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            var batchRvtProcess = Process.Start(psi);

            return batchRvtProcess;
        }

        public static string GetDataFolderPath()
        {
            return Path.Combine(PathUtil.GetLocalAppDataFolderPath(), SCRIPT_DATA_FOLDER_NAME);
        }

        public static string GetBatchRvtFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetBatchRvtScriptsFolderPath()
        {
            return Path.Combine(GetBatchRvtFolderPath(), SCRIPTS_FOLDER_NAME);
        }

        public static bool IsBatchRvtAddinInstalled(RevitVersion.SupportedRevitVersion revitVersion)
        {
            bool isAddinInstalled = false;
            
            var revitAddinsBaseFolders = new [] { Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolder.ApplicationData };

            var revitAddinsFolderPaths = revitAddinsBaseFolders.Select(f => RevitVersion.GetRevitAddinsFolderPath(revitVersion, f)).ToList();

            foreach (var revitAddinsFolderPath in revitAddinsFolderPaths)
            {
                var batchRvtAddinFilePath = Path.Combine(revitAddinsFolderPath, BATCHRVT_ADDIN_FILENAMES[revitVersion]);

                if (File.Exists(batchRvtAddinFilePath))
                {
                    isAddinInstalled = true;
                }
            }

            return isAddinInstalled;
        }
    };
}