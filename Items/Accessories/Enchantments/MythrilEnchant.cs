using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Items.Accessories.Enchantments
{
    public class MythrilEnchant : ModItem
    {
        private readonly Mod thorium = ModLoader.GetMod("ThoriumMod");

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mythril Enchantment");
            Tooltip.SetDefault(
@"'You feel the knowledge of your weapons seep into your mind'
25% increased weapon use speed");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.accessory = true;
            ItemID.Sets.ItemNoGravity[item.type] = true;
            item.rare = 5;
            item.value = 100000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FargoPlayer>(mod).AttackSpeed *= 1.25f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("FargowiltasSouls:AnyMythrilHead");
            recipe.AddIngredient(ItemID.MythrilChainmail);
            recipe.AddIngredient(ItemID.MythrilGreaves);
            
            if(Fargowiltas.Instance.ThoriumLoaded)
            {      
                recipe.AddIngredient(thorium.ItemType("MythrilStaff"));
                recipe.AddIngredient(ItemID.LaserRifle);
                recipe.AddIngredient(ItemID.ClockworkAssaultRifle);
                recipe.AddIngredient(ItemID.Gatligator);
                recipe.AddIngredient(ItemID.OnyxBlaster);
                recipe.AddIngredient(ItemID.Megashark);
                recipe.AddIngredient(thorium.ItemType("Trigun"));  
            }
            else
            {
                recipe.AddIngredient(ItemID.LaserRifle);
                recipe.AddIngredient(ItemID.ClockworkAssaultRifle);
                recipe.AddIngredient(ItemID.Gatligator);
                recipe.AddIngredient(ItemID.OnyxBlaster);
            }
            
            recipe.AddTile(TileID.CrystalBall);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
