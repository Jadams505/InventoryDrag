using System;
using System.Collections.Generic;
using Terraria.ModLoader.Config;

namespace InventoryDrag.Config;

public class InventoryConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("MainFeatures")]
    [Expand(false)]
    public LeftMouseOptions LeftMouse = new();

    [Expand(false)]
    public RightMouseOptions RightMouse = new();

    [Header("ExtraFeatures")]
    [Expand(false)]
    public StackableGrabBags StackableGrabBags = new();
}

public class LeftMouseOptions
{
    public bool Enable = true;
    public ModifierOptions ModifierOptions = new();

    public override bool Equals(object obj)
    {
        return obj is LeftMouseOptions options &&
               Enable == options.Enable &&
               EqualityComparer<ModifierOptions>.Default.Equals(ModifierOptions, options.ModifierOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enable, ModifierOptions);
    }
}

public class RightMouseOptions
{
    public bool Enable = true;
    public ModifierOptions ModifierOptions = new();

    public override bool Equals(object obj)
    {
        return obj is RightMouseOptions options &&
               Enable == options.Enable &&
               EqualityComparer<ModifierOptions>.Default.Equals(ModifierOptions, options.ModifierOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enable, ModifierOptions);
    }
}

public class ModifierOptions
{
    public bool AllowCtrl = true;
    public bool AllowShift = true;
    public bool AllowAlt = true;

    public override bool Equals(object obj)
    {
        return obj is ModifierOptions options &&
               AllowCtrl == options.AllowCtrl &&
               AllowShift == options.AllowShift &&
               AllowAlt == options.AllowAlt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AllowCtrl, AllowShift, AllowAlt);
    }
}


public class StackableGrabBags
{
    public bool Enable = true;

    public bool ShowTooltip = true;

    public override bool Equals(object obj)
    {
        return obj is StackableGrabBags bags &&
               Enable == bags.Enable &&
               ShowTooltip == bags.ShowTooltip;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enable, ShowTooltip);
    }
}

