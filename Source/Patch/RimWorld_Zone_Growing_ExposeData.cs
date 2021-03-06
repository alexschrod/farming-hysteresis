using HarmonyLib;
using RimWorld;

namespace FarmingHysteresis.Patch
{
	[HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.ExposeData))]
	internal static class RimWorld_Zone_Growing_ExposeData
	{
		private static void Postfix(ref Zone_Growing __instance)
		{
			__instance.GetFarmingHysteresisData().ExposeData();
		}
	}
}