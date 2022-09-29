﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public abstract class Dummy : ModProjectile
    {
        protected int ValidType;
        private int Width;
        private int Height;

        public Tile Parent => Main.tile[ParentX, ParentY];

        public virtual int ParentX => (int)Projectile.Center.X / 16;
        public virtual int ParentY => (int)Projectile.Center.Y / 16;

        public Dummy(int validType, int width, int height)
        {
            ValidType = validType;
            Width = width;
            Height = height;
        }

        public virtual bool ValidTile(Tile tile) => (tile.TileType == ValidType && tile.HasTile); //the tile is null only where tiles are unloaded in multiPlayer. We don't want to kill off dummies on unloaded tiles until tile is known because Projectile is recieved MUCH farther than the tiles.

        public override bool PreDraw(ref Color lightColor) => false;

        public override string Texture => AssetDirectory.Invisible;

        public virtual void Update() { }

        public virtual void Collision(Player Player) { }

        public virtual void SafeSetDefaults() { }

        public virtual void SafeSendExtraAI(BinaryWriter writer) { }

        public virtual void SafeReceiveExtraAI(BinaryReader reader) { }

        public sealed override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public sealed override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ValidType);
            writer.Write(Width);
            writer.Write(Height);

            SafeSendExtraAI(writer);
        }

        public sealed override void ReceiveExtraAI(BinaryReader reader)
        {
            ValidType = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();

            Point16 key = new Point16(ParentX, ParentY);
            DummyTile.dummies[key] = Projectile;

            SafeReceiveExtraAI(reader);
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            Projectile.width = Width;
            Projectile.height = Height;
            Projectile.hostile = true;
            Projectile.damage = 1;
            Projectile.timeLeft = 2;
            Projectile.netImportant = true;
        }

        public sealed override void AI()
        {
            if (ValidTile(Parent))
                Projectile.timeLeft = 2;
                
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //in single Player we can use the CanHitPlayer, but in MP that is only run by the server so we need to check Players manually for clients
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player Player = Main.player[i];
                    if (Player.Hitbox.Intersects(Projectile.Hitbox))
                        Collision(Player);
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
