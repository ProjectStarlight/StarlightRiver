using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Helpers;

using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Items.Herbology.Materials;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Foregrounds;

namespace StarlightRiver.Content.NPCs.Corruption
{
	class Dweller : ModNPC
	{
		Tile root = null;

		public ref float State => ref npc.ai[0];
		public ref float Timer => ref npc.ai[1];
		public ref float Variant => ref npc.ai[2];
		public ref float Height => ref npc.ai[3];

		public Player Target => Main.player[npc.target];

		public enum States
		{
			Idle,
			Transforming,
			Attacking
		};

		public override string Texture => "StarlightRiver/Assets/NPCs/Corruption/Dweller";

		public override void SetDefaults()
		{
			Variant = Main.rand.Next(3);

			if (Variant == 0) npc.width = 66;
			if (Variant == 1) npc.width = 58;
			if (Variant == 2) npc.width = 64;

			npc.height = npc.width;

			npc.lifeMax = 120;
			npc.defense = 6;
			npc.knockBackResist = 0;
			npc.aiStyle = -1;
			npc.noGravity = true;
			npc.behindTiles = true;
		}

		public override void AI()
		{
			if(root is null) //scan to find a valid root tile under where it spawned
			{
				npc.Center = npc.Center - new Vector2(npc.Center.X % 16, npc.Center.Y % 16);

				for(int k = 0; root is null; k++)
				{
					var tile = Framing.GetTileSafely((int)npc.Center.X / 16, (int)npc.Center.Y / 16 + k);

					if(tile is null) //failsafe
					{
						npc.active = false;
						return;
					}

					if (tile.active() && tile.collisionType == 1)
					{
						root = tile;
						Height = k * 16;

						Main.NewText(tile.type);
					}
				}
			}

			if(!root.active()) //should automatically activate if the tile under it is killed
			{
				State = (int)States.Transforming;
				Timer = 0;
			}

			Timer++;

			switch(State)
			{
				case (int)States.Idle:

					//clientside vignette effect
					var distance = Vector2.Distance(Main.LocalPlayer.Center, npc.Center + Vector2.UnitY * Height);

					if (distance < 500)
					{
						Vignette.visible = true;
						Vignette.offset = Vector2.Zero;
						Vignette.extraOpacity = 1 - distance / 500f;
					}
					else
						Vignette.visible = false;

					npc.TargetClosest();

					var yOff = Target.Center.Y - npc.Center.Y;

					if (Timer > 60 && Math.Abs(Target.Center.X - npc.Center.X) < 32 && yOff < (Height - 16) && yOff > 0) //under the tree
					{
						State = (int)States.Transforming;
						Timer = 0;
					}

					break;

				case (int)States.Transforming:

					if(Timer == 100)
					{
						Vignette.visible = false;
						npc.noGravity = false;

						Main.PlaySound(SoundID.Grass, npc.Center);
					}

					if(Timer > 60)
					{
						for (int k = 0; k < 5; k++)
						{
							Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(npc.width / 2),
								DustType<Dusts.GreyLeaf>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, new Color(210, 200, 255), Main.rand.NextFloat(1.0f, 1.3f));
						}
					}

					if(Timer >= 100)
					{
						if (npc.velocity.Y == 0)
						{
							State = (int)States.Attacking;
							npc.knockBackResist = 1.2f;
							npc.damage = 20;
						}
					}

					break;

				case (int)States.Attacking:

					npc.TargetClosest();

					npc.velocity.X += Target.Center.X > npc.Center.X ? 0.1f : -0.1f;

					if (npc.velocity.X > 3) npc.velocity.X = 2.9f;
					if (npc.velocity.X < -3) npc.velocity.X = -2.9f;

					npc.rotation += npc.velocity.X / (npc.width / 2f);

					break;
			}

		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			Random rand = new Random(npc.GetHashCode());

			switch(State)
			{
				case (int)States.Transforming: //fall-through moment
				case (int)States.Idle:

					var barkTex = GetTexture("Terraria/Tiles_5_0"); //corruption tree bark 

					for (int k = 0; k < Height; k += 16)
					{
						Vector2 pos = npc.Center + Vector2.UnitY * k;
						pos -= new Vector2(pos.X % 16, pos.Y % 16);

						if (State == (int)States.Transforming && Timer > 60)
						{
							pos.Y += (Timer - 60) / 30f * Height;

							if (pos.Y > npc.Center.Y + Height)
								continue;
						}

						Rectangle source = new Rectangle(rand.Next(2) * 22, rand.Next(6) * 22, 20, 16);
						Color color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);

						spriteBatch.Draw(barkTex, pos - Main.screenPosition, source, color);

						if(rand.Next(6) == 0 && k > 48 && k < Height - 48)
						{
							var branchTex = GetTexture(Texture + "Branches");

							bool right = rand.Next(2) == 0;
							Vector2 branchPos = pos + new Vector2(right ? 16 : -branchTex.Width / 2 + 6, -16);
							Rectangle branchSource = new Rectangle(right ? branchTex.Width / 2 : 0, rand.Next(3) * branchTex.Height / 3, 42, 42);
							Color branchColor = Lighting.GetColor((int)branchPos.X / 16, (int)branchPos.Y / 16);

							spriteBatch.Draw(branchTex, branchPos - Main.screenPosition, branchSource, branchColor);
						}

						if(k == Height - 16)
						{
							bool rightRoot = rand.Next(2) == 0;
							spriteBatch.Draw(barkTex, pos - Main.screenPosition + Vector2.UnitX * (rightRoot ? 14 : -14), new Rectangle(rightRoot ? 22 : 44, 6 * 22 + rand.Next(3) * 22, 22, 22), color);
						}
					}

					var topperTex = GetTexture(Texture + "Tops");

					Vector2 topperPos = npc.Center - new Vector2(npc.Center.X % 16, npc.Center.Y % 16) + new Vector2(11, 8);
					Rectangle topperSource = new Rectangle((int)Variant * 82, 0, 82, 82);

					if (State == (int)States.Transforming)
						topperSource.Y = (int)(Timer < 30 ? Timer / 30 * 5 : Timer % 10 < 5 ? 3 : 4) * 84;

					spriteBatch.Draw(topperTex, topperPos - Main.screenPosition, topperSource, drawColor, 0, new Vector2(41, 41), 1, 0, 0);

					break;

				case (int)States.Attacking:

					npc.frame = new Rectangle(0, (int)Timer / 7 % 3 * npc.height, npc.width, npc.height);

					spriteBatch.Draw(GetTexture(Texture + Variant), npc.Center - Main.screenPosition, npc.frame, drawColor, npc.rotation, npc.Size / 2, npc.scale, 0, 0);

					break;
			}

			return false;
		}
	}
}
