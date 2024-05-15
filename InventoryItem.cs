using InventoryDrag.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace InventoryDrag;

public class InventoryItem : GlobalItem
{
    public static bool CanShiftStack(Item item)
    {
        if (Main.ItemDropsDB.GetRulesForItemID(item.type).Count > 0)
        {
            return true;
        }
        return false;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        var config = InventoryConfig.Instance.SplittableGrabBags;
        bool enabled = config.Enabled && config.ShowTooltip;
        if (enabled && CanShiftStack(item))
        {
            // index of the Right Click to Open tooltip (not a good way to do it)
            int rightClickIndex = tooltips.FindIndex(x => x.Name == "Tooltip0");
            tooltips.Insert(rightClickIndex, new TooltipLine(Mod, "Shift", "Shift Click to unstack"));

            // when holding shift remove the right click tooltip since it is no longer its function
            if (ItemSlot.ShiftInUse)
            {
                tooltips[rightClickIndex + 1].Hide();
            }
        }
    }
}
