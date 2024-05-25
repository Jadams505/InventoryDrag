using androLib;
using androLib.UI;
using System;
using System.Reflection;
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

    [JITWhenModsEnabled("androLib")]
    public static bool DidBagSlotChange()
    {
        if (!Enabled) return false;

        // can probably do better than this, but I think its good enough
        if (NoBagsHovered) return false;

        if (Main.LocalPlayer.TryGetModPlayer<AndroLibPlayer>(out var player))
        {
            return player.didSlotChange;
        }

        return false;
    }

    [JITWhenModsEnabled("androLib")]
    public static bool NoBagsHovered => MasterUIManager.NoUIBeingHovered;

}

[JITWhenModsEnabled("androLib")]
public static class AndroLibReference
{
    internal static MethodInfo BagsUI_UpdateItemSlot = null;
    private delegate void orig_BagsUI_UpdateItemSlot(BagUI self, int inventoryIndex, BagUI bagUI, Item[] inventory, UIItemSlotData[] slotDatas);

    private static void On_BagsUI_UpdateItemSlot(orig_BagsUI_UpdateItemSlot orig, BagUI self, int inventoryIndex, BagUI bagUI, Item[] inventory, UIItemSlotData[] slotDatas)
    {
        try
        {
            UIItemSlotData slot = slotDatas[inventoryIndex];
            if (slot.IsMouseHovering)
            {
                if (Main.LocalPlayer.TryGetModPlayer<AndroLibPlayer>(out var player))
                {
                    player.UpdateSlotChange(slot.ID, inventoryIndex);
                }
                //InventoryDrag.DebugInChat($"topL: {slot.TopLeft} bottomR: {slot.BottomRight} id:{slot.ID} index: {inventoryIndex}");
            }

            orig(self, inventoryIndex, bagUI, inventory, slotDatas);
        }
        catch (NullReferenceException e)
        {
            // I don't know why this happens (seems to happen when reloading mods)
            // Nothing bads seems to happen though just the message in chat
            var message = e.Message;
        }
        catch (Exception e)
        {
            _ = e;
        }
    }

    public static void Load()
    {
        BagsUI_UpdateItemSlot = typeof(BagUI).GetMethod("UpdateItemSlot", BindingFlags.Instance | BindingFlags.NonPublic);
        MonoModHooks.Add(BagsUI_UpdateItemSlot, On_BagsUI_UpdateItemSlot);
    }

    public static void Unload()
    {
        BagsUI_UpdateItemSlot = null;
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


