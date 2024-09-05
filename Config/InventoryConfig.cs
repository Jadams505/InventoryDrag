using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace InventoryDrag.Config;

public class InventoryConfig : ModConfig
{
    public static InventoryConfig Instance => ModContent.GetInstance<InventoryConfig>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("MainFeatures")]
    [Expand(false)]
    public LeftMouseOptions LeftMouse = new();

    [Expand(false)]
    public RightMouseOptions RightMouse = new();

    [Header("ExtraFeatures")]
    [Expand(false)]
    public SplittableGrabBags SplittableGrabBags = new();

    [Expand(false)]
    public ThrowDragging ThrowDragging = new();

    public bool DebugMessages = false;
}

public class LeftMouseOptions
{
    public bool Enabled = true;
    public ModifierOptions ModifierOptions = new(
        allowCtrl: true, 
        allowShift: true, 
        allowAlt: true, 
        allowThrow: true,
        requireModifier: true);

    public override bool Equals(object obj)
    {
        return obj is LeftMouseOptions options &&
               Enabled == options.Enabled &&
               EqualityComparer<ModifierOptions>.Default.Equals(ModifierOptions, options.ModifierOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, ModifierOptions);
    }
}

public class RightMouseOptions
{
    public bool Enabled = true;
    public ModifierOptions ModifierOptions = new()
    {
        AllowCtrl = true,
        AllowShift = true,
        AllowAlt = true,
        AllowThrow = false,
        RequireModifier = false,
    };

    public override bool Equals(object obj)
    {
        return obj is RightMouseOptions options &&
               Enabled == options.Enabled &&
               EqualityComparer<ModifierOptions>.Default.Equals(ModifierOptions, options.ModifierOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, ModifierOptions);
    }
}

public class ModifierOptions
{
    public bool AllowCtrl = true;
    public bool AllowShift = true;
    public bool AllowAlt = true;
    public bool AllowThrow = true;
    public bool RequireModifier = false;

    public ModifierOptions()
    {
        AllowCtrl = true;
        AllowShift = true;
        AllowAlt = true;
        AllowThrow = true;
        RequireModifier = false;
    }

    public ModifierOptions(bool allowCtrl, bool allowShift, bool allowAlt, bool allowThrow, bool requireModifier)
    {
        AllowCtrl = allowCtrl;
        AllowShift = allowShift;
        AllowAlt = allowAlt;
        AllowThrow = allowThrow;
        RequireModifier = requireModifier;
    }

    public bool IsSatisfied(Player player)
    {
        var altHeld = Main.keyState.IsKeyDown(Main.FavoriteKey);
        var ctrlHeld = ItemSlot.ControlInUse;
        var shiftHeld = ItemSlot.ShiftInUse;
        var throwHeld = player.controlThrow;
        //TODO: consider the order of presedence in vanilla: Alt > Ctrl > Shift
        if (!AllowAlt && altHeld) return false;
        if (!AllowCtrl && ctrlHeld) return false;
        if (!AllowShift && shiftHeld) return false;
        if (!AllowThrow && throwHeld) return false;
        if (RequireModifier && !(altHeld || ctrlHeld || shiftHeld || throwHeld)) return false;
        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is ModifierOptions options &&
               AllowCtrl == options.AllowCtrl &&
               AllowShift == options.AllowShift &&
               AllowAlt == options.AllowAlt &&
               AllowThrow == options.AllowThrow &&
               RequireModifier == options.RequireModifier;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AllowCtrl, AllowShift, AllowAlt, AllowThrow, RequireModifier);
    }
}

public class ThrowDragging
{
    public bool PlaySound = true;

    [Range(0, 100)]
    public int ThrowDelay = 10;

    public override bool Equals(object obj)
    {
        return obj is ThrowDragging dragging &&
               PlaySound == dragging.PlaySound &&
               ThrowDelay == dragging.ThrowDelay;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PlaySound, ThrowDelay);
    }
}


public class SplittableGrabBags
{
    public bool Enabled = true;

    public bool ShowTooltip = true;

    public override bool Equals(object obj)
    {
        return obj is SplittableGrabBags bags &&
               Enabled == bags.Enabled &&
               ShowTooltip == bags.ShowTooltip;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, ShowTooltip);
    }
}

