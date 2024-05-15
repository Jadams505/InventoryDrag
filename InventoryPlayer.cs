using InventoryDrag.Config;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag;

public class InventoryPlayer : ModPlayer
{
    internal int contextCache = -1;
    internal int slotCache = -1;
    internal bool dragging = false;
    internal int itemCache = ItemID.None;

    // This is required for right clickable items like crates and bags.
    // Basically the vanilla functions set Main.mouseRightRelease to false so
    // I can't differentiate between whether a click happened or not
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

        // you can right click an empty vanity slot to switch with main equip
        // as far as I know this might be the only case for a empty slot to be draggable
        // TODO: validate this claim and only allow for right click
        bool allowEmptySlots = context == ItemSlot.Context.EquipArmorVanity || context == ItemSlot.Context.EquipAccessoryVanity;
        if (inventory[slot].IsAir && !allowEmptySlots) return false;

        // this seems dumb. A better way must exist
        if (!(Main.mouseLeft || Main.mouseRight))
        {
            dragging = false;
        }

        if (Main.mouseLeft && (!dragging || slotChanged))
        {
            HandleLeftClick(inventory, context, slot);
        }

        else if (Main.mouseRight && (!dragging || slotChanged))
        {
            HandleRightClick(inventory, context, slot);
        }

        return base.HoverSlot(inventory, context, slot);
    }

    /// <summary>
    /// Performs a left click if the config allows
    /// </summary>
    /// <param name="inventory">The collection of items</param>
    /// <param name="context">What type of slot should be clicked on</param>
    /// <param name="slot">The index in inventory containing the current item</param>
    /// <returns>True if an additional left click was fired</returns>
    private bool HandleLeftClick(Item[] inventory, int context, int slot)
    {
        dragging = true;
        bool mouseLeftRelease = Main.mouseLeftRelease;

        // skip when the mouse was just pressed down since vanilla can handle it as a click
        if (mouseLeftRelease)
        {
            Main.NewText($"vanilla left click context: {context}, slot: {slot}");
            return false;
        }

        // skip extra left click if disabled by config
        var leftMouse = InventoryConfig.Instance.LeftMouse;
        if (!leftMouse.Enabled) return false;
        if (!leftMouse.ModifierOptions.IsSatisfied()) return false;

        Main.NewText($"custom left click context: {context}, slot: {slot}");

        // this call skips the need for Main.mouseLeftRelease to be true
        if (VanillaLeftClick(inventory, context, slot))
            return true;

        Main.mouseLeftRelease = true;
        ItemSlot.LeftClick(inventory, context, slot);
        Main.mouseLeftRelease = mouseLeftRelease;

        return true;
    }

    // Returns true if an additional right click was fired
    private bool HandleRightClick(Item[] inventory, int context, int slot)
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

        return true;
    }

    internal static MethodInfo ItemSlot_OverrideLeftClick = typeof(ItemSlot).GetMethod("OverrideLeftClick", BindingFlags.NonPublic | BindingFlags.Static);
    internal static MethodInfo ItemSlot_LeftClick_SellOrTrash = typeof(ItemSlot).GetMethod("LeftClick_SellOrTrash", BindingFlags.NonPublic | BindingFlags.Static);

    /// <summary>
    /// Same as vanilla's ItemSlot.LeftClick() but removes the need for Main.mouseLeftRelease since
    /// it could already be reset from the original left click call
    /// </summary>
    /// <returns>True if a click was perfomed</returns>
    private static bool VanillaLeftClick(Item[] inventory, int context, int slot)
    {
        Player player = Main.LocalPlayer;
        bool leftClick = /* Main.mouseLeftRelease && */Main.mouseLeft;
        if (leftClick)
        {
            if ((bool)ItemSlot_OverrideLeftClick?.Invoke(null, [inventory, context, slot]))
                return true;

            inventory[slot].newAndShiny = false;
            if ((bool)ItemSlot_LeftClick_SellOrTrash?.Invoke(null, [inventory, context, slot]) || player.itemAnimation != 0 || player.itemTime != 0)
                return true;
        }
        return false;
    }
}
