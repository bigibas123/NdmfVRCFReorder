using System;
using System.Reflection;
using System.Linq;
using System.Text;
using nadena.dev.ndmf;
using cc.dingemans.bigibas123.NdmfVrcfReorder;
using UnityEngine;
using Debug = UnityEngine.Debug;

[assembly: ExportsPlugin(typeof(VrcfReorderedPlugin))]

namespace cc.dingemans.bigibas123.NdmfVrcfReorder
{
	public class VrcfReorderedPlugin : Plugin<VrcfReorderedPlugin>
	{
		public override string QualifiedName => "cc.dingemans.bigibas123.NdmfVrcfReorder.VrcfReorderedPlugin";
		public override string DisplayName => "VRCFury Reordered";

		private static readonly string TAG = "[VrcfReordered]";

		private static readonly object VrcFuryBuildSuccesEnum =
			Enum.ToObject(Type.GetType("VF.Builder.VRCFuryBuilder+Status, VRCFury-Editor", true), 0);

		protected override void Configure()
		{
			InPhase(BuildPhase.Optimizing)
				.AfterPlugin("com.anatawa12.avatar-optimizer")
				.AfterPlugin("nadena.dev.modular-avatar")
				.Run("VRCFury but at a non-default time", ctx =>
				{
					if (InsideVRCFuryCall())
					{
						Debug.LogWarning($"{TAG} Currently inside of vrcfury call, not re-running vrcfury");
						return;
					}

					var builder = Activator.CreateInstance(GetVRCFBuilderType());
					MethodInfo method = GetVrcfBuilderSafeRunMethod();
					var avatarRootAsVf = GetAsVFObject(ctx.AvatarRootObject);


					Debug.Log($"{TAG} Running upload method: {method}");
					object vrcFuryStatus = method.Invoke(builder, new object[]
					{
						avatarRootAsVf
					});
					if (!VrcFuryBuildSuccesEnum.Equals(vrcFuryStatus))
					{
						throw new Exception(
							"Error building VRCF from Reordered position please check log for details, return code: " +
							vrcFuryStatus + ", wanted: " + VrcFuryBuildSuccesEnum);
					}
				});
		}

		private bool InsideVRCFuryCall()
		{
			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
			var frames = stackTrace.GetFrames();
			if (frames == null)
			{
				return false;
			}

			StringBuilder builder = new StringBuilder();
			foreach (var frame in frames)
			{
				builder.Append($"Frame: {frame.GetMethod().DeclaringType?.FullName} {frame.GetMethod()}");
			}

			Debug.Log($"{TAG},{builder}");

			return frames
				.Select(frame => { return frame.GetMethod().DeclaringType; })
				.Any(type => type?.FullName is "VF.PlayModeTrigger" or "VF.Builder.VRCFuryBuilder");
		}

		private MethodInfo GetVrcfPlaymodeRescanMethod()
		{
			MethodInfo dynMethod = Type.GetType("VF.PlayModeTrigger, ", true).GetMethod("OnSceneLoaded",
				BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
				BindingFlags.Static);
			return dynMethod;
		}

		private Type GetVRCFBuilderType()
		{
			Type t = Type.GetType("VF.Builder.VRCFuryBuilder, VRCFury-Editor", true);
			return t;
		}

		private MethodInfo GetVrcfBuilderSafeRunMethod()
		{
			MethodInfo dynMethod = GetVRCFBuilderType().GetMethod("SafeRun",
				BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
				BindingFlags.Instance);
			return dynMethod;
		}

		private Type GetVFGameObjectType()
		{
			Type t = Type.GetType("VF.Builder.VFGameObject, VRCFury-Editor", true);
			return t;
		}

		private object GetAsVFObject(GameObject avatarRoot)
		{
			return Activator.CreateInstance(
				GetVFGameObjectType(),
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
				null,
				new object[] { avatarRoot },
				null,
				null
			);
		}
	}
}