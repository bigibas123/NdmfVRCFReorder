using System;
using System.Reflection;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using System.Collections.Generic;
using nadena.dev.ndmf;
using tk.dingemans.bigibas123.NdmfVrcfReorder;
using UnityEngine;
using VRC.SDK3A.Editor;
using tk.dingemans.bigibas123.NdmfVrcfReorder.Extensions;
using UnityEngine.SceneManagement;
using VF.Builder;
using VF.Builder.Exceptions;
using VF.Builder;

[assembly: ExportsPlugin(typeof(VrcfReorderedPlugin))]

namespace tk.dingemans.bigibas123.NdmfVrcfReorder
{
    public class VrcfReorderedPlugin : Plugin<VrcfReorderedPlugin>
    {
        public override string QualifiedName => "tk.dingemans.bigibas123.NdmfVrcfReorder.VrcfReorderedPlugin";
        public override string DisplayName => "VRCFury Reordered";

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .Run("VRCFury but at a non-default time", ctx =>
                {
                    if (!Application.isPlaying)
                    {
                        var builder = new VRCFuryBuilder();
                        var method = getVRCFBuilderSafeRunMethod();
                        bool vrcFurySuccess = (bool)method.Invoke(builder, new object[]
                        {
                            (VFGameObject)ctx.AvatarRootObject.asVf(),
                            (VFGameObject) null
                        });
                        if (!vrcFurySuccess)
                        {
                            throw new VRCFBuilderException(
                                "Error building VRCF from Reordered position please check log for details");
                        }
                    }
                    else
                    {
                        var method = getVRCFPlaymodeRescanMethod();
                        this.Print("Running method: " + method);
                        method.Invoke(null, new object[] { ctx.AvatarRootObject.scene, LoadSceneMode.Single });
                    }
                });
        }

        private MethodInfo getVRCFPlaymodeRescanMethod()
        {
            MethodInfo dynMethod = typeof(VF.PlayModeTrigger).GetMethod("OnSceneLoaded",
                BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                BindingFlags.Static);
            return dynMethod;
        }

        private MethodInfo getVRCFBuilderSafeRunMethod()
        {
            MethodInfo dynMethod = typeof(VRCFuryBuilder).GetMethod("SafeRun",
                BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                BindingFlags.Instance);
            return dynMethod;
        }
    }


    public class NdmfVrcfReorderOnSdkAttempt
    {
        [InitializeOnLoadMethod]
        static void go()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += (sender, e) =>
            {
                fixList();
                if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
                {
                    builder.OnSdkBuildStart += (sender2, target) => { fixList(); };
                }
            };
        }

        private static List<IVRCSDKPreprocessAvatarCallback> getAllPreprocessAvatarCallbacks()
        {
            var callbackField = typeof(VRCBuildPipelineCallbacks).GetField("_preprocessAvatarCallbacks",
                BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                BindingFlags.Static);
            List<IVRCSDKPreprocessAvatarCallback> list =
                (List<IVRCSDKPreprocessAvatarCallback>)callbackField.GetValue(null);
            return list;
        }

        public static void fixList()
        {
            var cbs = getAllPreprocessAvatarCallbacks();
            cbs.RemoveAll(callback =>
            {
                switch (callback.GetType().FullName)
                {
                    case "VF.VrcHooks.PreuploadHook":
                        Print("Found VRCFury PreuploadHook");
                        return true;
                    default:
                        return false;
                }
            });
        }

        public static void Print(string arg)
        {
            int lastDot = typeof(NdmfVrcfReorderOnSdkAttempt).Namespace.LastIndexOf(".") + 1;
            string name = typeof(NdmfVrcfReorderOnSdkAttempt).Namespace.Substring(lastDot) + "." +
                          nameof(NdmfVrcfReorderOnSdkAttempt);

            Debug.Log("[" + name + "] " + arg);
        }
    }
}