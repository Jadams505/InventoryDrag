using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag
{
    public class InventoryPlayer : ModPlayer
    {
        internal int contextCache = -1;
        internal int slotCache = -1;
        internal bool dragging = false;
        internal int itemCache = ItemID.None;
        public bool HoverSlot2(Item[] inventory, int context, int slot)
        {
            // journey mode slots are always context 29 and slot 0 so their only difference is the item 
            bool journeyModeSlotChange = itemCache != inventory[slot].type && context == ItemSlot.Context.CreativeInfinite;

            bool slotChanged = contextCache != context || slotCache != slot || journeyModeSlotChange;
            contextCache = context;
            slotCache = slot;
            itemCache = inventory[slot].type;

            // the vanilla click methods should handle which special keys are pressed so this can just be true
            bool holdingSpecialKey = ItemSlot.ControlInUse || ItemSlot.ShiftInUse || true;

            // you can right click an empty vanity slot to switch with main equip
            // as far as I know this might be the only case for a empty slot to be draggable
            // TODO: validate this claim and only allow for right click
            bool allowEmptySlots = context == ItemSlot.Context.EquipArmorVanity;
            if (inventory[slot].IsAir && !allowEmptySlots) return false;

            // this seems dumb. A better way must exist
            if (!((Main.mouseLeft || Main.mouseRight) && holdingSpecialKey))
            {
                dragging = false;
            }

            if (Main.mouseLeft && holdingSpecialKey && (!dragging || slotChanged))
            {
                dragging = true;
                Main.NewText($"context: {context}, slot: {slot}");
                bool leftClick = Main.mouseLeftRelease && Main.mouseLeft;
                bool mlr = Main.mouseLeftRelease;

                // skip when the mouse was just pressed down since vanilla can handle it as a click
                if (mlr) return false;

                Main.NewText("custom left click");

                if (!leftClick)
                {
                    
                    // this call skips the meed for Main.mouseLeftRelease to be true
                    if (VanillaLeftClick(inventory, context, slot))
                        return false;

                    Main.mouseLeftRelease = true;
                }

                // do the normal left click
                ItemSlot.LeftClick(inventory, context, slot);

                Main.mouseLeftRelease = mlr;
            }

            else if (Main.mouseRight && holdingSpecialKey && (!dragging || slotChanged))
            {
                dragging = true;
                Main.NewText($"context: {context}, slot: {slot}");
                bool mrr = Main.mouseRightRelease;

                // skip right click for the same reason as above
                if (mrr) return false;

                Main.mouseRightRelease = true;
                ItemSlot.RightClick(inventory, context, slot);

                Main.mouseRightRelease = mrr;
            }

            return base.HoverSlot(inventory, context, slot);
        }

        internal static MethodInfo ItemSlot_OverrideLeftClick = typeof(ItemSlot).GetMethod("OverrideLeftClick", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo ItemSlot_LeftClick_SellOrTrash = typeof(ItemSlot).GetMethod("LeftClick_SellOrTrash", BindingFlags.NonPublic | BindingFlags.Static);

        private static bool VanillaLeftClick(Item[] inventory, int context, int slot)
        {
            Player player = Main.LocalPlayer;
            bool leftClick = Main.mouseLeftRelease && Main.mouseLeft || true;
            if (leftClick)
            {
                //Main.NewText($"cursorOverride: {Main.cursorOverride}");
                if ((bool)ItemSlot_OverrideLeftClick?.Invoke(null, [inventory, context, slot]))
                    return true;

                inventory[slot].newAndShiny = false;
                if ((bool)ItemSlot_LeftClick_SellOrTrash?.Invoke(null, [inventory, context, slot]) || player.itemAnimation != 0 || player.itemTime != 0)
                    return true;
            }
            return false;
        }
    }
}
