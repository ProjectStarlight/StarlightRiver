using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    public abstract class InfusionItem : ModItem
    {
        public virtual Type AbilityType { get; }
        public Player Player { get; internal set; } // TODO sync this somehow
        public Ability Ability
        {
            get
            {
                if (AbilityType == null) return null;
                if (ability == null) Player.GetHandler().GetAbility(AbilityType, out ability);
                return ability;
            }
        }
        private Ability ability;

        public virtual void OnActivate() => Ability?.OnActivate();
        public virtual void UpdateActive() => Ability?.UpdateActive();
        public virtual void UpdateActiveEffects() => Ability?.UpdateActiveEffects();
        public virtual void UpdateFixed() => Ability?.UpdateFixed();
        public virtual void OnExit() => Ability?.OnExit();

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = GetTexture("StarlightRiver/Abilities/Infusion/InfusionGlow");
            Texture2D tex2 = GetTexture(Texture);

            Color color;

            if (AbilityType == null) color = Main.DiscoColor;
            else
            {
                if (!Main.LocalPlayer.GetHandler().GetAbility(AbilityType, out Ability ability)) return false;
                color = ability.Color;
            }

            float sin = 0.75f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
            Vector2 pos = position + tex2.Size() / 2 - Vector2.One;

            spriteBatch.Draw(tex, pos, null, color * sin, 0, tex.Size() / 2, 1, 0, 0);

            spriteBatch.Draw(tex2, pos, null, Color.White, 0, tex2.Size() / 2, 1, 0, 0);
            return false;
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Color color;

            if (AbilityType == null) color = Main.DiscoColor;
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
            Texture2D tex = GetTexture("StarlightRiver/Abilities/Infusion/InfusionGlow");
            Texture2D tex2 = GetTexture(Texture);

            Color color;

            if (AbilityType == null) color = Main.DiscoColor;
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
    }

    public abstract class InfusionItem<T> : InfusionItem where T : Ability
    {
        public override Type AbilityType => typeof(T);
        public new T Ability => (T)base.Ability;
    }
}
