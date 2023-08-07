using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA
{
	internal class mission01: Script
	{
		private Blip MissionBlip;
		private Vehicle Target;
		private int stage = 0;
		private void OnTick(object sender, EventArgs e)
		{
			if (stage == 0)
			{
				if (Target.Position.DistanceTo(Game.Player.Character.Position) < 2)
				{
					GTA.UI.Notification.Show("Drive the car!");
					stage = 1;
				}
			}
			if (stage == 1)
			{
				if (Target.IsAttachedTo(Game.Player.Character))
				{
					GTA.UI.Notification.Show("Successfully Drive The Car");
				}
			}
		}
		private void OnShutdown(object sender, EventArgs e)
		{
			if (Target != null && Target.Exists())
			{
				// Destroy the blip if it exists
				if (Target.AttachedBlip != null && Target.AttachedBlip.Exists())
				{
					Target.AttachedBlip.Delete();
				}

				// Then, destroy the vehicle
				Target.Delete();
			}
			Target?.Delete();

		}
			public mission01()
		{
			Tick += OnTick;
			Aborted += OnShutdown;
			Model Banshee = new Model(VehicleHash.Banshee2);
			Banshee.Request();
			while (!Banshee.IsLoaded)
			{
				Yield();
			}
			Target = World.CreateVehicle(Banshee, Game.LocalPlayerPed.Position + (GTA.Math.Vector3.RelativeFront * 3), 268.2f);
			MissionBlip = World.CreateBlip(Target.Position);
			MissionBlip.Sprite = BlipSprite.Lester;
			MissionBlip.Color = BlipColor.Yellow;
			MissionBlip.Name = "Placeholder (LMP)";
			Target.AddBlip();
			Target.AttachedBlip.Sprite = BlipSprite.GetawayCar;
			Target.AttachedBlip.IsShortRange = false;
			Target.AttachedBlip.Color = BlipColor.Green;
		}

	}
}
