using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Misc
{
	internal class JadeStopwatch : SmartAccessory
	{
		public int slowTime;
		public int flashTime;

		public string feat;
		public string time;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public JadeStopwatch() : base("Jade stopwatch", "Time moves more quickly for you\n" +
			"Time moves more slowly for you briefly after being hit\n" +
			"50% reduced damage")
		{ }

		public override void Load()
		{
			Terraria.On_Player.Update += Speedup;
			StarlightPlayer.OnHitByNPCEvent += DamageEffect;
			StarlightPlayer.OnHitByProjectileEvent += ProjDamageEffect;
			StarlightPlayer.PreDrawEvent += DrawClock;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			int sin = (int)(Math.Sin(Main.GameUpdateCount * 0.1f) * 50);
			tooltips.FirstOrDefault(n => n.Name == "ItemName").OverrideColor = new Color(50 + sin, 255, 150 + sin);

			var featLine = new TooltipLine(Mod, "StopwatchMark", $"Obtained by beating {feat} in {time}")
			{
				OverrideColor = new Color(250 + sin, 200 + sin, 100 + sin)
			};

			tooltips.Add(featLine);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			if (slowTime > 0)
				slowTime--;

			if (slowTime == 1)
				flashTime = 20;

			if (flashTime > 0)
				flashTime--;

			Player.GetDamage(DamageClass.Generic) -= 0.5f;

			if (slowTime <= 0)
			{
				float rot = Main.rand.NextFloat(6.28f);
				var d = Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.GlowFollowPlayer>(), Vector2.One.RotatedBy(rot + 1.5f) * 1.5f, 0, new Color(50, 255, 150) * 0.5f, 0.25f);
				d.customData = new object[] { Player, Vector2.One.RotatedBy(rot) * 36 };
			}
			else
			{
				float rot = Main.rand.NextFloat(6.28f);
				var d = Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.GlowFollowPlayer>(), Vector2.One.RotatedBy(rot + 1.5f) * 0.5f, 0, Color.Red * 0.5f, 0.25f);
				d.customData = new object[] { Player, Vector2.One.RotatedBy(rot) * 36 };
			}
		}

		public override void OnEquip(Player player, Item item)
		{
			var instance = GetEquippedInstance(player) as JadeStopwatch;
			instance.flashTime = 20;
		}

		private void ProjDamageEffect(Player player, Projectile projectile, Player.HurtInfo info)
		{
			if (Equipped(player))
			{
				var instance = GetEquippedInstance(player) as JadeStopwatch;

				if (instance.slowTime <= 0)
				{
					instance.slowTime = 60;
					(GetEquippedInstance(player) as JadeStopwatch).flashTime = 20;
				}
			}
		}

		private void DamageEffect(Player player, NPC npc, Player.HurtInfo info)
		{
			if (Equipped(player))
			{
				var instance = GetEquippedInstance(player) as JadeStopwatch;

				if (instance.slowTime <= 0)
				{
					instance.slowTime = 60;
					(GetEquippedInstance(player) as JadeStopwatch).flashTime = 20;
				}
			}
		}

		private void Speedup(Terraria.On_Player.orig_Update orig, Player self, int i)
		{
			if (Equipped(self) && Main.GameUpdateCount % 2 == 0) //every other frame, 
			{
				var instance = GetEquippedInstance(self) as JadeStopwatch;

				if (instance.slowTime <= 0) //if speedup, run update again
					orig(self, i);
				else //if slowdown, dont run update
					return;
			}

			orig(self, i);
		}

		private void DrawClock(Player player, SpriteBatch spriteBatch)
		{
			if (Equipped(player))
			{
				var instance = GetEquippedInstance(player) as JadeStopwatch;

				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/Clock").Value;

				float alpha = 1 + instance.flashTime / 20f * 2;

				Color color = instance.slowTime > 0 ? new Color(255, 50, 50) : new Color(50, 255, 150);
				color.A = 0;
				color *= alpha;

				float speed = instance.slowTime > 0 ? 0.5f : 1.5f;

				Vector2 pos = player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition;

				spriteBatch.Draw(tex, pos, null, color * 0.3f * alpha, 0, tex.Size() / 2, 0.6f, 0, 0);

				Texture2D armTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailOneEnd").Value;

				var target = new Rectangle((int)pos.X, (int)pos.Y, 40, 12);
				spriteBatch.Draw(armTex, target, null, color * 0.5f * alpha, Main.GameUpdateCount * 0.12f * speed, new Vector2(0, armTex.Height / 2), 0, 0);

				var target2 = new Rectangle((int)pos.X, (int)pos.Y, 34, 12);
				spriteBatch.Draw(armTex, target2, null, color * 0.5f * alpha, Main.GameUpdateCount * 0.01f * speed, new Vector2(0, armTex.Height / 2), 0, 0);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["feat"] = feat;
			tag["time"] = time;
		}

		public override void LoadData(TagCompound tag)
		{
			feat = tag.GetString("feat");
			time = tag.GetString("time");
		}
	}
}
