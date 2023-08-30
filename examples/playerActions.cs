using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using static SHVDN.NativeMemory;

namespace GTA
{
	internal class PlayerActions: Script
	{
		public PlayerActions()
		{
			KeyDown += PlayerActions_KeyDown;
		}

		private void PlayerActions_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			/**
			if (e.KeyCode == Keys.T)
			{
				getOnNearbyVehicle();
			}

			if (e.KeyCode == Keys.M)
			{
				driveVehicleForward();
			}

			if (e.KeyCode == Keys.F10)
			{
				showPlayerPos();
			}
			*/
		}
		private void walkToModel(int model)
		{
			var entities = World.GetAllEntities();
			foreach (var entity in entities)
			{
				if (entity.Model.Hash == model)
				{
					var vehicle = Game.Player.Character.CurrentVehicle;
					if (vehicle != null)
					{
						Game.Player.Character.Task.DriveTo(vehicle, entity.Position, 10, VehicleDrivingFlags.None, 10);
					}
					else
					{
						Game.Player.Character.Task.GoTo(entity);
					}

					break;
				}
			}
		}
		private void walkTo(int hash)
		{
			var entities = World.GetAllEntities();
			foreach (var entity in entities)
			{
				if (entity.GetHashCode() == hash)
				{
					var vehicle = Game.Player.Character.CurrentVehicle;
					if (vehicle != null)
					{
						Game.Player.Character.Task.DriveTo(vehicle, entity.Position, 10, VehicleDrivingFlags.None, 10);
					}
					else
					{
						Game.Player.Character.Task.GoTo(entity);
					}

					break;
				}
			}
		}
		private bool didHurtAnyOne()
		{
			// there is no method found to get hurt record on others, so get ped and then check them
			var nearbyPeds = World.GetNearbyPeds(Game.Player.Character, 10);
			foreach (var ped in nearbyPeds)
			{
				if (ped.Health != ped.MaxHealth)
				{
					// got hurt
					return true;
				}
			}
			return false;
		}

		//run to an object
		private void runTo(int hash)
		{
			var entities = World.GetAllEntities();
			foreach (var entity in entities)
			{
				if (entity.GetHashCode() == hash)
				{
					Game.Player.Character.Task.RunTo(entity.Position);
					break;
				}
			}
		}
		/**
		 * swim is not supported now
		public void swimTo(int hash)
		{
			Ped player = Game.Player.Character;
			if (!player.IsSwimming && player.IsInWater)
			{
				//player.Task.Swim(); swim task doesn't exist.
			}
		}
		*/

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


		private void getOnNearbyVehicle()
		{
			Vehicle closest = World.GetClosestVehicle(Game.Player.Character.Position, 5);
			if (closest != null && closest.IsDriveable)
			{
				Game.Player.Character.Task.EnterVehicle(closest, GTA.VehicleSeat.Driver);
			}
		}

		//get off vehicle
		private void getOffVehicle()
		{
			Ped player = Game.Player.Character;
			Vehicle curVehicle = player.CurrentVehicle;
			if (curVehicle != null && player.IsInVehicle())
			{
				player.Task.WarpOutOfVehicle(curVehicle);
			}
		}

		private void driveForward(float add_v)
		{
			Vehicle now = Game.Player.Character.CurrentVehicle;
			if (now == null)
			{
				return;
			}
			now.Velocity += now.ForwardVector.Normalized * add_v;
		}

		//player sound horn in car (not working)
		private void pressHorn()
		{
			if (Game.Player.Character.IsInVehicle())
			{
				Vehicle vehicle = Game.Player.Character.CurrentVehicle;
				if (Game.IsControlPressed(Control.VehicleHorn))
				{
					// Play a horn sound (optional)
					vehicle.SoundHorn(2000);
				}
			}
		}
		private void rotate(Vector3 v)
		{
			Vehicle now = Game.Player.Character.CurrentVehicle;
			if (now == null)
			{
				return;
			}
			now.Rotation += v;
		}
		private void showPlayerPos()
		{
			Vector3 playerPosition = Game.Player.Character.Position;
			GTA.UI.Screen.ShowSubtitle($"Player Position: {playerPosition.X}, {playerPosition.Y}, {playerPosition.Z}");
		}


		//jump up the object in front of the player
		private void climbUp()
		{
			Ped player = Game.Player.Character;
			player.Task.Climb();
		}

		//climb up the ladder
		private void climbLadder()
		{
			Ped player = Game.Player.Character;
			player.Task.ClimbLadder();
		}

		//talk to someone (no real conversations, just gestures acting like the player is talking)
		private void talkTo(int target_ped_hash)
		{
			Ped target_ped = null;
			foreach (Ped ped in World.GetAllPeds())
			{
				if (ped.Model.Hash == target_ped_hash)
				{
					target_ped = ped;
					break;
				}
			}

			if (target_ped != null && Game.Player.Character.Position.DistanceTo(target_ped.Position) < 5)
			{
				Ped player = Game.Player.Character;
				player.Task.ChatTo(target_ped);
			}
			
		}

		private void talkToClosestPed()
		{
			Ped target_ped = World.GetClosestPed(Game.Player.Character.Position, 50.0f);

			if (target_ped != null)
			{
				Ped player = Game.Player.Character;
				player.Task.ChatTo(target_ped);
			}
		}

		//combat with someone
		private void combat(int target_ped_hash)
		{
			Ped target_ped = null;
			foreach (Ped ped in World.GetAllPeds())
			{
				if (ped.Model.Hash == target_ped_hash)
				{
					target_ped = ped;
					break;
				}
			}

			if (target_ped != null && Game.Player.Character.Position.DistanceTo(target_ped.Position) < 5)
			{
				Ped player = Game.Player.Character;
				player.Task.Combat(target_ped);
			}
		}

		//aim gun at an object
		private void aimGunAt(int hash)
		{
			var entities = World.GetAllEntities();
			foreach (var entity in entities)
			{
				if (entity.GetHashCode() == hash)
				{
					if (!Game.Player.Character.IsAiming)
					{
						Game.Player.Character.Task.AimGunAtEntity(entity, 5000);
					}
				}
			}
		}

	}
}
