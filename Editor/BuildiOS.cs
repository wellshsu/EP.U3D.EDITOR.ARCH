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
    public class BuildiOS : BuildWorker
    {
        public static Type WorkerType = typeof(BuildiOS);

        [MenuItem(Constants.MENU_ARCH_BUILD_IOS)]
        public static void Invoke()
        {
            var worker = Activator.CreateInstance(WorkerType) as BuildiOS;
            if (worker.BeforeBuild()) worker.Build();
        }

        public BuildiOS() : base("iOS", ".ios") { }

        public override bool BeforeBuild()
        {
            if (!base.BeforeBuild()) return false;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            return true;
        }

        public override void Build()
        {
            base.Build();
            BuildOptions ops = BuildOptions.None;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, Preferences.Instance.LiveMode ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            BuildPipeline.BuildPlayer(GetBuildScenes(), GetArchivePath(), BuildTarget.iOS, ops);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        }

        public override void GenArchiveName(bool isdir = false) { base.GenArchiveName(true); }

        public override string GetArchivePath()
        {
            return Helper.StringFormat("{0}{1}.ios", Constants.BUILD_ARCHIVE_PATH, ArchiveName);
        }

        public override void AfterBuild(BuildTarget target, string path)
        {
            base.AfterBuild(target, path);
        }
    }
}