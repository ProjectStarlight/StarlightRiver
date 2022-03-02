using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    [AutoloadEquip(EquipType.Head)]
    public class GeomancerHood : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer hood");
            //Tooltip.SetDefault("15% increased ranged critical strike damage");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
            item.defense = 5;
            item.rare = 3;
        }

        /*public override void UpdateEquip(Player player)
		{
            player.GetModPlayer<CritMultiPlayer>().RangedCritMult += 0.15f;
		}*/
    }

    [AutoloadEquip(EquipType.Body)]
    public class GeomancerRobe : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer Robe");
            //Tooltip.SetDefault("10% increased ranged damage");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
            item.defense = 6;
            item.rare = 3;
        }

        /*public override void UpdateEquip(Player player)
        {
            player.rangedDamage += 0.1f;
        }*/

        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<GeomancerHood>() && legs.type == ModContent.ItemType<GeomancerPants>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "egshels update this lol";

            player.GetModPlayer<GeomancerPlayer>().SetBonusActive = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GeomancerPants : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer Pants");
            //Tooltip.SetDefault("up to 20% ranged critical strike damage based on speed");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
            item.defense = 5;
            item.rare = 3;
        }

        /*public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<CritMultiPlayer>().RangedCritMult += Math.Min(0.2f, player.velocity.Length() / 16f * 0.2f);
        }*/
    }

    public enum StoredGem
    {
        Diamond,
        Ruby,
        Sapphire,
        Emerald,
        Amethyst,
        Topaz,
        None,
        All
    }

    public class GeomancerPlayer : ModPlayer
    {
        public bool SetBonusActive = false;

        public StoredGem storedGem = StoredGem.None;

        public bool DiamondStored = false;
        public bool RubyStored = false;
        public bool EmeraldStored = false;
        public bool SapphireStored = false;
        public bool TopazStored = false;
        public bool AmethystStored = false;

        private int allTimer = 150;

        static Item rainbowDye;
        static bool rainbowDyeInitialized = false;
        static int shaderValue = 0;

        public override void ResetEffects()
        {
            if (!rainbowDyeInitialized)
            {
                rainbowDyeInitialized = true;
                rainbowDye = new Item();
                rainbowDye.SetDefaults(ModContent.ItemType<RainbowCycleDye>());
                shaderValue = rainbowDye.dye;
            }

            if (!SetBonusActive)
                storedGem = StoredGem.None;
            SetBonusActive = false;

            if (DiamondStored && RubyStored && EmeraldStored && SapphireStored && TopazStored && AmethystStored)
            {
                DiamondStored = false;
                RubyStored = false;
                EmeraldStored = false;
                SapphireStored = false;
                TopazStored = false;
                AmethystStored = false;

                storedGem = StoredGem.All;

                allTimer = 150;
            }

            if (storedGem == StoredGem.All)
            {
                allTimer--;
                if (allTimer < 0)
                    storedGem = StoredGem.None;
            }
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (SetBonusActive)
            {

                layers.Insert(layers.FindIndex(x => x.Name == "Head" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemHead",
                   delegate (PlayerDrawInfo info) {
                       DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerHood_Head_Gems"), info, info.drawPlayer.bodyFrame, info.drawPlayer.headRotation);
                   }));

                layers.Insert(layers.FindIndex(x => x.Name == "Body" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemBody",
                   delegate (PlayerDrawInfo info) {
                       DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerRobe_Body_Gems"), info, info.drawPlayer.bodyFrame, info.drawPlayer.bodyRotation);
                   }));

                layers.Insert(layers.FindIndex(x => x.Name == "Body" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemBody2",
                   delegate (PlayerDrawInfo info) {
                       DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerRobe_Body_Rims"), info, info.drawPlayer.bodyFrame, info.drawPlayer.bodyRotation);
                   }));


                layers.Insert(layers.FindIndex(x => x.Name == "Legs" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemLegs",
                    delegate (PlayerDrawInfo info) {
                        DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerPants_Legs_Gems"), info, info.drawPlayer.legFrame, info.drawPlayer.legRotation);
                    }));
            }
        }

        public void DrawGemArmor(Texture2D texture, PlayerDrawInfo info, Rectangle frame, float rotation)
        {
            Player armorOwner = info.drawPlayer;

            Vector2 drawPos = (armorOwner.MountedCenter - Main.screenPosition) - new Vector2(0, 3 - player.gfxOffY);
            float timerVar = (float)(Main.GlobalTime % 2.4f / 2.4f) * 6.28f;
            float timer = ((float)(Math.Sin(timerVar) / 2f) + 0.5f);

            timer = 0.8f;

            Filters.Scene["RainbowArmor"].GetShader().Shader.Parameters["uTime"].SetValue(Main.GlobalTime * 0.1f);

            DrawData value = new DrawData(
                        texture,
                        new Vector2((int)drawPos.X, (int)drawPos.Y),
                        frame,
                        GetArmorColor(armorOwner),
                        rotation,
                        new Vector2(frame.Width / 2, frame.Height / 2),
                        1,
                        armorOwner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0
                    )
            {
                //shader = armorOwner.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All ? shaderValue : 0
                shader = shaderValue
            };

            Main.playerDrawData.Add(value);

            /*for (float i = 0; i < 6.28f; i += 1.57f)
            {
                Vector2 offset = (i.ToRotationVector2() * 2 * timer);
                DrawData value2 = new DrawData(
                        texture,
                        new Vector2((int)drawPos.X, (int)drawPos.Y) + offset,
                        frame,
                        armorOwner.GetDeathAlpha(GetArmorColor(armorOwner)) * 0.25f,
                        rotation,
                        new Vector2(frame.Width / 2, frame.Height / 2),
                        1,
                        armorOwner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0
                    )
                {
                 //shader = armorOwner.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All ? shaderValue : 0
                    shader = shaderValue
                };

                Main.playerDrawData.Add(value2);
            }*/
        }

        public override void PreUpdate()
        {
            if (!SetBonusActive)
                return;

            Lighting.AddLight(player.Center, (GetArmorColor(player)).ToVector3());
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (!SetBonusActive)
                return;

            if ((crit || target.life <= 0) && Main.rand.NextBool(1) && storedGem != StoredGem.All) //TOOD: Change nextbool to 15
            {
                SpawnGem(target);
            }


            if (crit && (storedGem == StoredGem.Sapphire || storedGem == StoredGem.All)) //TODO: instead of being just on crit, make it equal to crit chance
            {
                int numStars = Main.rand.Next(3) + 1;
                for (int i = 0; i < numStars; i++) //Doing a loop so they spawn separately
                {
                    Item.NewItem(new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), ItemID.Star);
                }
            }

            if (crit && (storedGem == StoredGem.Emerald || storedGem == StoredGem.All))
            {
                Item.NewItem(new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), ModContent.ItemType<EmeraldHeart>());
            }

        }

        private static void SpawnGem(NPC target)
        {
            int itemType = -1;
            switch (Main.rand.Next(6))
            {
                case 0:
                    itemType = ModContent.ItemType<GeoDiamond>();
                    break;
                case 1:
                    itemType = ModContent.ItemType<GeoRuby>();
                    break;
                case 2:
                    itemType = ModContent.ItemType<GeoEmerald>();
                    break;
                case 3:
                    itemType = ModContent.ItemType<GeoSapphire>();
                    break;
                case 4:
                    itemType = ModContent.ItemType<GeoAmethyst>();
                    break;
                default:
                    itemType = ModContent.ItemType<GeoTopaz>();
                    break;
            }
            Item.NewItem(new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), itemType, 1);
        }

        public static Color GetArmorColor(Player player)
        {
            StoredGem storedGem = player.GetModPlayer<GeomancerPlayer>().storedGem;

            return Color.White;
            return Main.hslToRgb((Main.GlobalTime * 0.2f) % 1, 1f, 0.5f);

            switch (storedGem)
            {
                case StoredGem.All:
                    return Main.hslToRgb((Main.GlobalTime * 0.1f) % 1, 1f, 0.5f);
                case StoredGem.Amethyst:
                    return Color.Purple;
                case StoredGem.Topaz:
                    return Color.Yellow;
                case StoredGem.Emerald:
                    return Color.Green;
                case StoredGem.Sapphire:
                    return Color.Blue;
                case StoredGem.Diamond:
                    return Color.Cyan;
                case StoredGem.Ruby:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }
    }

    public abstract class GeoGem : ModItem
    {

        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            SetName();
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            SetBonus(player);
            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 100);

        protected virtual void SetName() { }

        protected virtual void SetBonus(Player player) { }
    }

    public class GeoDiamond : GeoGem
    {
        protected override void SetName() => DisplayName.SetDefault("Diamond");

        protected override void SetBonus(Player player)
        {
            player.GetModPlayer<GeomancerPlayer>().DiamondStored = true;
            player.GetModPlayer<GeomancerPlayer>().storedGem = StoredGem.Diamond;
        }
    }

    public class GeoRuby : GeoGem
    {
        protected override void SetName() => DisplayName.SetDefault("Ruby");

        protected override void SetBonus(Player player)
        {
            player.GetModPlayer<GeomancerPlayer>().RubyStored = true;
            player.GetModPlayer<GeomancerPlayer>().storedGem = StoredGem.Ruby;
        }
    }

    public class GeoEmerald : GeoGem
    {
        protected override void SetName() => DisplayName.SetDefault("Emerald");

        protected override void SetBonus(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 20);
            player.HealEffect(20);
            player.statLife += healAmount;

            player.GetModPlayer<GeomancerPlayer>().EmeraldStored = true;
            player.GetModPlayer<GeomancerPlayer>().storedGem = StoredGem.Emerald;
        }
    }

    public class GeoSapphire : GeoGem
    {
        protected override void SetName() => DisplayName.SetDefault("Sapphire");

        protected override void SetBonus(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statManaMax2 - player.statMana, 200);
            player.ManaEffect(healAmount);
            player.statMana += healAmount;

            player.GetModPlayer<GeomancerPlayer>().SapphireStored = true;
            player.GetModPlayer<GeomancerPlayer>().storedGem = StoredGem.Sapphire;
        }
    }

    public class GeoTopaz : GeoGem
    {
        protected override void SetName() => DisplayName.SetDefault("Topaz");

        protected override void SetBonus(Player player)
        {
            player.GetModPlayer<GeomancerPlayer>().TopazStored = true;
            player.GetModPlayer<GeomancerPlayer>().storedGem = StoredGem.Topaz;
        }
    }

    public class GeoAmethyst : GeoGem
    {
        protected override void SetName() => DisplayName.SetDefault("Amethyst");

        protected override void SetBonus(Player player)
        {
            player.GetModPlayer<GeomancerPlayer>().AmethystStored = true;
            player.GetModPlayer<GeomancerPlayer>().storedGem = StoredGem.Amethyst;
        }
    }

    public class EmeraldHeart : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Emerald Heart");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 5);
            player.HealEffect(5);
            player.statLife += healAmount;

            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 100);
    }

    public class RainbowCycleDye : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;
        public override void SetDefaults()
        {
            /*				
			this.name = "Gel Dye";
			this.width = 20;
			this.height = 20;
			this.maxStack = 99;
			this.value = Item.sellPrice(0, 1, 50, 0);
			this.rare = 3;
			*/
            // item.dye is already assigned to this item prior to SetDefaults because of the GameShaders.Armor.BindShader code in ExampleMod.Load. 
            // This code here remembers item.dye so that information isn't lost during CloneDefaults. The above code is the data being cloned by CloneDefaults, for informational purposes.
            byte dye = item.dye;
            item.CloneDefaults(ItemID.GelDye);
            item.dye = dye;
        }
    }
}