using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
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

        // This is required for right clickable items like crates and bags.
        // Basically the vanilla functions set Main.mouseRightRelease to false so
        // I can't differenciate between whether a click happened or not
        // This variable caches the value before right click is called.
        internal bool rightClickCache = Main.mouseRightRelease;

        // This is called directly before ItemSlot.MouseHover()
        public bool OverrideHover(Item[] inventory, int context, int slot)
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
            bool allowEmptySlots = context == ItemSlot.Context.EquipArmorVanity || context == ItemSlot.Context.EquipAccessoryVanity;
            if (inventory[slot].IsAir && !allowEmptySlots) return false;

            // this seems dumb. A better way must exist
            if (!((Main.mouseLeft || Main.mouseRight) && holdingSpecialKey))
            {
                dragging = false;
            }

            if (Main.mouseLeft && holdingSpecialKey && (!dragging || slotChanged))
            {
                dragging = true;
                bool leftClick = Main.mouseLeftRelease && Main.mouseLeft;
                bool mlr = Main.mouseLeftRelease;

                // skip when the mouse was just pressed down since vanilla can handle it as a click
                if (mlr)
                {
                    Main.NewText($"vanilla left click context: {context}, slot: {slot}");
                    return false;
                }
                    
                Main.NewText($"custom left click context: {context}, slot: {slot}");

                if (!leftClick)
                {
                    
                    // this call skips the need for Main.mouseLeftRelease to be true
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
                bool mrr = Main.mouseRightRelease;
                bool rightClickable = context == ItemSlot.Context.InventoryItem && ItemLoader.CanRightClick(inventory[slot]);
                bool vanillaHandled = context == ItemSlot.Context.GuideItem || context == ItemSlot.Context.CraftingMaterial;

                // skip right click since vanilla already clicked
                // also skip if it can be right clicked since this would have already been handled before
                // HoverSlot is called (prevents double consumption)
                // TODO: Check this logic (mrr == false if rightClickable == true)?
                if (mrr || (rightClickCache && rightClickable) || vanillaHandled)
                {
                    Main.NewText($"vanilla right click context: {context}, slot: {slot} release: {Main.mouseRightRelease} cache: {rightClickCache}");
                    return false;
                }

                Main.NewText($"custom right click context: {context}, slot: {slot}");

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
