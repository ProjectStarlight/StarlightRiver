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

        public int allTimer = 150;
        public float ActivationCounter = 0;

        static Item rainbowDye;
        static bool rainbowDyeInitialized = false;
        static int shaderValue = 0;
        static int shaderValue2 = 0;


        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreDrawEvent += PreDrawGlowFX;
            return base.Autoload(ref name);
        }

        private void PreDrawGlowFX(Player player, SpriteBatch spriteBatch)
        {
            if (!player.GetModPlayer<GeomancerPlayer>().SetBonusActive)
                return;

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Effect effect = Filters.Scene["RainbowAura"].GetShader().Shader;

            if (player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All)
            {
                float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);
                float opacity = 1.25f - (((sin / 2) + 0.5f) * 0.8f);

                effect.Parameters["uTime"].SetValue(Main.GlobalTime * 0.6f);
                effect.Parameters["uOpacity"].SetValue(opacity);
                effect.CurrentTechnique.Passes[0].Apply();

                for (int k = 0; k < 6; k++)
                {
                    Vector2 dir = Vector2.UnitX.RotatedBy(k / 6f * 6.28f) * (5.5f + sin * 2.2f);
                    var color = Color.White * (opacity - sin * 0.1f) * 0.9f;

                    spriteBatch.Draw(CustomHooks.PlayerTarget.Target, Vector2.Zero + dir, color * 0.25f);
                }
            }
            else if (player.GetModPlayer<GeomancerPlayer>().ActivationCounter > 0)
            {
                float sin = player.GetModPlayer<GeomancerPlayer>().ActivationCounter;
                float opacity = 1.5f - sin;

                Color color = GetArmorColor(player) * (opacity - sin * 0.1f) * 0.9f;

                effect.Parameters["uColor"].SetValue(color.ToVector3());
                effect.Parameters["uOpacity"].SetValue(sin);
                effect.CurrentTechnique.Passes[1].Apply();

                for (int k = 0; k < 6; k++)
                {
                    Vector2 dir = Vector2.UnitX.RotatedBy(k / 6f * 6.28f) * (sin * 8f);

                    spriteBatch.Draw(CustomHooks.PlayerTarget.Target, Vector2.Zero + dir, Color.White * 0.25f);
                }
            }

            spriteBatch.End();

            SamplerState samplerState = Main.DefaultSamplerState;

            if (player.mount.Active)
                samplerState = Main.MountedSamplerState;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
        }

        public override void ResetEffects()
        {
            if (!rainbowDyeInitialized)
            {
                rainbowDyeInitialized = true;
                rainbowDye = new Item();
                rainbowDye.SetDefaults(ModContent.ItemType<RainbowCycleDye>());
                shaderValue = rainbowDye.dye;

                Item rainbowDye2 = new Item();
                rainbowDye2.SetDefaults(ModContent.ItemType<RainbowCycleDye2>());
                shaderValue2 = rainbowDye2.dye;
            }

            if (!SetBonusActive)
            {
                storedGem = StoredGem.None;
                DiamondStored = false;
                RubyStored = false;
                EmeraldStored = false;
                SapphireStored = false;
                TopazStored = false;
                AmethystStored = false;
            }
            SetBonusActive = false;

            /*if (DiamondStored && RubyStored && EmeraldStored && SapphireStored && TopazStored && AmethystStored)
            {
                DiamondStored = false;
                RubyStored = false;
                EmeraldStored = false;
                SapphireStored = false;
                TopazStored = false;
                AmethystStored = false;

                storedGem = StoredGem.All;

                allTimer = 150;
            }*/
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (SetBonusActive)
            {

                layers.Insert(layers.FindIndex(x => x.Name == "Head" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemHead",
                   delegate (PlayerDrawInfo info)
                   {
                       DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerHood_Head_Gems"), info, info.drawPlayer.bodyFrame, info.drawPlayer.headRotation);
                   }));

                layers.Insert(layers.FindIndex(x => x.Name == "Body" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemBody",
                   delegate (PlayerDrawInfo info)
                   {
                       DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerRobe_Body_Gems"), info, info.drawPlayer.bodyFrame, info.drawPlayer.bodyRotation);
                   }));

                layers.Insert(layers.FindIndex(x => x.Name == "Body" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemBody2",
                   delegate (PlayerDrawInfo info)
                   {
                       DrawGemArmor(ModContent.GetTexture(AssetDirectory.GeomancerItem + "GeomancerRobe_Body_Rims"), info, info.drawPlayer.bodyFrame, info.drawPlayer.bodyRotation);
                   }));


                layers.Insert(layers.FindIndex(x => x.Name == "Legs" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "GemLegs",
                    delegate (PlayerDrawInfo info)
                    {
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

            Filters.Scene["RainbowArmor"].GetShader().Shader.Parameters["uTime"].SetValue(Main.GlobalTime * 0.1f);

            Filters.Scene["RainbowArmor2"].GetShader().Shader.Parameters["uTime"].SetValue(Main.GlobalTime * 0.1f);
            Filters.Scene["RainbowArmor2"].GetShader().Shader.Parameters["uOpacity"].SetValue(1.25f - timer);

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
                shader = armorOwner.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All ? shaderValue : 0
                //shader = shaderValue
            };

            Main.playerDrawData.Add(value);

            /*for (float i = 0; i < 6.28f; i += 1.57f)
            {
                Vector2 offset = (i.ToRotationVector2() * 2 * timer);
                DrawData value2 = new DrawData(
                        texture,
                        new Vector2((int)drawPos.X, (int)drawPos.Y) + offset,
                        frame,
                        GetArmorColor(armorOwner) * 0.25f,
                        rotation,
                        new Vector2(frame.Width / 2, frame.Height / 2),
                        1,
                        armorOwner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0
                    )
                {
                    //shader = armorOwner.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All ? shaderValue2: 0
                    shader = shaderValue2
                };

                Main.playerDrawData.Add(value2);
            }*/
        }

        public override void PreUpdate()
        {
            if (!SetBonusActive)
                return;

            if (storedGem == StoredGem.All)
            {
                allTimer--;
                if (allTimer < 0)
                    storedGem = StoredGem.None;
            }

            ActivationCounter -= 0.03f;
            Lighting.AddLight(player.Center, (GetArmorColor(player)).ToVector3());
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (!SetBonusActive)
                return;

            if ((crit || target.life <= 0) && Main.rand.NextBool(1) && storedGem != StoredGem.All) //TOOD: Change nextbool to 15
            {
                SpawnGem(target, player.GetModPlayer<GeomancerPlayer>());
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

            if ((storedGem == StoredGem.Ruby || storedGem == StoredGem.All) && Main.rand.NextFloat() > 0.3f && proj.type != ModContent.ProjectileType<RubyDagger>())
            {
                Projectile.NewProjectile(player.Center, Main.rand.NextVector2Circular(7, 7), ModContent.ProjectileType<RubyDagger>(), (int)(damage * 0.3f), knockback, player.whoAmI, target.whoAmI);
            }

            if (storedGem == StoredGem.Amethyst || storedGem == StoredGem.All && target.GetGlobalNPC<GeoNPC>().amethystDebuff < 400)
            {
                if (Main.rand.Next((10 / player.HeldItem.useTime) * (int)Math.Pow(target.GetGlobalNPC<GeoNPC>().amethystDebuff, 0.3f)) == 0)
                {
                    Projectile.NewProjectile(
                        target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height)),
                        Vector2.Zero,
                        ModContent.ProjectileType<AmethystShard>(),
                        0,
                        0,
                        player.whoAmI,
                        target.GetGlobalNPC<GeoNPC>().amethystDebuff,
                        target.whoAmI);
                    target.GetGlobalNPC<GeoNPC>().amethystDebuff += 30;
                }
            }
        }

        private static void SpawnGem(NPC target, GeomancerPlayer modPlayer)
        {
            int itemType = -1;
            List<int> itemTypes = new List<int>();

            if (!modPlayer.AmethystStored)
                itemTypes.Add(ModContent.ItemType<GeoAmethyst>());

            if (!modPlayer.TopazStored)
                itemTypes.Add(ModContent.ItemType<GeoTopaz>());

            if (!modPlayer.EmeraldStored)
                itemTypes.Add(ModContent.ItemType<GeoEmerald>());

            if (!modPlayer.SapphireStored)
                itemTypes.Add(ModContent.ItemType<GeoSapphire>());

            if (!modPlayer.RubyStored)
                itemTypes.Add(ModContent.ItemType<GeoRuby>());

            if (!modPlayer.DiamondStored)
                itemTypes.Add(ModContent.ItemType<GeoDiamond>());

            if (itemTypes.Count == 0)
                return;

            itemType = itemTypes[Main.rand.Next(itemTypes.Count)];

            Item.NewItem(new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), itemType, 1);
        }

        public static Color GetArmorColor(Player player)
        {
            StoredGem storedGem = player.GetModPlayer<GeomancerPlayer>().storedGem;

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

    public class GeoNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int amethystDebuff;

        public override bool PreAI(NPC npc)
        {
            if (amethystDebuff > 0)
                amethystDebuff--;

            return base.PreAI(npc);
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.lifeRegen > 0)
            {
                npc.lifeRegen = 0;
            }
            npc.lifeRegen -= amethystDebuff / 50;
            if (damage < 1)
            {
                damage = 1;
            }
        }
    }
}