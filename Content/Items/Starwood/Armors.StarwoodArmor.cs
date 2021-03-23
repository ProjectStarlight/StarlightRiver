using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items.Armor;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Starwood
{
    [AutoloadEquip(EquipType.Head)]
    public class StarwoodHat : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public StarwoodHat() : base(GetTexture(AssetDirectory.StarwoodItem + "StarwoodHat_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Hat");
            Tooltip.SetDefault("5% increased magic damage");
        }

        public override void SetDefaults()
        {
            item.width = 42;
            item.height = 26;
            item.value = Item.sellPrice(0, 0, 10, 0);
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.magicDamage += 0.05f;
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;
        }

        public override void UpdateVanity(Player player, EquipType type) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public void DrawArmorLayer(PlayerDrawInfo info)//custom drawing the hat (todo)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().Empowered)
                ArmorHelper.QuickDrawHeadFramed(info, AssetDirectory.StarwoodItem + "StarwoodHat_Head_Alt", 1, new Vector2(10, 18));
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class StarwoodChest : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public StarwoodChest() : base(GetTexture(AssetDirectory.StarwoodItem + "StarwoodChest_Alt")) { }

        public override bool Autoload(ref string name)//adds method to Starlight player event
        {
            StarlightPlayer.ModifyHitNPCEvent += ModifyHitNPCStarwood;
            return true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Robes");
            Tooltip.SetDefault("Increases max mana by 20");
        }

        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 30;
            item.value = Item.sellPrice(0, 0, 12, 0);
            item.defense = 3;
        }

        public override void UpdateEquip(Player player)
        {
            player.statManaMax2 += 20;
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;
        }

        public override void UpdateVanity(Player player, EquipType type) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ItemType<StarwoodHat>() && legs.type == ItemType<StarwoodBoots>();//what items are required for set

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Picking up mana stars empowers starwood items";
            StarlightPlayer mp = player.GetModPlayer<StarlightPlayer>();

            if (mp.EmpowermentTimer > 0 && ArmorHelper.IsSetEquipped(this, player)) //checks if complete to disable empowerment if set is removed
            {
                for (int k = 0; k < 1; k++)//temp gfx 
                    Dust.NewDustPerfect(player.position + new Vector2(Main.rand.Next(player.width), Main.rand.Next(player.height)), DustType<Dusts.BlueStamina>(), -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(1.0f, 1.4f), 0, default, 1.2f);
                mp.EmpowermentTimer--;
            }
            else 
            { 
                mp.EmpowermentTimer = 0; 
                mp.Empowered = false; 
            }
        }

        private void ModifyHitNPCStarwood(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)//sets bool on hit npcs
        {
            if (ArmorHelper.IsSetEquipped(this, player))
                target.GetGlobalNPC<ManastarDrops>().DropStar = true;
        }

        public void DrawArmorLayer(PlayerDrawInfo info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().Empowered)
                ArmorHelper.QuickDrawBodyFramed(info, AssetDirectory.StarwoodItem + "StarwoodChest_Body_Alt", 1, new Vector2(10, 18));
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class StarwoodBoots : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public StarwoodBoots() : base(GetTexture(AssetDirectory.StarwoodItem + "StarwoodBoots_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Leggings");
            Tooltip.SetDefault("5% increased magic critial strike chance");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 8, 0);
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.magicCrit += 5;
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;
        }

        public override void UpdateVanity(Player player, EquipType type) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public void DrawArmorLayer(PlayerDrawInfo info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().Empowered)
                ArmorHelper.QuickDrawLegsFramed(info, AssetDirectory.StarwoodItem + "StarwoodBoots_Legs_Alt", 1, new Vector2(10, 18));
        }
    }

    //makes star items start starwood empowerment, starting it just doesn't do anything is the player does not have the set equiped
    internal class ManastarPickup : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            if (new int[] { ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum }.Contains(item.type))
            {
                StarlightPlayer mp = player.GetModPlayer<StarlightPlayer>();
                mp.StartStarwoodEmpowerment();
            }
            return base.OnPickup(item, player);
        }
    }

    //makes npcs drop stars if the bool the armor-set sets is true
    internal class ManastarDrops : GlobalNPC
    {
        public bool DropStar = false;
        public override bool InstancePerEntity => true;
        public override void NPCLoot(NPC npc)
        {
            if (DropStar && Main.rand.Next(2) == 0)
            {
                Item.NewItem(npc.Center, ItemID.Star);
            }
        }
    }
}

//modplayer to handle empowerment

namespace StarlightRiver.Core
{
    public partial class StarlightPlayer : ModPlayer
    {
        public bool starwoodArmorComplete = false;
        public bool Empowered = false;
        public int EmpowermentTimer = 0;

        public void StartStarwoodEmpowerment()
        {
            if (player.armor[1].modItem is Content.Items.Starwood.StarwoodChest && ArmorHelper.IsSetEquipped(player.armor[1].modItem, player))//checks if complete, not completely needed but is there so empowered isnt true for a brief moment
            {
                if (!Empowered)
                    for (int k = 0; k < 80; k++)//pickup sfx
                        Dust.NewDustPerfect(player.Center, DustType<Content.Dusts.BlueStamina>(), (Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.8f, 1.2f)) * new Vector2(1f, 1.5f), 0, default, 1.5f);
                
                else
                    for (int k = 0; k < 40; k++)//reduced pickup sfx if its already active
                        Dust.NewDustPerfect(player.Center, DustType<Content.Dusts.BlueStamina>(), (Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.5f, 0.8f)) * new Vector2(1f, 1.5f), 0, default, 1.5f);
                
                Empowered = true;
                EmpowermentTimer = 600;//resets timer
            }
        }
    }
}