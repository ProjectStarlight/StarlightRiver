using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles
{
    abstract class LootBubble : DummyTile
    {
        public virtual List<Loot> GoldLootPool => null;

        public virtual string BubbleTexture => "StarlightRiver/Assets/Tiles/Bubble";

        public virtual bool CanOpen(Player player) => true;

        public override int DummyType => ProjectileType<LootBubbleDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public virtual void PickupEffects(Vector2 origin)
        {
            //Main.PlaySound( , origin);

            for (int k = 0; k < 50; k++)
                Dust.NewDustPerfect(origin, DustType<Dusts.BlueStamina>(), Vector2.One.RotatedByRandom(3.14f) * Main.rand.NextFloat(4), 0, default, 0.5f);
        }

        public virtual void DrawBubble(Vector2 pos, SpriteBatch spriteBatch, float time)
        {
            int n = (int)(time % GoldLootPool.Count);
            Texture2D tex2 = Helper.GetItemTexture(GoldLootPool[n].Type);
            Rectangle itemTarget = new Rectangle((int)pos.X + 8, (int)pos.Y + 8, 16, 16);
            spriteBatch.Draw(tex2, itemTarget, Color.White);

            Texture2D tex = GetTexture(BubbleTexture);
            int sin = (int)(Math.Sin(time) * 4);
            int sin2 = (int)(Math.Sin(time + 0.75f) * 4);
            Rectangle bubbleTarget = new Rectangle((int)pos.X - sin / 2, (int)pos.Y + sin2 / 2, 32 + sin, 32 - sin2);
            spriteBatch.Draw(tex, bubbleTarget, Color.White);

            return;
        }
    }

    class LootBubbleDummy : Dummy, IDrawAdditive
    {
        public LootBubbleDummy() : base(0, 32, 32) { }

        public override bool ValidTile(Tile tile) => GetModTile(Parent.type) is LootBubble;

        public override void Collision(Player player)
        {
            LootBubble bubble = GetModTile(Parent.type) as LootBubble;

            if (bubble.CanOpen(player) && player.Hitbox.Intersects(new Rectangle(ParentX * 16, ParentY * 16, 16, 16)))
            {
                Loot loot = bubble.GoldLootPool[Main.rand.Next(bubble.GoldLootPool.Count)];
                Item.NewItem(projectile.Center, loot.Type, loot.GetCount());
                WorldGen.KillTile(ParentX, ParentY);

                bubble.PickupEffects(projectile.Center);
            }
        }

        public override void Update() => projectile.ai[0] += 0.1f;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            LootBubble bubble = GetModTile(Parent.type) as LootBubble;
            bubble.DrawBubble(projectile.position - Main.screenPosition, spriteBatch, projectile.ai[0]);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            float sin = 0.5f + (float)(Math.Sin(StarlightWorld.rottime) * 0.5f);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.SkyBlue * (0.4f + sin * 0.1f), 0, tex.Size() / 2, 0.8f + sin * 0.1f, 0, 0);
        }
    }
}
