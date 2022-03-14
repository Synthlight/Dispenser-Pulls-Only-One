using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace Dispenser_Pulls_Only_One {
    [HarmonyPatch]
    [UsedImplicitly]
    public static class DispenserPatch {
        private static readonly MethodInfo DISPENSER_MOVE_TO = typeof(Dispenser).GetMethod("MoveTo", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTargetMethod]
        [UsedImplicitly]
        public static MethodBase TargetMethod() {
            return typeof(Dispenser).GetMethod("LoadSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static void LoadSlot(ref Dispenser __instance, ref Dispenser.Element slot, ItemDefinition targetItem, ref OnlineCargoConnector m_connector) {
            if (slot.Item.Item != targetItem && !slot.Animation.IsAnimating) {
                if (slot.Item.IsValid) {
                    var args = new object[] {slot, m_connector.OnlineCargo, Dispenser.State.Unload, 0};
                    DISPENSER_MOVE_TO.Invoke(__instance, args);
                    slot = (Dispenser.Element) args[0];
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