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
        public virtual void OnEnd() { }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = GetTexture(Texture);
            float sin = (float)Math.Sin(StarlightWorld.rottime) * 0.1f;
            spriteBatch.Draw(tex, position, frame, Color.White * (0.5f + sin), 0, tex.Size() / 2, 1 + sin, 0, 0);
            return true;
        }
    }

    public abstract class InfusionItem<T> : InfusionItem where T : Ability
    {
        public override Type AbilityType => typeof(T);
        public new T Ability => (T)base.Ability;
    }
}
