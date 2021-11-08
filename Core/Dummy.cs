using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public abstract class Dummy : ModProjectile
    {
        protected readonly int ValidType;
        private readonly int Width;
        private readonly int Height;

        public Tile Parent => Main.tile[ParentX, ParentY];
        public virtual int ParentX => (int)projectile.Center.X / 16;
        public virtual int ParentY => (int)projectile.Center.Y / 16;

        public Dummy(int validType, int width, int height)
        {
            ValidType = validType;
            Width = width;
            Height = height;
        }

        public virtual bool ValidTile(Tile tile) => tile.type == ValidType;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override string Texture => AssetDirectory.Invisible;

        public virtual void Update() { }

        public virtual void Collision(Player player) { }

        public virtual void SafeSetDefaults() { }

        public sealed override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            projectile.width = Width;
            projectile.height = Height;
            projectile.hostile = true;
            projectile.damage = 1;
            projectile.timeLeft = 2;
        }

        public sealed override void AI()
        {
            if (ValidTile(Parent)) projectile.timeLeft = 2;
            Update();
        }

        public sealed override bool CanHitPlayer(Player target)
        {
            Collision(target);
            return false;
        }
    }
}
