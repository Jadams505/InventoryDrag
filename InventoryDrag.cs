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
        Terraria.UI.On_ItemSlot.RightClick_ItemArray_int_int += On_ItemSlot_RightClick_ItemArray_int_int;

        MonoModHooks.Add(ItemLoader_CanRightClick, On_ItemLoader_CanRightClick_Item);
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

    private static void On_ItemSlot_RightClick_ItemArray_int_int(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
    {
        Main.LocalPlayer.GetModPlayer<InventoryPlayer>().rightClickCache = Main.mouseRightRelease;
        orig(inv, context, slot);
    }

    private static void On_ItemSlot_MouseHover_ItemArray_int_int(On_ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot)
    {
        Main.LocalPlayer.GetModPlayer<InventoryPlayer>().OverrideHover(inv, context, slot);

        // call orig after so that the tooltip does not display if items were moved
        orig(inv, context, slot);
    }
}