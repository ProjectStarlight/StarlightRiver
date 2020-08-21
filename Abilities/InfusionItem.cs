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
        public abstract Type AbilityType { get; }
        public Ability Ability { get; internal set; }

        public virtual void UpdateActive() { }
        public virtual void UpdateFixed() { }
        public virtual void OnExit() { }
        public virtual void OnActivate() { }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = GetTexture("StarlightRiver/Abilities/Infusion/InfusionGlow");
            Texture2D tex2 = GetTexture(Texture);

            Ability ability;
            Color color;

            if (AbilityType == null) color = Color.White;
            else
            {
                if (!Main.LocalPlayer.GetHandler().GetAbility(AbilityType, out ability)) return false;
                color = ability.Color;
            }

            float sin = 0.75f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
            Vector2 pos = position + tex2.Size() / 2 - Vector2.One;

            spriteBatch.Draw(tex, pos, null, color * sin, 0, tex.Size() / 2, 1, 0, 0);

            spriteBatch.Draw(tex2, pos, null, color, 0, tex2.Size() / 2, 1, 0, 0);
            return false;
        }
    }

    public abstract class InfusionItem<T> : InfusionItem where T : Ability
    {
        public override Type AbilityType => typeof(T);
        public new T Ability => (T)base.Ability;
    }
}
