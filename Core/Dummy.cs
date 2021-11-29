using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public abstract class Dummy : ModProjectile
    {
        protected int ValidType;
        private int Width;
        private int Height;

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

        public virtual void SafeSendExtraAI(BinaryWriter writer) { }

        public virtual void SafeReceiveExtraAI(BinaryReader reader) { }

        public sealed override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ValidType);
            writer.Write(Width);
            writer.Write(Height);

            SafeSendExtraAI(writer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ValidType = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();

            SafeReceiveExtraAI(reader);
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            projectile.width = Width;
            projectile.height = Height;
            projectile.hostile = true;
            projectile.damage = 1;
            projectile.timeLeft = 2;
            projectile.netImportant = true;
        }

        public sealed override void AI()
        {
            if (ValidTile(Parent))
                projectile.timeLeft = 2;

            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            {
                //in single player we can use the CanHitPlayer, but in MP that is only run by the server so we need to check players manually for clients
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.Hitbox.Intersects(projectile.Hitbox))
                        Collision(player);
                }
            }

            Update();
        }

        public sealed override bool CanHitPlayer(Player target)
        {
            Collision(target);
            return false;
        }
    }
}
