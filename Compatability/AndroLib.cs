using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag.Compatability;
public static class AndroLib
{
    internal static string modName = "androLib";
    public static Mod Instance = null;
    public static bool Enabled = ModLoader.TryGetMod(modName, out Instance);

    /// <summary>
    /// Call before ItemSlot.Handle()
    /// </summary>
    public static void FixDoubleClickInBags()
    {
        if (!Enabled) return;

        var player = Main.LocalPlayer.GetModPlayer<InventoryPlayer>();
        // This function is used by androLib/Vacuum Bags. This fixes a double click issue.
        player.noSlot = false;
    }

    /// <summary>
    /// Call in InventoryPlayer.HandleLeftClick()
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool PreventDoubleClickInJourneyMode(int context, bool overrideShiftLeftClick)
    {
        if (!Enabled) return false;

        return context == ItemSlot.Context.CreativeInfinite && overrideShiftLeftClick;
    }
}
