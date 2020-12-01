using KEUtils.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WinConsole.Misc {
    class FindAndroidVersions : Runnable {
        private static readonly string PROJECTS_DIR = @"C:\AndroidStudioProjects\";
        private static readonly string CSV_PATH = @"C:\Users\evans\Documents\Android Studio";
        public static readonly string TIME_FORMAT = "yyyy'-'MM'-'dd";
        private static List<Versions> versionsList = new List<Versions>();

        private static void Search(string dir) {
            //Console.WriteLine("Processing directory " + dir);
            if (!Directory.Exists(dir)) return;
            // Check for project directory
            bool projectDir = false;
            // Look for .gradle
            foreach (String subdir in Directory.GetDirectories(dir)) {
                if (Path.GetFileName(subdir).ToLower().Equals(".gradle")) {
                    projectDir = true;
                    break;
                }
            }
            if (!projectDir) {
                // Check for settings files
                foreach (String file in Directory.GetFiles(dir)) {
                    if (Path.GetFileName(file).ToLower().Equals("settings.gradle")) {
                        projectDir = true;
                        break;
                    }
                }
            }
            // If not a project directory look for build.gradle
            if (!projectDir) {
                foreach (String file in Directory.GetFiles(dir)) {
                    if (Path.GetFileName(file).Equals("build.gradle")) {
                        Console.WriteLine("Processing file " + file);
                        parseFile(file);
                        return;
                    }
                }
            }
            // Search the sub directories
            foreach (String subdir in Directory.GetDirectories(dir)) {
                Search(subdir);
            }
        }

        private static void parseFile(string fileName) {
            string compileSdkVersion = "<Not found>";
            string applicationId = "<Not found>";
            string minSdkVersion = "<Not found>";
            string targetSdkVersion = "<Not found>";
            string versionCode = "<Not found>";
            string versionName = "<Not found>";
            string[] tokens;
            string appDir;
            string projDir;
            try {
                appDir = Path.GetDirectoryName(fileName);
                projDir = Path.GetDirectoryName(appDir);
                int index = projDir.IndexOf(PROJECTS_DIR);
                if (index > -1) {
                    projDir = projDir.Substring(PROJECTS_DIR.Length);
                }
                foreach (string line in File.ReadAllLines(fileName)) {
                    tokens = Regex.Split(line.Trim(), @"\s+");
                    if (tokens.Length != 2) continue;
                    int end = tokens.Length - 1;
                    if (line.Contains("compileSdkVersion")) {
                        if (tokens[0].Equals("compileSdkVersion")) {
                            compileSdkVersion = tokens[end];
                        }
                    } else if (line.Contains("applicationId")) {
                        if (tokens[0].Equals("applicationId")) {
                            applicationId = tokens[end];
                        }
                    } else if (line.Contains("minSdkVersion")) {
                        if (tokens[0].Equals("minSdkVersion")) {
                            minSdkVersion = tokens[end];
                        }
                    } else if (line.Contains("targetSdkVersion")) {
                        if (tokens[0].Equals("targetSdkVersion")) {
                            targetSdkVersion = tokens[end];
                        }
                    } else if (line.Contains("versionCode")) {
                        if (tokens[0].Equals("versionCode")) {
                            versionCode = tokens[end];
                        }
                    } else if (line.Contains("versionName")) {
                        if (tokens[0].Equals("versionName")) {
                            versionName = tokens[end];
                        }
                    }
                }
                Versions versions = new Versions(projDir, compileSdkVersion, applicationId,
                    minSdkVersion, targetSdkVersion, versionCode, versionName);
                versionsList.Add(versions);
            } catch (Exception ex) {
                Utils.excMsg("Failed to read " + fileName, ex);
            }
        }

        private static string MakeCsvFile() {
            DateTime now = DateTime.Now;
            string fileName = Path.Combine(CSV_PATH, "Android Versions "
                + now.ToString(TIME_FORMAT) + ".csv");
            try {
                using (StreamWriter outputFile = File.CreateText(fileName)) {
                    outputFile.WriteLine("Android project versions as of date: "
                        + now.ToString(TIME_FORMAT));
                    outputFile.WriteLine(
                      "Project,"
                      + "ApplicationId,"
                      + "CompileSdkVersion,"
                      + "MinSdkVersion,"
                      + "TargetSdkVersion,"
                      + "VersionName,"
                      + "VersionCode");
                    foreach (Versions versions in versionsList) {
                        outputFile.WriteLine(
                            versions.FileName + ","
                            + versions.ApplicationId + ","
                            + versions.CompileSdkVersion + ","
                            + versions.MinSdkVersion + ","
                            + versions.TargetSdkVersion + ","
                            + versions.VersionName + ","
                            + versions.VersionCode);
                    }
                }
            } catch (Exception ex) {
                Utils.excMsg("Error writing" + fileName, ex);
                return null;
            }
            return fileName;
        }

        public override void Main(string[] args) {
            Console.WriteLine("FindAndroidVersions: Run at " + DateTime.Now);
            Search(PROJECTS_DIR);
            if (versionsList.Count == 0) {
                Console.WriteLine("No versions found");
                return;
            }
            string fileName = MakeCsvFile();
            if (fileName == null) {
                Console.WriteLine("Failed to write CSV file");
            } else {
                Console.WriteLine("Wrote " + fileName);
            }
        }
    }

    public class Versions {
        public string FileName;
        public string CompileSdkVersion;
        public string ApplicationId;
        public string MinSdkVersion;
        public string TargetSdkVersion;
        public string VersionCode;
        public string VersionName;

        public Versions(string fileName, string compile, string id, string min,
            string target, string code, string name) {
            this.FileName = fileName;
            this.CompileSdkVersion = compile;
            this.ApplicationId = id;
            this.MinSdkVersion = min;
            this.TargetSdkVersion = target;
            this.VersionCode = code;
            this.VersionName = name;
        }
    }
}
