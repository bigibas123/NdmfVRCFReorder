using System;
using System.Reflection;
using nadena.dev.ndmf;
using tk.dingemans.bigibas123.NdmfVrcfReorder;
using UnityEngine;
using UnityEngine.SceneManagement;
using VF.Builder;
using VF.Builder.Exceptions;

[assembly: ExportsPlugin(typeof(VrcfReorderedPlugin))]
namespace tk.dingemans.bigibas123.NdmfVrcfReorder
{
	public class VrcfReorderedPlugin : Plugin<VrcfReorderedPlugin>
	{
		public override string QualifiedName => "tk.dingemans.bigibas123.NdmfVrcfReorder.VrcfReorderedPlugin";
		public override string DisplayName => "VRCFury Reordered";

		private static readonly string TAG = "[VrcfReordered]";

		private static readonly object vrcFuryBuildSuccesEnum =
			Enum.ToObject(typeof(VRCFuryBuilder).Assembly.GetType("VF.Builder.VRCFuryBuilder+Status"), 0);

		protected override void Configure()
		{
			InPhase(BuildPhase.Optimizing)
				.AfterPlugin("com.anatawa12.avatar-optimizer")
				.AfterPlugin("nadena.dev.modular-avatar")
				.Run("VRCFury but at a non-default time", ctx =>
				{
					if (!Application.isPlaying)
					{
						VRCFuryBuilder builder = new VRCFuryBuilder();
						MethodInfo method = GetVrcfBuilderSafeRunMethod();
						Debug.Log($"{TAG} Running upload method: {method}");
						object vrcFuryStatus = method.Invoke(builder, new object[]
						{
							ctx.AvatarRootObject.asVf(),
							null,
							false,
						});
						if (!vrcFuryBuildSuccesEnum.Equals(vrcFuryStatus))
						{
							throw new VRCFBuilderException(
								"Error building VRCF from Reordered position please check log for details, return code: "+vrcFuryStatus + ", wanted: "+vrcFuryBuildSuccesEnum);
						}
					}
					else
					{
						var method = GetVrcfPlaymodeRescanMethod();
						Debug.Log($"{TAG} Running play mode method: {method}");
						method.Invoke(null, new object[] { ctx.AvatarRootObject.scene, LoadSceneMode.Single });
					}
				});
		}

		private MethodInfo GetVrcfPlaymodeRescanMethod()
		{
			MethodInfo dynMethod = typeof(VF.PlayModeTrigger).GetMethod("OnSceneLoaded",
				BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
				BindingFlags.Static);
			return dynMethod;
		}

		private MethodInfo GetVrcfBuilderSafeRunMethod()
		{
			MethodInfo dynMethod = typeof(VRCFuryBuilder).GetMethod("SafeRun",
				BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | BindingFlags.NonPublic |
				BindingFlags.Instance);
			return dynMethod;
		}
	}
}
