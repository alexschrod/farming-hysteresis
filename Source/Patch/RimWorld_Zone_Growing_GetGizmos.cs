using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace FarmingHysteresis.Patch
{
	[HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.GetGizmos))]
	internal static class RimWorld_Zone_Growing_GetGizmos
	{
		private static void Postfix(Zone_Growing __instance, ref IEnumerable<Gizmo> __result)
		{
			if (Find.Selector.NumSelected != 1)
			{
				return;
			}

			var data = __instance.GetFarmingHysteresisData();
			var harvestedThingDef = __instance.GetPlantDefToGrow().plant.harvestedThingDef;
			var harvestHysteresisCommand = new Command_Toggle
			{
				defaultLabel = "FarmingHysteresis.EnableFarmingHysteresis".Translate(),
				defaultDesc = "FarmingHysteresis.EnableFarmingHysteresisisDesc".Translate(),
				icon = TexCommand.ForbidOff,
				isActive = () => data.Enabled,
				toggleAction = () =>
				{
					if (data.Enabled)
					{
						data.Disable(__instance);
					}
					else
					{
						data.Enable(__instance);
					}
				}
			};

			var result = new List<Gizmo>(__result);
			if (harvestedThingDef != null)
			{
				result.Add(harvestHysteresisCommand);
			}

			if (data.Enabled)
			{
				if (harvestedThingDef == null)
				{
					data.DisableDueToMissingHarvestedThingDef(__instance);
					return;
				}

				// If hysteresis is enabled, disable the manual sowing enabled button
				var sowingGizmo = result.Find(g => g is Command_Toggle t && t.defaultLabel == "CommandAllowSow".Translate());
				result.Remove(sowingGizmo);

				var useGlobalValuesCommand = new Command_Toggle
				{
					defaultLabel = "FarmingHysteresis.UseGlobalValues".Translate(),
					defaultDesc = "FarmingHysteresis.UseGlobalValuesDesc".Translate(),
					icon = TexCommand.ForbidOff,
					isActive = () => data.useGlobalValues,
					toggleAction = () =>
					{
						if (data.useGlobalValues || FarmingHysteresisMapComponent.For(Find.CurrentMap).HasBoundsFor(harvestedThingDef))
						{
							// We were already using global values OR such global values already exist for this harvest type.
							// So just flip the value.
							data.useGlobalValues = !data.useGlobalValues;
						}
						else
						{
							// This is the first time this harvest type is switching to global values.
							// Copy the initial global values over from the local values for a better user experience.
							var currentLowerBound = data.LowerBound;
							var currentUpperBound = data.UpperBound;

							data.useGlobalValues = true;

							data.LowerBound = currentLowerBound;
							data.UpperBound = currentUpperBound;
						}
					}
				};
				result.Add(useGlobalValuesCommand);

				Texture2D uiIcon = harvestedThingDef.uiIcon;
				var decrementLowerHysteresisCommand = new Command_Decrement
				{
					defaultLabel = "FarmingHysteresis.DecrementLowerHysteresis".Translate(GenUI.CurrentAdjustmentMultiplier()),
					defaultDesc = "FarmingHysteresis.DecrementLowerHysteresisDesc".Translate(
						GenUI.CurrentAdjustmentMultiplier(),
						KeyBindingDefOf.ModifierIncrement_10x.MainKeyLabel,
						KeyBindingDefOf.ModifierIncrement_100x.MainKeyLabel
					),
					icon = uiIcon,
					action = () => data.LowerBound -= GenUI.CurrentAdjustmentMultiplier()
				};
				result.Add(decrementLowerHysteresisCommand);

				var incrementLowerHysteresisCommand = new Command_Increment
				{
					defaultLabel = "FarmingHysteresis.IncrementLowerHysteresis".Translate(GenUI.CurrentAdjustmentMultiplier()),
					defaultDesc = "FarmingHysteresis.IncrementLowerHysteresisDesc".Translate(
						GenUI.CurrentAdjustmentMultiplier(),
						KeyBindingDefOf.ModifierIncrement_10x.MainKeyLabel,
						KeyBindingDefOf.ModifierIncrement_100x.MainKeyLabel
					),
					icon = uiIcon,
					action = () => data.LowerBound += GenUI.CurrentAdjustmentMultiplier()
				};
				result.Add(incrementLowerHysteresisCommand);

				var decrementUpperHysteresisCommand = new Command_Decrement
				{
					defaultLabel = "FarmingHysteresis.DecrementUpperHysteresis".Translate(GenUI.CurrentAdjustmentMultiplier()),
					defaultDesc = "FarmingHysteresis.DecrementUpperHysteresisDesc".Translate(
						GenUI.CurrentAdjustmentMultiplier(),
						KeyBindingDefOf.ModifierIncrement_10x.MainKeyLabel,
						KeyBindingDefOf.ModifierIncrement_100x.MainKeyLabel
					),
					icon = uiIcon,
					action = () => data.UpperBound -= GenUI.CurrentAdjustmentMultiplier()
				};
				result.Add(decrementUpperHysteresisCommand);

				var incrementUpperHysteresisCommand = new Command_Increment
				{
					defaultLabel = "FarmingHysteresis.IncrementUpperHysteresis".Translate(GenUI.CurrentAdjustmentMultiplier()),
					defaultDesc = "FarmingHysteresis.IncrementUpperHysteresisDesc".Translate(
						GenUI.CurrentAdjustmentMultiplier(),
						KeyBindingDefOf.ModifierIncrement_10x.MainKeyLabel,
						KeyBindingDefOf.ModifierIncrement_100x.MainKeyLabel
					),
					icon = uiIcon,
					action = () => data.UpperBound += GenUI.CurrentAdjustmentMultiplier()
				};
				result.Add(incrementUpperHysteresisCommand);
			}
			__result = result;
		}
	}
}