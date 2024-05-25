using androLib;
using androLib.UI;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag.Compatability;
public static class AndroLib
{
    internal const string androLib = "androLib";
    public static Mod Instance = null;
    public static bool Enabled = ModLoader.TryGetMod(androLib, out Instance);

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

    public static void Load(Mod mod)
    {
        if (!Enabled) return;

        AndroLibReference.Load();
        mod.AddContent<AndroLibPlayer>();
    }

    public static void Unload(Mod mod)
    {
        AndroLibReference.Unload();
    }

    [JITWhenModsEnabled(androLib)]
    public static bool DidBagSlotChange()
    {
        if (!Enabled) return false;

        // can probably do better than this, but I think its good enough
        if (NoBagsHovered) return false;
        //InventoryDrag.DebugInChat($"id: {HoverId}");

        if (Main.LocalPlayer.TryGetModPlayer<AndroLibPlayer>(out var player))
        {
            return player.didSlotChange;
        }

        return false;
    }

    [JITWhenModsEnabled(androLib)]
    public static bool NoBagsHovered => MasterUIManager.NoUIBeingHovered;

    [JITWhenModsEnabled(androLib)]
    public static int HoverId => MasterUIManager.UIBeingHovered;

    [JITWhenModsEnabled(androLib)]
    public static void UpdateBagSlotCache()
    {
        if (!Enabled) return;

        AndroLibReference.UpdateItemSlot();
    }

}

[JITWhenModsEnabled("androLib")]
public static class AndroLibReference
{
    internal static FieldInfo BagUI_drawnUIData;
    public static void UpdateItemSlot()
    {
        var bagUI = StorageManager.BagUIs.FirstOrDefault(x => x.Hovering, null);
        if (bagUI is null) return;

        var data = BagUI_drawnUIData?.GetValue(bagUI) as BagUI.DrawnUIData;
        if (data is null) return;

        int index = Array.FindIndex(data.slotData, x => x.IsMouseHovering);
        if (index == -1) return;

        if (Main.LocalPlayer.TryGetModPlayer<AndroLibPlayer>(out var player) is false) return;

        player.UpdateSlotChange(bagUI.ID, index);
    }

    public static void Load()
    {
        BagUI_drawnUIData = typeof(BagUI).GetField("drawnUIData", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public static void Unload()
    {
        BagUI_drawnUIData = null;
    }
}

[Autoload(false)]
public class AndroLibPlayer : ModPlayer
{
    internal int slotIdCache;
    internal int slotIndexCache;
    internal bool didSlotChange = false;

    public void UpdateSlotChange(int slotId, int slotIndex)
    {
        if (Main.LocalPlayer.whoAmI != Player.whoAmI) return;

        didSlotChange = slotId != slotIdCache || slotIndex != slotIndexCache;
        slotIdCache = slotId;
        slotIndexCache = slotIndex;
    }
}


