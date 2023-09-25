using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using SHVDN;
using static SHVDN.NativeMemory;

namespace GTA
{
	internal class PlayerActions: Script
	{
		public PlayerActions()
		{
			KeyDown += PlayerActions_KeyDown;
		}

		public void PlayerActions_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{

			if (e.KeyCode == Keys.F9)
			{
				showPlayerPos();
			}


		}
		public static void walkToModel(int model)
		{
			var entities = World.GetAllEntities();
			foreach (var entity in entities)
			{
				if (entity.Model.Hash == model)
				{
					GTA.UI.Screen.ShowSubtitle($"in walkToModel, found model, pos:{entity.Position}");
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

		public static bool walkToEntity(Entity target)
		{
			if (target == null)
			{
				//Log.Message(Log.Level.Debug, "walkToEntity, target not found");
				//Console.WriteLine("");
				return false;
			}
			Game.Player.Character.Task.GoTo(target, new Vector3(1.0f, 1.0f, 1.0f));
			//Log.Message(Log.Level.Debug, "walkToEntity, go to target");
			return true;
			//Console.WriteLine("walkToEntity, go to target");
		}
		
		public void walkTo(int hash)
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

		public static bool walkToPos(Vector3 pos)
		{
			Game.Player.Character.Task.GoTo(pos);
			return true;
		}

		public bool didHurtAnyOne()
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
		public static bool runTo(Entity target)
		{
			if (target == null)
			{
				//Log.Message(Log.Level.Debug, "walkToEntity, target not found");
				//Console.WriteLine("");
				return false;
			}
			Game.Player.Character.Task.RunTo(target.Position);
			//Log.Message(Log.Level.Debug, "walkToEntity, go to target");
			return true;
			//Console.WriteLine("walkToEntity, go to target");
		}
		
		// swim is not supported now
		public static bool swimTo(Entity target)
		{
			Ped player = Game.Player.Character;
			if (player == null)
			{
				return false;
			}
			if (player.IsInWater)
			{
				player.Task.RunTo(target.Position);
				return true;
			}
			return false;
		}
		

		public void driveVehicleForward()
		{
			var vehicle = Game.Player.Character.CurrentVehicle;
			if (vehicle != null)
			{
			}
		}
		public void getOnNearByVehicle()
		{
			Game.Player.Character.Task.EnterAnyVehicle();
		}


		public static bool getOnNearbyVehicle()
		{
			Vehicle closest = World.GetClosestVehicle(Game.Player.Character.Position, 10);
			if (closest != null && closest.IsDriveable)
			{
				Game.Player.Character.Task.EnterVehicle(closest, GTA.VehicleSeat.Driver);
				return true;
			}
			return false;
		}

		//get off vehicle
		public static void getOffVehicle()
		{
			Ped player = Game.Player.Character;
			Vehicle curVehicle = player.CurrentVehicle;
			if (curVehicle != null && player.IsInVehicle())
			{
				player.Task.LeaveVehicle(curVehicle, true);
			}
		}

		public static bool driveForward(float add_v = 1)
		{
			Vehicle now = Game.Player.Character.CurrentVehicle;
			if (now == null)
			{
				return false;
			}
			now.Velocity += now.ForwardVector.Normalized * add_v;
			return true;
		}

		public static bool driveTo(Vehicle tool, Vector3 pos)
		{
			if (pos == null)
			{
				return false;
			}
			Game.Player.Character.Task.DriveTo(tool, pos, 5.0f, 10.0f);
			return true;
		}


		//player sound horn in car (not working)
		public void pressHorn()
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
		public void rotate(Vector3 v)
		{
			Vehicle now = Game.Player.Character.CurrentVehicle;
			if (now == null)
			{
				return;
			}
			now.Rotation += v;
		}
		public static void showPlayerPos()
		{
			Vector3 playerPosition = Game.Player.Character.Position;
			GTA.UI.Screen.ShowSubtitle($"Player Position: {playerPosition.X}, {playerPosition.Y}, {playerPosition.Z}");
		}


		//jump up the object in front of the player
		public void climbUp()
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
		public void talkTo(int target_ped_hash)
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

		public void talkToClosestPed()
		{
			Ped target_ped = World.GetClosestPed(Game.Player.Character.Position, 50.0f);

			if (target_ped != null)
			{
				Ped player = Game.Player.Character;
				player.Task.ChatTo(target_ped);
			}
		}

		//combat with someone
		public void combat(int target_ped_hash)
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
		public void aimGunAt(int hash)
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

		public static void openVehicleFrontRightDoor()
		{
			Ped player = Game.Player.Character;
			Vehicle car = World.GetClosestVehicle(player.Position, 5.0f);

			if (car != null) 
			{
				car.Doors[VehicleDoorIndex.FrontRightDoor].Open();
			}
		}
		public static bool letDogGoToPlayer()
		{
			Ped player = Game.Player.Character;
			Ped chop = World.GetClosestPed(player.Position, 10.0f, PedHash.Chop);

			if (chop.Exists() && player.Exists())
			{
				
				float distanceToPlayer = chop.Position.DistanceTo(player.Position);
				
				if (distanceToPlayer >= 1.0f)
				{
					GTA.UI.Notification.Show("try to make chop follow...");
					Math.Vector3 offset = new Math.Vector3(0.3f, 0.3f, 0.3f); 
					float speed = 2.0f;  // 设置移动速度
					chop.Task.GoTo(player.Position + offset);
					Log.Message(Log.Level.Debug, "letDogFollow, go to player");
					return true;
				}
			}
			return false;
		}

		public static bool letFollow(Ped target)
		{
			Ped player = Game.Player.Character;

			if (target.Exists() && player.Exists())
			{
				float distanceToPlayer = target.Position.DistanceTo(player.Position);

				target.Task.FollowToOffsetFromEntity(player, new Vector3(0.2f, 0.3f, 0.0f), 1.0f);
				return true;
			}
			return false;
		}

		public static void letStopFollow(Ped target)
		{
			Ped player = Game.Player.Character;

			if (target.Exists() && player.Exists())
			{
				float distanceToPlayer = target.Position.DistanceTo(player.Position);

				if (distanceToPlayer <= 5.0f)
				{
					target.Task.ClearAllImmediately();
				}
			}
		}

		public static bool letOnCar(Ped target)
		{

			Ped player = Game.Player.Character;
			Vehicle car = World.GetClosestVehicle(player.Position, 5.0f);
			if (target.Exists() && player.Exists() && car.Exists())
			{
				Log.Message(Log.Level.Debug, "letDogOnCar.");
				float distanceToCar = target.Position.DistanceTo(car.Position);
				bool isFrontRightDoorOpen = car.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen;
				if (distanceToCar <= 5.0f && isFrontRightDoorOpen)
				{
					GTA.UI.Notification.Show("RightFront door is open, letting chop into the car...");
					target.Task.ClearAllImmediately();
					target.Task.EnterVehicle(car, VehicleSeat.RightFront);
					return true;
				}
			}
			return false;
		}

		public static bool stopFight(Ped target1, Ped  target2)
		{
			Ped player = Game.Player.Character;
			if (target1.Exists() && target2.Exists() && player.Exists())
			{
				float distance1 = target1.Position.DistanceTo(player.Position);
				float distance2 = target2.Position.DistanceTo(player.Position);

				if (distance1 <= 5.0f || distance2 <= 5.0f)
				{
					player.Task.PlayAnimation("anim@heists@ornate_bank@chat_manager", "average_clothes");
					target1.Task.ClearAllImmediately();
					target2.Task.ClearAllImmediately();
					return true;
				}
			}
			return false;
		}

	}
}
