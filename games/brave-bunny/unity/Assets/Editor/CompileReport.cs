#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public static class CompileReport
{
    public static void Run()
    {
        var path = Environment.GetEnvironmentVariable("BRAVE_COMPILE_REPORT") ?? "/tmp/brave-compile.log";
        var sw = new StringWriter();
        CompilationPipeline.assemblyCompilationFinished += (asmPath, msgs) =>
        {
            foreach (var m in msgs)
            {
                if (m.type == CompilerMessageType.Error || m.type == CompilerMessageType.Warning)
                {
                    sw.WriteLine($"[{m.type}] {m.file}({m.line},{m.column}): {m.message} in {asmPath}");
                }
            }
        };
        CompilationPipeline.RequestScriptCompilation();
        // Block until compile completes
        while (EditorApplication.isCompiling) System.Threading.Thread.Sleep(100);
        File.WriteAllText(path, sw.ToString());
        Debug.Log($"[CompileReport] wrote {path}");
        EditorApplication.Exit(0);
    }
}
#endif
