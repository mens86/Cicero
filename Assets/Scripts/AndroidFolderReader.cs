#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using System.IO;

public class AndroidFolderReader : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
    }
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        // Do the preprocessing here
        string[] fileEntries = Directory.GetFiles("Assets/Resources/", "*.csv");
        System.IO.Directory.CreateDirectory("Assets/StreamingAssets/");
        using (StreamWriter sw = new StreamWriter("Assets/StreamingAssets/alphabet.txt", false))
        {

            foreach (string filename in fileEntries)
            {
                sw.WriteLine(Path.GetFileNameWithoutExtension(filename));
            }

        }
    }
}
#endif