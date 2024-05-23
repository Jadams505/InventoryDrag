using InventoryDrag.Compatability;
using InventoryDrag.Config;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag;

public class InventoryDrag : Mod
{
    internal static MethodInfo ItemLoader_CanRightClick = typeof(ItemLoader).GetMethod("CanRightClick", BindingFlags.Static | BindingFlags.Public);
    public override void Load()
    {
        Terraria.UI.On_ItemSlot.MouseHover_ItemArray_int_int += On_ItemSlot_MouseHover_ItemArray_int_int;
        Terraria.UI.On_ItemSlot.Handle_ItemArray_int_int += On_ItemSlot_Handle_ItemArray_int_int;
        Terraria.UI.On_ItemSlot.RightClick_ItemArray_int_int += On_ItemSlot_RightClick_ItemArray_int_int;
        Terraria.On_Main.DrawInventory += On_Main_DrawInventory;

        MonoModHooks.Add(ItemLoader_CanRightClick, On_ItemLoader_CanRightClick_Item);
        MonoModHooks.Add(PlayerLoader_ShiftClickSlot, On_PlayerLoader_ShiftClickSlot);
    }

    private void On_ItemSlot_Handle_ItemArray_int_int(On_ItemSlot.orig_Handle_ItemArray_int_int orig, Item[] inv, int context, int slot)
    {
        AndroLib.FixDoubleClickInBags();
        orig(inv, context, slot);
    }

    private void On_Main_DrawInventory(On_Main.orig_DrawInventory orig, Main self)
    {
        var player = Main.LocalPlayer.GetModPlayer<InventoryPlayer>();
        player.hovering = false;
        orig(self);
        player.noSlot = !player.hovering;
    }

    private delegate bool orig_ItemLoader_CanRightClick(Item item);
    private static bool On_ItemLoader_CanRightClick_Item(orig_ItemLoader_CanRightClick orig, Item item)
    {
        bool ret = orig(item);
        bool enabled = InventoryConfig.Instance.SplittableGrabBags.Enabled;
        if (enabled && ItemSlot.ShiftInUse && Main.ItemDropsDB.GetRulesForItemID(item.type).Count > 0)
        {
            return false;
        }
        return ret;
    }

    internal static MethodInfo PlayerLoader_ShiftClickSlot = typeof(PlayerLoader).GetMethod("ShiftClickSlot", BindingFlags.Static | BindingFlags.Public);
    private delegate bool orig_PlayerLoader_ShiftClickSlot(Player player, Item[] inventory, int context, int slot);
    private static bool On_PlayerLoader_ShiftClickSlot(orig_PlayerLoader_ShiftClickSlot orig, Player player, Item[] inventory, int context, int slot)
    {
        bool ret = orig(player, inventory, context, slot);
        if (player.TryGetModPlayer<InventoryPlayer>(out var invPlayer))
            invPlayer.overrideShiftLeftClick = ret;
        return ret;
    }

    private static void On_ItemSlot_RightClick_ItemArray_int_int(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
    {
        Main.LocalPlayer.GetModPlayer<InventoryPlayer>().rightClickCache = Main.mouseRightRelease;
        orig(inv, context, slot);
    }

    private static void On_ItemSlot_MouseHover_ItemArray_int_int(On_ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot)
    {
        if (Main.LocalPlayer.TryGetModPlayer<InventoryPlayer>(out var inventoryPlayer))
        {
            bool customClick = Main.LocalPlayer.GetModPlayer<InventoryPlayer>().OverrideHover(inv, context, slot);
        }

        // call orig after so that the tooltip does not display if items were moved
        orig(inv, context, slot);
    }

    internal static bool debugMessages = true;
    public static void DebugInChat(string text)
    {
        if (debugMessages)
            Main.NewText(text);
    }
}