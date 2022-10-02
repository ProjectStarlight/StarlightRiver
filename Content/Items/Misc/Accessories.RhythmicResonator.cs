using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.UI;
using Terraria.GameInput;

namespace StarlightRiver.Content.Items.Misc
{
    public class RhythmicResonator : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public RhythmicResonator() : base("Rhythmic Resonator", "Attack in rhythm with your weapon to gradually increase damage and knockback\nDisables autoswing") { }

        public override void Load()
        {
            StarlightItem.CanAutoReuseItemEvent += PreventAutoReuse;
            StarlightItem.UseItemEvent += UseItemEffects;
        }

        public override void Unload()
        {
            StarlightItem.CanAutoReuseItemEvent -= PreventAutoReuse;
            StarlightItem.UseItemEvent -= UseItemEffects;
        }

        public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.GetModPlayer<RhythmicResonatorModPlayer>().equipped = true;
        }

        private bool? PreventAutoReuse(Item Item, Player Player)
        {
            if (Equipped(Player))
                return false;

            return null;
        }

        private bool? UseItemEffects(Item Item, Player Player)
        {
            var modPlayer = Player.GetModPlayer<RhythmicResonatorModPlayer>();
            if (Equipped(Player))
            {
                if (modPlayer.RhythmTimer > 0 && modPlayer.RhythmTimer < 5) // five frame window
                {
                    modPlayer.flashTimer = 20;
                    modPlayer.RhythmStacks++;
                    modPlayer.ResetTimer = Utils.Clamp((int)((Item.useTime * (1f - (Player.GetTotalAttackSpeed(Item.DamageType) - 1f))) * 2f), 30, 180); //set the reset timer for the stacks to triple the items use time, clamped at 3 seconds
                    SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 1.15f, Pitch = -0.2f, PitchVariance = 0.15f }, Player.Center);

                    if (Main.myPlayer == Player.whoAmI) //only spawn dust on client
                        for (int i = 0; i < 15; i++)
                        {
                            Dust.NewDustPerfect(Main.MouseWorld + (Vector2.One * 10) + Main.rand.NextVector2Circular(2.5f, 2.5f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1.75f, 1.75f), 0, Color.White, 0.7f);
                        }
                }

                float speed = Item.useTime * (1f - (Player.GetTotalAttackSpeed(Item.DamageType) - 1f));
                modPlayer.RhythmTimer = (int)(speed * (Item.useTime < 30 ? 1.2f : 1.15f));
                modPlayer.MaxRhythmTimer = (int)(speed * (Item.useTime < 30 ? 1.2f : 1.15f));
            }
            return null;
        }
    }

    public class RhythmicResonatorModPlayer : ModPlayer //needs a modplayer for the UI
    {
        public bool equipped;
        public int RhythmTimer;
        public int MaxRhythmTimer; //just used visually for the UI
        public int RhythmStacks;
        public int ResetTimer;
        public int flashTimer; // also for UI

        public override void ResetEffects()
        {
            equipped = false;

            RhythmStacks = Utils.Clamp(RhythmStacks, 0, 5);
            if (ResetTimer > 0)
                ResetTimer--;
            else
                RhythmStacks = 0;

            if (RhythmTimer > 0)
                RhythmTimer--;

            if (flashTimer > 0)
                flashTimer--;
        }

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (equipped && RhythmStacks > 0)
            {
                damage = (int)(damage * (1f + (0.05f * RhythmStacks))); //5% increase up to 25%
                knockback = knockback * (1f + (0.1f * RhythmStacks)); // 10% yadada
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (equipped && RhythmStacks > 0)
            {
                damage = (int)(damage * (1f + (0.05f * RhythmStacks)));
                knockback = knockback * (1f + (0.1f * RhythmStacks));
            }
        }
    }

    public class RhythmicResonatorUIState : SmartUIState
    {
        public override bool Visible => !Main.playerInventory && Main.LocalPlayer.GetModPlayer<RhythmicResonatorModPlayer>().equipped && ((!Main.player[Main.myPlayer].dead && !Main.player[Main.myPlayer].ghost && !Main.gameMenu) || !PlayerInput.InvisibleGamepadInMenus);

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(n => n.Name == "Vanilla: Mouse Text");

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUIBig").Value;
            Texture2D texOutline = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUIBig_Outline").Value;

            Texture2D texSmall = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUISmall").Value;
            Texture2D texSmallOutline = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUISmall_Outline").Value;

            Vector2 mouse = Main.MouseWorld;

            Color border = Main.MouseBorderColor;
            Color inside = Main.cursorColor;

            if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
                inside = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f, byte.MaxValue);

            var modPlayer = Main.LocalPlayer.GetModPlayer<RhythmicResonatorModPlayer>();

            if (modPlayer.flashTimer > 0)
            {
                Color color = Main.cursorColor;
                if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
                    color = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f, byte.MaxValue);

                border = Color.Lerp(Color.White, Main.MouseBorderColor, 1f - modPlayer.flashTimer / 20f);
                inside = Color.Lerp(Color.White, color, 1f - modPlayer.flashTimer / 20f);
            }

            if (modPlayer.RhythmTimer > 0)
            {
                float progress = 1f - modPlayer.RhythmTimer / (float)modPlayer.MaxRhythmTimer;
                float alpha = MathHelper.Lerp(0f, 1f, progress * 2);

                int start = Utils.Clamp(modPlayer.MaxRhythmTimer * 3, 30, 90);
                Vector2 offset = Vector2.Lerp(new Vector2(-start, 2), new Vector2(6, 2), 1f - modPlayer.RhythmTimer / (float)modPlayer.MaxRhythmTimer);
                Main.spriteBatch.Draw(texSmallOutline, mouse + (Vector2.One * 9) + offset - Main.screenPosition, null, border * alpha, 0f, texOutline.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(texSmall, mouse + (Vector2.One * 9) + offset - Main.screenPosition, null, inside * alpha, 0f, tex.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);

                offset = Vector2.Lerp(new Vector2(start + 46, 2), new Vector2(42, 2), 1f - modPlayer.RhythmTimer / (float)modPlayer.MaxRhythmTimer);
                Main.spriteBatch.Draw(texSmallOutline, mouse + (Vector2.One * 9) + offset - Main.screenPosition, null, border * alpha, 0f, texOutline.Size() / 2f, Main.cursorScale, SpriteEffects.FlipHorizontally, 0f);

                Main.spriteBatch.Draw(texSmall, mouse + (Vector2.One * 9) + offset - Main.screenPosition, null, inside * alpha, 0f, tex.Size() / 2f, Main.cursorScale, SpriteEffects.FlipHorizontally, 0f);
            }

            Main.spriteBatch.Draw(texOutline, mouse + (Vector2.One * 9) - Main.screenPosition, null, border, 0f, texOutline.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(tex, mouse + (Vector2.One * 9) - Main.screenPosition, null, inside, 0f, tex.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);

        }
    }
}
