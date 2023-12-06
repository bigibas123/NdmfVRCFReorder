using System;
using System.Reflection;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using tk.dingemans.bigibas123.NdmfVrcfReorder;
using UnityEngine;
using VRC.SDK3A.Editor;
using tk.dingemans.bigibas123.NdmfVrcfReorder.Extensions;
using UnityEngine.SceneManagement;
using VF.Builder.Exceptions;

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
                    ctx.AvatarRootObject.name += "(Clone)";
                    bool result = Fixer.Instance.callVRCF(ctx.AvatarRootObject);
                    if (!result)
                    {
                        throw new VRCFBuilderException(
                            "Error building VRCF from Reordered position please check log for details");
                    }
                });
        }
    }

    // public class BeforeAllHook : IVRCSDKPreprocessAvatarCallback
    // {
    //     //  Hopefully before everything keeping the list stable as it progresses
    //     public int CallbackOrder = int.MinValue;
    //     public int callbackOrder => CallbackOrder;
    //
    //     public bool OnPreprocessAvatar(GameObject avatarGameObject)
    //     {
    //         this.Print("OnPreprocessAvatar before\n" + Fixer.Instance.GetAllContentsAsString());
    //         Fixer.Instance.fixList();
    //         this.Print("OnPreprocessAvatar after\n" + Fixer.Instance.GetAllContentsAsString());
    //         return true;
    //     }
    // }
    //
    // public class NormalHook : IVRCSDKPreprocessAvatarCallback
    // {
    //     // Before RemoveAvatarEditorOnly and other VRCSDK callbacks after most stuff has happened
    //     // but in roughly the same time as the original ndmf hook
    //     public int CallbackOrder = -1040;
    //     public int callbackOrder => CallbackOrder;
    //
    //     public bool OnPreprocessAvatar(GameObject avatarGameObject)
    //     {
    //         this.Print("OnPreprocessAvatar before\n" + Fixer.Instance.GetAllContentsAsString());
    //         Fixer.Instance.fixList();
    //         this.Print("OnPreprocessAvatar after\n" + Fixer.Instance.GetAllContentsAsString());
    //         //TODO return Fixer.Instance.callVRCF(avatarGameObject);
    //         return true;
    //     }
    // }

    public class NdmfVrcfReorderOnSdkAttempt
    {
        [InitializeOnLoadMethod]
        static void go()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += (sender, e) =>
            {
                Fixer.Instance.fixList();
                if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
                {
                    builder.OnSdkBuildStart += (sender2, target) => { Fixer.Instance.fixList(); };
                }
            };
        }

        public static void Print(string arg)
        {
            int lastDot = typeof(NdmfVrcfReorderOnSdkAttempt).Namespace.LastIndexOf(".", StringComparison.Ordinal);
            string name = typeof(NdmfVrcfReorderOnSdkAttempt).Namespace.Substring(lastDot) + "." +
                          nameof(NdmfVrcfReorderOnSdkAttempt);

            Debug.Log("[" + name + "] " + arg);
        }
    }

    public sealed class Fixer
    {
        private List<IVRCSDKPreprocessAvatarCallback> _vrcfHooks = new List<IVRCSDKPreprocessAvatarCallback>();

        public void fixList()
        {
            var cbs = getAllPreprocessAvatarCallbacks();
            cbs.RemoveAll(callback =>
            {
                switch (callback.GetType().FullName)
                {
                    case "VF.VrcHooks.PreuploadHook":
                        _vrcfHooks.Add(callback);
                        this.Print("Found VRCFury PreuploadHook");
                        return true;
                    default:
                        return false;
                }
            });
        }

        public bool callVRCF(GameObject avatarGameObject)
        {
            if (!Application.isPlaying)
            {
                this.Print("Running vrcf Callbacks");
                foreach (var cb in _vrcfHooks.OrderBy(callback => callback.callbackOrder))
                {
                    if (!cb.OnPreprocessAvatar(avatarGameObject))
                    {
                        string message =
                            "The VRCSDK build was aborted in the NdmfVrcReorder because the VRCSDKPreprocessAvatarCallback '" +
                            cb.GetType().Name + "' reported a failure.";
                        Debug.LogError((object)message);
                        EditorUtility.DisplayDialog("Preprocess Callback Failed", message, "Ok");
                        return false;
                    }
                    else
                    {
                        this.Print("Step: " + cb.GetType().FullName + " executed successfuly");
                    }
                }
            }
            else
            {
                var method = getVRCFPlaymodeRescanMethod();
                this.Print("Running method: "+method);
                method.Invoke(null, new object[] { avatarGameObject.scene, LoadSceneMode.Single });
            }

            return true;
        }

        private List<IVRCSDKPreprocessAvatarCallback> getAllPreprocessAvatarCallbacks()
        {
            var callbackField = typeof(VRCBuildPipelineCallbacks).GetField("_preprocessAvatarCallbacks",
                BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                BindingFlags.Static);
            List<IVRCSDKPreprocessAvatarCallback> list =
                (List<IVRCSDKPreprocessAvatarCallback>)callbackField.GetValue(null);
            return list;
        }

        private MethodInfo getVRCFPlaymodeRescanMethod()
        {
            MethodInfo dynMethod = typeof(VF.PlayModeTrigger).GetMethod("OnSceneLoaded",
                BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                BindingFlags.Static);
            return dynMethod;
        }

        private Fixer()
        {
        }

        public static Fixer Instance
        {
            get { return Nested.instance; }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly Fixer instance = new Fixer();
        }
    }
}