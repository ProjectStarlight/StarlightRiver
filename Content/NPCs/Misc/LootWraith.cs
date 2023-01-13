using System;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Misc
{
	internal class LootWraith : ModNPC
	{
		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		public int xTile = 0;
		public int yTile = 0;

		public int chestFrame = 0;

		private Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.MiscNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Loot Wraith");
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 62;
			NPC.height = 46;
			NPC.damage = 0;
			NPC.defense = 5;
			NPC.lifeMax = 150;
			NPC.value = 100f;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.Grass;
			NPC.DeathSound = SoundID.Grass;
		}

		public override bool NeedSaving()
		{
			return true;
		}

		public override void LoadData(TagCompound tag)
		{
			xTile = tag.GetInt("xTile");
			yTile = tag.GetInt("yTile");
			chestFrame = tag.GetInt("chestFrame");
		}

		public override void SaveData(TagCompound tag)
		{
			tag["xTile"] = xTile;
			tag["yTile"] = yTile;
			tag["chestFrame"] = chestFrame;
		}

		public override void AI()
		{

		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			Main.NewText(chestFrame);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Request<Texture2D>(Texture).Value;

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(NPC.width / 2, NPC.height / 2);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			var slopeOffset = new Vector2(0, NPC.gfxOffY);
			Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
		}
	}
}