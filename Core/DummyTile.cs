using Microsoft.Xna.Framework;
using StarlightRiver.Packets;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	internal abstract class DummyTile : ModTile
    {
        public virtual int DummyType { get; }

        public Projectile Dummy { get; set; }
        public virtual void PostSpawnDummy(Projectile dummy) { }

        public virtual void SafeNearbyEffects(int i, int j, bool closer) { }

        public virtual bool SpawnConditions(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.frameX == 0 && tile.frameY == 0;
        }

        public sealed override void NearbyEffects(int i, int j, bool closer)
        {
            if (!Main.tileFrameImportant[Type] || SpawnConditions(i, j))
            {
                int type = DummyType;//cache type here so you dont grab the it from a dict every single iteration

                if (!Main.projectile.Any(n => n.active && n.type == type && n.position == new Vector2(i, j) * 16))
                {
                    Projectile p = new Projectile();
                    p.SetDefaults(type);

                    int n = Projectile.NewProjectile(new Vector2(i, j) * 16 + p.Size / 2, Vector2.Zero, type, 1, 0);
                    Dummy = Main.projectile[n];

                    if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                    {
                        SpawnDummy packet = new SpawnDummy(Main.myPlayer, i * 16, j * 16, type);
                        packet.Send(-1, -1, false);
                    }

                    PostSpawnDummy(Dummy);
                    p = null;
                }
            }
            SafeNearbyEffects(i, j, closer);
        }
    }
}
