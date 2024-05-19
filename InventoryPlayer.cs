using InventoryDrag.Config;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag;

public class InventoryPlayer : ModPlayer
{
    internal int contextCache = -1;
    internal int slotCache = -1;
    internal int itemCache = ItemID.None;
    internal bool hovering = false;

    /// <summary>
    /// Set by InventoryDrag.DrawInventory() to determine when the player is no longer hovering over a slot
    /// </summary>
    internal bool noSlot = true; 

    // This is required for right clickable items like crates and bags.
    // Basically the vanilla functions set Main.mouseRightRelease to false so
    // I can't differentiate between whether a click happened or not
    // This variable caches the value before right click is called.
    internal bool rightClickCache = Main.mouseRightRelease;

    // This is called directly before ItemSlot.MouseHover()
    public bool OverrideHover(Item[] inventory, int context, int slot)
    {
        hovering = true;

        // journey mode slots are always context 29 and slot 0 so their only difference is the item 
        bool journeyModeSlotChange = itemCache != inventory[slot].type && context == ItemSlot.Context.CreativeInfinite;

        bool slotChanged = noSlot || contextCache != context || slotCache != slot || journeyModeSlotChange;
        contextCache = context;
        slotCache = slot;
        itemCache = inventory[slot].type;

        // you can right click an empty vanity slot to switch with main equip
        // as far as I know this might be the only case for a empty slot to be draggable
        // TODO: validate this claim and only allow for right click
        bool allowEmptySlots = context == ItemSlot.Context.EquipArmorVanity || context == ItemSlot.Context.EquipAccessoryVanity;
        if (inventory[slot].IsAir && !allowEmptySlots) return false;

        if (Main.mouseLeft && slotChanged)
        {
            HandleLeftClick(inventory, context, slot);
        }

        else if (Main.mouseRight && slotChanged)
        {
            HandleRightClick(inventory, context, slot);
        }

        return base.HoverSlot(inventory, context, slot);
    }

    /// <summary>
    /// Performs a left click if the config allows
    /// </summary>
    /// <returns>True if an additional left click was fired</returns>
    private bool HandleLeftClick(Item[] inventory, int context, int slot)
    {
        bool mouseLeftRelease = Main.mouseLeftRelease;

        // skip when the mouse was just pressed down since vanilla already handled it as a click
        if (mouseLeftRelease)
        {
            InventoryDrag.DebugInChat($"vanilla left click context: {context}, slot: {slot}");
            return false;
        }

        // skip extra left click if disabled by config
        var leftMouse = InventoryConfig.Instance.LeftMouse;
        if (!leftMouse.Enabled) return false;
        if (!leftMouse.ModifierOptions.IsSatisfied()) return false;

        InventoryDrag.DebugInChat($"custom left click context: {context}, slot: {slot}");

        // this call skips the need for Main.mouseLeftRelease to be true
        if (VanillaLeftClick(inventory, context, slot))
            return true;

        Main.mouseLeftRelease = true;
        ItemSlot.LeftClick(inventory, context, slot);
        Main.mouseLeftRelease = mouseLeftRelease;

        return true;
    }

    /// <summary>
    /// Performs a right click if the config allows
    /// </summary>
    /// <returns>True if an additional right click was fired</returns>
    private bool HandleRightClick(Item[] inventory, int context, int slot)
    {
        bool mouseRightRelease = Main.mouseRightRelease;
        bool rightClickable = context == ItemSlot.Context.InventoryItem && ItemLoader.CanRightClick(inventory[slot]);
        bool vanillaHandled = context == ItemSlot.Context.GuideItem || context == ItemSlot.Context.CraftingMaterial;

        // skip right click since vanilla already clicked
        // also skip if it can be right clicked since this would have already been
        // handled before HoverSlot is called (prevents double consumption)
        // TODO: Check this logic (mouseRightRelease == false if rightClickable == true)?
        if (mouseRightRelease || (rightClickCache && rightClickable) || vanillaHandled)
        {
            InventoryDrag.DebugInChat($"vanilla right click context: {context}, slot: {slot} release: {Main.mouseRightRelease} cache: {rightClickCache}");
            return false;
        }

        // skip extra right click if disabled by config
        var rightMouse = InventoryConfig.Instance.RightMouse;
        if (!rightMouse.Enabled) return false;
        if (!rightMouse.ModifierOptions.IsSatisfied()) return false;

        InventoryDrag.DebugInChat($"custom right click context: {context}, slot: {slot}");

        Main.mouseRightRelease = true;
        ItemSlot.RightClick(inventory, context, slot);
        Main.mouseRightRelease = mouseRightRelease;

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
