using StarlightRiver.Core.DrawingRigs;
using System.IO;
using System.Text.Json;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class Revenant : ModNPC
	{
		private static StaticRig rig;

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			Stream stream = StarlightRiver.Instance.GetFileStream("Assets/NPCs/Crimson/RevenantRig.json");
			rig = JsonSerializer.Deserialize<StaticRig>(stream);
			stream.Close();
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 500;
			NPC.width = 64;
			NPC.height = 128;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
		}

		public override void AI()
		{
			base.AI();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.NPCs.Crimson.Revenant.Value;

			foreach (StaticRigPoint point in rig.Points)
			{
				Vector2 targetPos = NPC.position + point.Pos - Main.screenPosition;
				var frame = new Rectangle(point.Frame * 42, 0, 42, 54);

				spriteBatch.Draw(tex, targetPos, frame, drawColor, 0f, Vector2.Zero, 1f, 0, 0);
			}

			return false;
		}
	}
}
