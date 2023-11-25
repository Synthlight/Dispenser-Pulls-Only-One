using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace Dispenser_Pulls_Only_One {
    [HarmonyPatch]
    [UsedImplicitly]
    public static class DispenserPatch {
        [HarmonyTargetMethod]
        [UsedImplicitly]
        public static MethodBase TargetMethod() {
            return typeof(Dispenser).GetMethod(nameof(Dispenser.LoadSlot), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static void LoadSlot(ref Dispenser __instance, ref Dispenser.Element slot, ItemDefinition targetItem, ref OnlineCargoConnector m_connector) {
            if (slot.Item.Item != targetItem && !slot.Animation.IsAnimating) {
                if (slot.Item.IsValid) {
                    __instance.MoveTo(ref slot, m_connector.OnlineCargo, Dispenser.State.Unload);
                    return;
                }
                if (targetItem != null) {
                    var propertySet = PropertySetReader.GetInternalSet(slot.Item.Stats) ?? new PropertySet();
                    var num         = m_connector.OnlineCargo.Remove(__instance.gameObject, targetItem, 1, 0, propertySet);
                    if (num > 0) {
                        slot.Item  = new InventoryItem(targetItem, num, propertySet);
                        slot.State = Dispenser.State.Load;
                        slot.Animation.TransitionTo(true);
                        __instance.SetDirtyBit(uint.MaxValue);
                        return;
                    }
                    if (slot.State != Dispenser.State.Missing) {
                        slot.State = Dispenser.State.Missing;
                        __instance.SetDirtyBit(uint.MaxValue);
                    }
                }
            }
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        public static bool Prefix(ref Dispenser __instance, ref Dispenser.Element slot, ref ItemDefinition targetItem, ref OnlineCargoConnector ___m_connector) {
            LoadSlot(ref __instance, ref slot, targetItem, ref ___m_connector);
            return false;
        }
    }
}