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
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using EP.U3D.EDITOR.BASE;
using Preferences = EP.U3D.LIBRARY.BASE.Preferences;

namespace EP.U3D.EDITOR.ARCH
{
    public class BuildWorker
    {
        public string TargetName;
        public string ArchiveName;
        public string ArchiveVer;
        public string ArchiveExt;

        public BuildWorker(string target, string ext = "")
        {
            TargetName = target;
            ArchiveExt = ext;
        }

        public virtual bool BeforeBuild()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Warning", "Please wait till compile done.", "OK");
                return false;
            }
            if (File.Exists(Constants.PREF_STEAMING_FILE) == false)
            {
                EditorUtility.DisplayDialog("Warning", "Finish preferences settings and try again.", "OK");
                EditorApplication.ExecuteMenuItem(Constants.MENU_WIN_PREF);
                return false;
            }
            if (File.Exists(Constants.PLAT_STEAMING_FILE) == false)
            {
                EditorUtility.DisplayDialog("Warning", "Finish platform settings and try again.", "OK");
                EditorApplication.ExecuteMenuItem(Constants.MENU_WIN_PLAT);
                return false;
            }
#if EFRAME_ILR
            if (Preferences.Instance.LiveMode && ValidiateILR(new string[] { TargetName }).Length == 0) return false;
#endif
#if EFRAME_LUA
            if (Preferences.Instance.LiveMode && ValidiateLua(new string[] { TargetName }).Length == 0) return false;
#endif
            if (TargetName == "Windows")
            {
#if !UNITY_STANDALONE_WIN
                if (EditorUtility.DisplayDialog("Hint", $"Click OK to switch to {TargetName} environment.", "OK", "Cancel"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                }
                return false;
#endif
            }
            else if (TargetName == "Android")
            {
#if !UNITY_ANDROID
                if (EditorUtility.DisplayDialog("Hint", $"Click OK to switch to {TargetName} environment.", "OK", "Cancel"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                }
                return false;
#endif
            }
            else if (TargetName == "iOS")
            {
#if !UNITY_IOS
                if (EditorUtility.DisplayDialog("Hint", $"Click OK to switch to {TargetName} environment.", "OK", "Cancel"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                }
                return false;
#endif
            }
            GenArchiveName();
            EditorEvtcat.OnPostBuildPlayerEvent += AfterBuild;
            CopyStreamingBundles();
            return true;
        }

        public virtual void Build() { }

        public virtual void AfterBuild(BuildTarget target, string path)
        {
            EditorEvtcat.OnPostBuildPlayerEvent -= AfterBuild;
            DeleteStreamingBundles();
            Helper.CollectScenes();
            string toast = $"Build {TargetName} archive done.";
            Helper.Log("[FILE@{0}] {1}", GetArchivePath(), toast);
            Helper.ShowToast(toast);
        }

        public virtual string[] GetBuildScenes()
        {
            List<string> names = new List<string>();
            names.Add(Constants.LAUNCHER_SECNE);
            return names.ToArray();
        }

        public virtual void GenArchiveName(bool isdir = false)
        {
            int maxIndex = 1;
            string datetime = DateTime.Now.ToString("yyyyMMdd");
            if (Directory.Exists(Constants.BUILD_ARCHIVE_PATH))
            {
                DirectoryInfo binDirectory = new DirectoryInfo(Constants.BUILD_ARCHIVE_PATH);
                FileSystemInfo[] files;
                if (isdir)
                {
                    files = binDirectory.GetDirectories();
                }
                else
                {
                    files = binDirectory.GetFiles();
                }
                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        var file = files[i];
                        if (file == null) continue;
                        string name = file.Name;
                        if (string.IsNullOrEmpty(name)) continue;
                        if (!string.IsNullOrEmpty(ArchiveExt) && name.EndsWith(ArchiveExt) == false) continue;
                        int buildIndex = name.LastIndexOf("Build");
                        if (buildIndex == -1) continue;
                        name = name.Replace("Build", "");
                        name = name.Substring(buildIndex);
                        if (name.StartsWith(datetime))
                        {
                            name = name.Replace(datetime, "");
                            int index;
                            int.TryParse(name, out index);
                            if (index >= maxIndex)
                            {
                                maxIndex = index + 1;
                            }
                        }
                    }
                }
            }
            ArchiveName = Helper.StringFormat("{0}_{1}_{2}_{3}_Build{4}{5}",
                Constants.PROJ_NAME, Application.version,
                Preferences.Instance.LiveMode ? "Live" : "Test",
                Preferences.Instance.ReleaseMode ? "Release" : "Debug", datetime, maxIndex);
            ArchiveVer = datetime + maxIndex;
        }

        public virtual string GetArchivePath() { return string.Empty; }

        public virtual void CopyStreamingBundles()
        {
            AssetDatabase.Refresh();
            Helper.DeleteDirectory(Constants.STREAMING_ASSET_BUNDLE_PATH);
            Helper.DeleteDirectory(Constants.STREAMING_SCRIPT_BUNDLE_ROOT);
            Helper.CopyDirectory(Constants.BUILD_ASSET_BUNDLE_PATH, Constants.STREAMING_ASSET_BUNDLE_PATH, ".meta", ".manifest", ".DS_Store");
            Helper.CopyDirectory(Constants.BUILD_SCRIPT_BUNDLE_PATH, Constants.STREAMING_SCRIPT_BUNDLE_ROOT, ".meta", ".manifest", ".DS_Store");
            AssetDatabase.Refresh();
        }

        public virtual void DeleteStreamingBundles()
        {
            AssetDatabase.Refresh();
            Helper.DeleteDirectory(Constants.STREAMING_ASSET_BUNDLE_PATH);
            Helper.DeleteDirectory(Constants.STREAMING_SCRIPT_BUNDLE_ROOT);
            AssetDatabase.Refresh();
        }

        public virtual string[] ValidiateILR(string[] inputs)
        {
            // TODO
            List<string> valids = new List<string>();
            valids.AddRange(inputs);
            return valids.ToArray();
        }

        public virtual string[] ValidiateLua(string[] inputs)
        {
            List<string> valids = new List<string>();
            for (int i = 0; i < inputs.Length; i++)
            {
                var plat = inputs[i];
                string core = $"{Constants.BUILD_PATCH_ROOT}{plat}/{Constants.BUILD_LUA_BUNDLE_PATH.Replace(Constants.BUILD_PATCH_PATH, "")}x64/libs{Constants.LUA_BUNDLE_FILE_EXTENSION}";
                if (File.Exists(core))
                {
                    string coretxt = File.ReadAllText(core);
                    if (coretxt.Contains("topameng@gmail.com"))
                    {
                        Helper.LogError("There has unencrypted script in {0}, please switch to LiveMode and recompile script.", plat);
                    }
                    else
                    {
                        valids.Add(plat);
                    }
                }
            }
            return valids.ToArray();
        }
    }
}