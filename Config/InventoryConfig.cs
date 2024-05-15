﻿using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

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
}

public class LeftMouseOptions
{
    public bool Enabled = true;
    public ModifierOptions ModifierOptions = new();

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
    public ModifierOptions ModifierOptions = new();

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
