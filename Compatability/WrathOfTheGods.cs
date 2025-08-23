using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.UI.Books;
using Terraria;
using Terraria.ModLoader;

namespace InventoryDrag.Compatability;
public static class WrathOfTheGods
{
	internal const string NoxusBoss = "NoxusBoss";
	public static Mod Instance = null;
	public static bool Enabled = ModLoader.TryGetMod(NoxusBoss, out Instance);

	public static bool IsBookshelfSlot(Item item) =>
		Enabled && WrathOfTheGodsReference.IsBookshelfSlot(item);

	public static bool IsUnclaimedReward(Item item) =>
		Enabled && WrathOfTheGodsReference.IsUnclaimedReward(item);
}

[JITWhenModsEnabled(WrathOfTheGods.NoxusBoss)]
public static class WrathOfTheGodsReference
{
	public static bool IsBookshelfSlot(Item item)
	{
		if (!item.TryGetGlobalItem<InstancedGlobalItem>(out var wrathItem))
		{
			return false;
		}

		if (wrathItem.SlotInBookshelfUI)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnclaimedReward(Item item)
	{
		if (!Main.LocalPlayer.TryGetModPlayer<SolynBookRewardsPlayer>(out var player)) return false;

		return player.UnclaimedRewards.Contains(item);
	}
}
