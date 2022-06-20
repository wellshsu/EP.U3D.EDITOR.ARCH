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
using UnityEditor;
using EP.U3D.EDITOR.BASE;
using Preferences = EP.U3D.LIBRARY.BASE.Preferences;

namespace EP.U3D.EDITOR.ARCH
{
    public class BuildAndroid : BuildWorker
    {
        public static Type WorkerType = typeof(BuildAndroid);
        public static string KeyStoreName;
        public static string KeyStorePass;
        public static string KeyaliasName;
        public static string KeyaliasPass;
        public static AndroidArchitecture TargetArchitectures = AndroidArchitecture.ARMv7;

        [MenuItem(Constants.MENU_ARCH_BUILD_ANDROID)]
        public static void Invoke()
        {
            var worker = Activator.CreateInstance(WorkerType) as BuildAndroid;
            if (worker.BeforeBuild()) worker.Build();
        }

        public BuildAndroid() : base("Android", ".apk") { }

        public override bool BeforeBuild()
        {
            if (!base.BeforeBuild()) return false;
            if (!string.IsNullOrEmpty(KeyStoreName)) PlayerSettings.Android.keystoreName = KeyStoreName;
            if (!string.IsNullOrEmpty(KeyStorePass)) PlayerSettings.Android.keystorePass = KeyStorePass;
            if (!string.IsNullOrEmpty(KeyaliasName)) PlayerSettings.Android.keyaliasName = KeyaliasName;
            if (!string.IsNullOrEmpty(KeyaliasPass)) PlayerSettings.Android.keyaliasPass = KeyaliasPass;
            PlayerSettings.Android.targetArchitectures = TargetArchitectures;
            return true;
        }

        public override void Build()
        {
            base.Build();
            BuildOptions ops = BuildOptions.None;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, Preferences.Instance.LiveMode ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            BuildPipeline.BuildPlayer(GetBuildScenes(), GetArchivePath(), BuildTarget.Android, ops);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        }

        public override void GenArchiveName(bool isdir = false) { base.GenArchiveName(true); }

        public override string GetArchivePath()
        {
            return Helper.StringFormat("{0}{1}.apk", Constants.BUILD_ARCHIVE_PATH, ArchiveName);
        }
    }
}