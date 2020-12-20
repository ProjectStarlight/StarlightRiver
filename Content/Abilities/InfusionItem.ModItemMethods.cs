using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Abilities
{
    public abstract partial class InfusionItem : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Draw(spriteBatch, position + GetTexture(Texture).Size() / 2, 1, true);
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float opacity, bool glow)
        {
            Texture2D outlineTex = GetTexture("StarlightRiver/Assets/Abilities/Tier" + (int)Tier);
            spriteBatch.Draw(outlineTex, position, null, Color.White * opacity, 0, outlineTex.Size() / 2, 1, 0, 0);
            Texture2D mainTex = GetTexture(Texture);
            spriteBatch.Draw(mainTex, position, null, Color.White * opacity, 0, mainTex.Size() / 2, 1, 0, 0);

            if (glow)
            {
                Texture2D glowTex = GetTexture("StarlightRiver/Assets/Abilities/InfusionGlow");
                Color color;

                if (AbilityType == null || Ability == null)
                    color = Color.Gray;
                else
                    color = Ability.Color;

                float sin = 0.75f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
                spriteBatch.Draw(glowTex, position, null, color * sin * opacity, 0, glowTex.Size() / 2, 1, 0, 0);
            }
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Color color;

            if (AbilityType == null) color = Color.Gray;
            else
            {
                if (!Main.LocalPlayer.GetHandler().GetAbility(AbilityType, out Ability ability)) return;
                color = ability.Color;
            }

            float rot = Main.rand.NextFloat((float)Math.PI * 2);
            Dust d = Dust.NewDustPerfect(item.Center + Vector2.One.RotatedBy(rot) * 16, 264, Vector2.One.RotatedBy(rot) * -1.25f, 0, color, 0.8f);
            d.noGravity = true;
            d.noLight = true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Draw(spriteBatch, item.Center - Main.screenPosition, 1, true);
            return false;
        }

        public override bool OnPickup(Player player)
        {
            Helper.UnlockEntry<Codex.Entries.InfusionEntry>(player);
            return true;
        }

        public override bool CanRightClick()
        {
            return Main.LocalPlayer.GetHandler().CanSetInfusion(this);
        }

        public override void RightClick(Player player)
        {
            var mp = player.GetHandler();

            for (int i = 0; i < mp.InfusionLimit; i++)
                if (mp.GetInfusion(i) == null || i == mp.InfusionLimit - 1)
                {
                    mp.SetInfusion(item.Clone().modItem as InfusionItem, i);
                    item.TurnToAir();
                    return;
                }
        }
    }
}
