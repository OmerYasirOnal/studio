#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Brave.Editor
{
    public static class iOSExportComplianceProcessor
    {
        // ITSAppUsesNonExemptEncryption = false makes App Store Connect skip the
        // export-compliance dialog on every TestFlight upload. Brave Bunny ships
        // only Apple's standard HTTPS (UnityWebRequest under the hood) — exempt
        // per Apple's export rules — so this Boolean is correct and permanent.
        // See https://developer.apple.com/documentation/security/declaring_your_app_s_use_of_encryption
        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            plist.WriteToFile(plistPath);
        }
    }
}
#endif
