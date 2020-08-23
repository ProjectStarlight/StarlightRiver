using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    public abstract partial class InfusionItem : ModItem
    {
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            DrawInventory(spriteBatch, position, 1);
            return false;
        }

        public void DrawInventory(SpriteBatch spriteBatch, Vector2 position, float opacity)
        {
            Texture2D tex = GetTexture("StarlightRiver/Abilities/Content/Infusions/InfusionGlow");
            Texture2D tex2 = GetTexture(Texture);

            Color color;

            if (AbilityType == null) color = Color.Gray;
            else
            {
                if (Main.LocalPlayer.GetHandler().GetAbility(AbilityType, out Ability ability))
                    color = ability.Color;
                else
                    return;
            }

            float sin = 0.75f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
            Vector2 pos = position + tex2.Size() / 2 - Vector2.One;

            spriteBatch.Draw(tex, pos, null, color * sin * opacity, 0, tex.Size() / 2, 1, 0, 0);

            spriteBatch.Draw(tex2, pos, null, Color.White * opacity, 0, tex2.Size() / 2, 1, 0, 0);
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
            Texture2D tex = GetTexture("StarlightRiver/Abilities/Content/Infusions/InfusionGlow");
            Texture2D tex2 = GetTexture(Texture);

            Color color;

            if (AbilityType == null) color = Color.Gray;
            else
            {
                if (!Main.LocalPlayer.GetHandler().GetAbility(AbilityType, out Ability ability)) return false;
                color = ability.Color;
            }

            float sin = 0.75f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
            Vector2 pos = item.Center - Main.screenPosition;

            spriteBatch.Draw(tex, pos, null, color * sin, 0, tex.Size() / 2, 1, 0, 0);

            spriteBatch.Draw(tex2, pos, null, Color.White, 0, tex2.Size() / 2, 1, 0, 0);
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
            {
                if (mp.GetInfusion(i) == null || i == mp.InfusionLimit - 1)
                {
                    mp.SetInfusion(item.Clone().modItem as InfusionItem, i);
                    item.TurnToAir();
                    return;
                }
            }
        }
    }
}
