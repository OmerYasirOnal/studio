#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BraveBunny.Editor
{
    public static class GameViewScreenshot
    {
        [MenuItem("BraveBunny/Capture Game View")]
        public static void Capture()
        {
            string path = "/tmp/bb-game-view.png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[Screenshot] requested at {path}");
            // Wait a frame for ScreenCapture to flush
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(path))
                    Debug.Log($"[Screenshot] saved {new FileInfo(path).Length / 1024} KB to {path}");
                else
                    Debug.LogError($"[Screenshot] file not found at {path}");
            };
        }
    }
}
#endif
