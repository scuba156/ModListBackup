using System;
using System.IO;

namespace ModListBackup.build.tasks
{
    public class EmptyClass
    {
        int MajorVersion;
        int MinorVersion;
        int BuildVersion;
        int RevisionVersion;

        string AssemblyInfoFile;

        public EmptyClass()
        {
            var lines = File.ReadAllLines(AssemblyInfoFile);

            foreach (var line in lines)
            {
                if (line.Contains("assembly: AssemblyFileVersion("))
                {
                    int start = line.IndexOf("\"");
                    int end = line.LastIndexOf("\"");
                    //Log.LogMessage(MessageImportance.High, string.Format("text: '{0}'", line.Substring(start, end)));
                    var values = line.Substring(26, line.Length - 6).Split('.');
                    Version v = new Version(line.Substring(26, line.Length - 6));
                    int.TryParse(values[0], out MajorVersion);
                    int.TryParse(values[1], out MinorVersion);
                    int.TryParse(values[2], out BuildVersion);
                    int.TryParse(values[3], out RevisionVersion);
                }
            }
        }
    }
}
