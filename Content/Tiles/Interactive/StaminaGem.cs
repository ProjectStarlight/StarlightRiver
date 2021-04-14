using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.Tiles.Interactive
{
    internal class StaminaGem : DummyTile
    {
        public override int DummyType => ProjectileType<StaminaGemDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.InteractiveTile + name;
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Content.Dusts.Stamina>(), SoundID.Shatter, false, new Color(255, 186, 66));

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<StaminaGemItem>());

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.236f * 1.1f;
            g = 0.144f * 1.1f;
            b = 0.071f * 1.1f;
        }
    }
    internal class StaminaGemItem : QuickTileItem 
    { 
        public StaminaGemItem() : base("Stamina Gem", "Restores stamina when hit with an ability", TileType<StaminaGem>(), 8, AssetDirectory.InteractiveTile) { } 
    }

    internal class StaminaGemDummy : Dummy
    {
        public StaminaGemDummy() : base(TileType<StaminaGem>(), 16, 16) { }

        public override void Update()
        {
            if (projectile.ai[0] > 0) projectile.ai[0]--; else if (Main.rand.Next(3) == 0) Dust.NewDust(projectile.position, 16, 16, DustType<Dusts.Stamina>());

            Lighting.AddLight(projectile.Center, new Vector3(1, 0.4f, 0.1f) * 0.35f);
        }

        public override void Collision(Player player)
        {
            AbilityHandler mp = player.GetHandler();

            if (projectile.ai[0] == 0 && projectile.Hitbox.Intersects(player.Hitbox) && mp.Stamina < mp.StaminaMax && mp.ActiveAbility != null)
            {
                mp.Stamina++;
                projectile.ai[0] = 300;

                Main.PlaySound(SoundID.Shatter, projectile.Center);
                Main.PlaySound(SoundID.Item112, projectile.Center);
                CombatText.NewText(player.Hitbox, new Color(255, 170, 60), "+1");
                for (float k = 0; k <= 6.28; k += 0.1f)
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(k), (float)Math.Sin(k)) * (Main.rand.Next(50) * 0.1f), 0, default, 3f);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[0] == 0)
            {
                Color color = Color.White * (float)Math.Sin(StarlightWorld.rottime * 3f);
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Interactive/StaminaGemGlow"), projectile.position - Main.screenPosition, color);
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Interactive/StaminaGemOn"), projectile.position - Main.screenPosition, Color.White);
            }
        }
    }
}