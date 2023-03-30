using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles
{
	abstract class LootBubble : DummyTile
	{
		public virtual List<Loot> GoldLootPool => null;

		public virtual string BubbleTexture => "StarlightRiver/Assets/Tiles/Bubble";

		public override int DummyType => ProjectileType<LootBubbleDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public virtual void PickupEffects(Vector2 origin)
		{
			//Terraria.Audio.SoundEngine.PlaySound( , origin);

			for (int k = 0; k < 50; k++)
				Dust.NewDustPerfect(origin, DustType<Dusts.BlueStamina>(), Vector2.One.RotatedByRandom(3.14f) * Main.rand.NextFloat(4), 0, default, 0.5f);
		}

		public virtual bool CanOpen(Player Player)
		{
			return true;
		}

		public virtual void DrawBubble(Vector2 pos, SpriteBatch spriteBatch, float time)
		{
			int n = (int)(time % GoldLootPool.Count);
			Texture2D tex2 = Helper.GetItemTexture(GoldLootPool[n].type);
			var ItemTarget = new Rectangle((int)pos.X + 8, (int)pos.Y + 8, 16, 16);
			spriteBatch.Draw(tex2, ItemTarget, Color.White);

			Texture2D tex = Request<Texture2D>(BubbleTexture).Value;
			int sin = (int)(Math.Sin(time) * 4);
			int sin2 = (int)(Math.Sin(time + 0.75f) * 4);
			var bubbleTarget = new Rectangle((int)pos.X - sin / 2, (int)pos.Y + sin2 / 2, 32 + sin, 32 - sin2);
			spriteBatch.Draw(tex, bubbleTarget, Color.White);

			return;
		}
	}

	class LootBubbleDummy : Dummy, IDrawAdditive
	{
		public LootBubbleDummy() : base(0, 32, 32) { }

		public override bool ValidTile(Tile tile)
		{
			return GetModTile(Parent.TileType) is LootBubble;
		}

		public override void Collision(Player Player)
		{
			var bubble = GetModTile(Parent.TileType) as LootBubble;

			if (bubble.CanOpen(Player) && Player.Hitbox.Intersects(new Rectangle(ParentX * 16, ParentY * 16, 16, 16)))
			{
				Loot loot = bubble.GoldLootPool[Main.rand.Next(bubble.GoldLootPool.Count)];
				Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, loot.type, loot.GetCount());
				bubble.PickupEffects(Projectile.Center);

				WorldGen.KillTile(ParentX, ParentY);
				NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 2, TileChangeType.None);
			}
		}

		public override void Update()
		{
			Projectile.ai[0] += 0.1f;
		}

		public override void PostDraw(Color lightColor)
		{
			var bubble = GetModTile(Parent.TileType) as LootBubble;
			bubble.DrawBubble(Projectile.position - Main.screenPosition, Main.spriteBatch, Projectile.ai[0]);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			float sin = 0.5f + (float)(Math.Sin(StarlightWorld.visualTimer) * 0.5f);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.SkyBlue * (0.4f + sin * 0.1f), 0, tex.Size() / 2, 0.8f + sin * 0.1f, 0, 0);
		}
	}
}