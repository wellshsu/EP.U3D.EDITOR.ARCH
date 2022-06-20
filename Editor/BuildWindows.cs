//---------------------------------------------------------------------//
//                    GNU GENERAL PUBLIC LICENSE                       //
//                       Version 2, June 1991                          //
//                                                                     //
// Copyright (C) Wells Hsu, wellshsu@outlook.com, All rights reserved. //
// Everyone is permitted to copy and distribute verbatim copies        //
// of this license document, but changing it is not allowed.           //
//                  SEE LICENSE.md FOR MORE DETAILS.                   //
//---------------------------------------------------------------------//
using System;
using System.IO;
using UnityEditor;
using EP.U3D.EDITOR.BASE;
using Preferences = EP.U3D.LIBRARY.BASE.Preferences;
using System.Threading;

namespace EP.U3D.EDITOR.ARCH
{
    public class BuildWindows : BuildWorker
    {
        public static Type WorkerType = typeof(BuildWindows);

        [MenuItem(Constants.MENU_ARCH_BUILD_WINDOWS)]
        public static void Invoke()
        {
            var worker = Activator.CreateInstance(WorkerType) as BuildWindows;
            if (worker.BeforeBuild()) worker.Build();
        }

        public BuildWindows() : base("Windows") { }

        public override void Build()
        {
            base.Build();
            BuildOptions ops = BuildOptions.None;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, Preferences.Instance.LiveMode ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            BuildPipeline.BuildPlayer(GetBuildScenes(), GetArchivePath(), BuildTarget.StandaloneWindows64, ops);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        }

        public override void GenArchiveName(bool isdir = false) { base.GenArchiveName(true); }

        public override string GetArchivePath()
        {
            return Helper.StringFormat("{0}{1}/{2}.exe", Constants.BUILD_ARCHIVE_PATH, ArchiveName, ArchiveName);
        }

        public override void AfterBuild(BuildTarget target, string path)
        {
            base.AfterBuild(target, path);
            if (GetArchivePath() != Constants.SIMULATOR_EXE)
            {
                if (EditorUtility.DisplayDialog("Hint", "Replace simulator?", "OK", "Cancel"))
                {
                    EditorApplication.ExecuteMenuItem(Constants.MENU_SIMULATOR_STOP);
                    new Thread(() =>
                    {
                        Thread.Sleep(2000);
                        Loom.QueueInMainThread(() =>
                        {
                            string simulatorDir = Path.GetDirectoryName(Constants.SIMULATOR_EXE) + "/";
                            Helper.DeleteDirectory(simulatorDir);
                            Helper.CopyDirectory(Path.GetDirectoryName(GetArchivePath()), simulatorDir);
                            Directory.Move(Path.Combine(simulatorDir, ArchiveName + "_Data"),
                                Path.Combine(simulatorDir, Path.GetFileNameWithoutExtension(Constants.SIMULATOR_EXE) + "_Data"));
                            Directory.Move(Path.Combine(simulatorDir, ArchiveName + ".exe"),
                               Path.Combine(simulatorDir, Path.GetFileNameWithoutExtension(Constants.SIMULATOR_EXE) + ".exe"));
                        });
                    }).Start();
                }
            }
        }
    }
}