using System.Reflection;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Editor;

namespace cc.dingemans.bigibas123.NdmfVrcfReorder
{
	public class VrcfReorderedVrcfRemover
	{
		private static readonly string TAG = "[VrcfReordered]";

		[InitializeOnLoadMethod]
		static void go()
		{
			VRCSdkControlPanel.OnSdkPanelEnable += (sender, e) =>
			{
				FixList();

				if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkBuilderApi>(out var builder))
				{
					builder.OnSdkBuildStart += (sender2, target) => { FixList(); };
				}
			};
		}

		private static List<IVRCSDKPreprocessAvatarCallback> GetAllPreprocessAvatarCallbacks()
		{
			var callbackField = typeof(VRCBuildPipelineCallbacks).GetField("_preprocessAvatarCallbacks",
				BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
				BindingFlags.Static);
			List<IVRCSDKPreprocessAvatarCallback> list =
				(List<IVRCSDKPreprocessAvatarCallback>)callbackField?.GetValue(null);
			return list;
		}

		private static void FixList()
		{
			var cbs = GetAllPreprocessAvatarCallbacks();
			if (cbs is null)
			{
				Debug.LogError(
					$"{TAG} Could not find PreprocessAvatarCallbacks list, this probably means that VRCSDK has updated but this plugin hasn't");
				return;
			}

			bool found = false;
			cbs.RemoveAll(callback =>
			{
				switch (callback.GetType().FullName)
				{
					case "VF.VrcHooks.PreuploadHook":
						Debug.Log($"{TAG} Found VRCFury VrcHooks PreuploadHook");
						found = true;
						return true;
					case "VF.Hooks.VrcPreuploadHook":
						Debug.Log($"{TAG} Found VRCFury Hooks VrcPreuploadHook");
						found = true;
						return true;
					default:
						return false;
				}
			});
			if (!found)
			{
				Debug.LogError(
					"Did not find VRCFury in upload hooks, this will probably mean the build will fail or behave weird");
			}
		}
	}
}
