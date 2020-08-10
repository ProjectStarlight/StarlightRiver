using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.Dummies
{
    internal abstract class Dummy : ModProjectile
    {
        private readonly int ValidType;
        private readonly int Width;
        private readonly int Height;
        public Tile Parent { get => Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16]; }
        public int ParentX { get => (int)projectile.Center.X / 16; }
        public int ParentY { get => (int)projectile.Center.Y / 16; }

        public Dummy(int validType, int width, int height)
        {
            ValidType = validType;
            Width = width;
            Height = height;
        }

        public override string Texture => "StarlightRiver/Invisible";

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
            if (Parent.type == ValidType) projectile.timeLeft = 2;
            Update();
        }

        public sealed override bool CanHitPlayer(Player target)
        {
            Collision(target);
            return false;
        }
    }
}
