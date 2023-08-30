using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Math;

namespace GTA
{
	internal class mission01: mission
	{
		private Blip MissionBlip;
		private Vehicle Target;
		private int stage = 0;
		private Vector3 end = new Vector3(1, 1, 1);
		private Ped ped;
		private void initActions()
		{
			
		}
		private void walkTo(int hash)
		{
			var entities = World.GetAllEntities();
			foreach(var entity in entities)
			{
				if (entity.GetHashCode() == hash)
				{
					var vehicle = Game.Player.Character.CurrentVehicle;
					if (vehicle != null)
					{
						Game.Player.Character.Task.DriveTo(vehicle, entity.Position, 10,VehicleDrivingFlags.None,10);
					} else
					{
						Game.Player.Character.Task.GoTo(entity);
					}
					
					break;
				}
			}
		}
		private void driveVehicleForward()
		{
			var vehicle = Game.Player.Character.CurrentVehicle;
			if (vehicle != null)
			{
			}
		}
		private void getOnNearByVehicle()
		{
			Game.Player.Character.Task.EnterAnyVehicle();
		}
		public override void load()
		{
			if (ped == null)
			{
				ped = World.CreatePed(PedHash.Beach01AMY, Game.LocalPlayerPed.Position + (GTA.Math.Vector3.RelativeFront * 3));
			}
			hitted = false;
			GTA.UI.Notification.Show("shoot Beach01AMY!");
		}
		public override void destroy()
		{
			if (ped != null)
			{
				ped.Delete();
			}

		}
		private void initActions()
		{
			
		}

		//player move to object with hash
		
		private bool is_hit_end()
		{
			return Game.Player.Character.Position.DistanceTo(end) < 5;
		}
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
		
		private bool hitted = false;
		public void shoot()
		{
			// use ray to get the object
			// Game.Player.Character.Task.VehicleShootAtPed();
			var last = Game.Player.Character.DamageRecords.Last<EntityDamageRecord>();
			if (last != null && last.Victim.EntityType == EntityType.Ped && last.Victim == ped)
			{
				hitted = true;
			}
		}
		public override bool is_mission_finished()
		{
			return hitted;
		}
	}
}
// task list:

// 1. shoot the ped with cloth in specific color
// 2. move to a car and then drive it to a position
// 3. track a ped
// 4. 

// use lua script to communicate with lm
