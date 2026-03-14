using WeaveLoader.API;
using WeaveLoader.API.Item;
using WeaveLoader.API.Events;
using WeaveLoader.API.Native;
using System;
using System.Runtime.InteropServices;

namespace Cravings;

[Mod("cravings", Name = "Cravings: Foods and Drinks", Version = "0.1.0", Author = "UniPM")]
public class Cravings : IMod
{
    public static bool AddHunger(nint playerPtr, int amountToAdd = 2, float saturationMult = 1.0f)
    {
        if (playerPtr == nint.Zero) return false;
        if (!NativeOffsets.TryGet("Player", "foodData", out var foodDataOffset)) return false;
        
        nint foodDataPtr = playerPtr + foodDataOffset;
        if (foodDataPtr == nint.Zero) return false;

        int foodLevel = Marshal.ReadInt32(foodDataPtr);
        float saturation = BitConverter.Int32BitsToSingle(Marshal.ReadInt32(foodDataPtr + 4));

        int newFood = Math.Min(amountToAdd + foodLevel, 20);
        float newSat = Math.Min(saturation + (float)amountToAdd * saturationMult * 2.0f, (float)newFood);

        Marshal.WriteInt32(foodDataPtr, 0x0, newFood);
        Marshal.WriteInt32(foodDataPtr, 0x4, BitConverter.SingleToInt32Bits(newSat));

        return true;
    }
    public static int GetHunger(nint playerPtr)
    {
        if (playerPtr == nint.Zero) return -1;
        if (!NativeOffsets.TryGet("Player", "foodData", out var foodDataOffset)) return -1;

        nint foodDataPtr = playerPtr + foodDataOffset;
        if (foodDataPtr == nint.Zero) return -1;

        return Marshal.ReadInt32(foodDataPtr);
    }
    private sealed class Consumable : Item
    {
        public int hunger;
        public float satMultiplier;
        public override UseItemResult OnUseItem(UseItemContext ctx)
        {
            if (!IdHelper.TryGetItemIdentifier(ctx.ItemId, out Identifier item_identifier))
            {
                Logger.Info($"[Cravings] FAILED to grab Identifier for ID {ctx.ItemId}");
                return UseItemResult.CancelVanilla;
            }
            if (ctx.IsClientSide)
            {
                return UseItemResult.ContinueVanilla;
            }
            int hunger = GetHunger(ctx.NativePlayerPtr);
            if (hunger == -1 || hunger == 20) return UseItemResult.CancelVanilla;
            ctx.ConsumeInventoryItem(item_identifier, 1);
            Logger.Info($"{AddHunger(ctx.NativePlayerPtr, hunger, satMultiplier)}");
            return UseItemResult.CancelVanilla;
        }
    }
    # [TODO] add ConsumableContained : Consumable {} (allows an item to be given upon eating i.e. buckets or bowls) - uni
    public static RegisteredItem? cookedEgg;
    public static RegisteredItem? cookedRice;
    public static RegisteredItem? beefJerky;
    public static RegisteredItem? porkJerky;
    public static RegisteredItem? iceCream;

    public void OnInitialize()
    {

        cookedEgg = Registry.Item.Register("cravings:cooked_egg", new Consumable { hunger = 5, satMultiplier = 1.2f },
            new ItemProperties()
                .MaxStackSize(64)
                .Icon("cravings:item/cooked_egg")
                .InCreativeTab(CreativeTab.Food)
                .Name(Text.Translatable("item.cravings.cooked_egg")));
        
        cookedRice = Registry.Item.Register("cravings:cooked_rice", new Consumable { hunger = 3, satMultiplier = 0.9f }, # change to ConsumableContained for bowl - uni
            new ItemProperties()
                .MaxStackSize(16)
                .Icon("cravings:item/cooked_rice")
                .InCreativeTab(CreativeTab.Food)
                .Name(Text.Translatable("item.cravings.cooked_rice")));
        
        beefJerky = Registry.Item.Register("cravings:beef_jerky", new Consumable { hunger = 3, satMultiplier = 1.2f },
            new ItemProperties()
                .MaxStackSize(16)
                .Icon("cravings:item/beef_jerky")
                .InCreativeTab(CreativeTab.Food)
                .Name(Text.Translatable("item.cravings.beef_jerky")));

	porkJerky = Registry.Item.Register("cravings:pork_jerky", new Consumable { hunger = 3, satMultiplier = 1.2f },
	    new ItemProperties()
	        .MaxStackSize(16)
		.Icon("cravings:item/pork_jerky")
		.InCreativeTab(CreativeTab.Food)
		.Name(Text.Translatable("item.cravings.pork_jerky")));

	iceCream = Registry.Item.Register("cravings:ice_cream", new Consumable { hunger = 3, satMultiplier = 1.5f },
	    new ItemProperties()
	        .MaxStackSize(8)
		.Icon("cravings:item/ice_cream")
		.InCreativeTab(CreativeTab.Food)
		.Name(Text.Translatable("item.cravings.ice_cream")));

        Registry.Recipe.AddFurnace("minecraft:egg", "cravings:cooked_egg", 1.0f);

        Logger.Info("Cravings Initialized!");
    }
}
