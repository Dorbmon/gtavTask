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

		public static bool walkTo(Entity target)
		{
			if (target == null)
			{
				return false;
			}
			Game.Player.Character.Task.GoTo(target, new Vector3(1.0f, 1.0f, 1.0f));
			return true;
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

		public static bool didHurtAnyOne()
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
				return false;
			}
			Game.Player.Character.Task.RunTo(target.Position);
			return true;
		}
		
		// swim to an object
		public static bool swimTo(Entity target)
		{
			Ped player = Game.Player.Character;
			if (player == null)
			{
				return false;
			}
			player.Task.RunTo(target.Position);
			return true;
		}
		
		/*
		public void getOnNearByVehicle()
		{
			Game.Player.Character.Task.EnterAnyVehicle();
		}
		*/

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

		public static bool getOnVehicle(Entity target)
		{
			if (target != null)
			{
				Vehicle car = target as Vehicle;
				if (car != null && car.IsDriveable)
				{
					Game.Player.Character.Task.EnterVehicle(car, VehicleSeat.Driver);
					return true;
				}
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

		public static bool driveTo(Entity tool, Entity target)
		{
			if (target == null)
			{
				return false;
			}
			Vehicle vehicle = tool as Vehicle;
			if (vehicle != null)
			{
				Game.Player.Character.Task.DriveTo(vehicle, target.Position, 5.0f, 10.0f);
				return true;
			}
			return false;
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
		public static void climbUp()
		{
			Ped player = Game.Player.Character;
			player.Task.Climb();
		}

		//climb up the ladder
		private void climbLadderTo(Entity target)
		{
			Ped player = Game.Player.Character;
			player.Task.GoTo(target);
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

		public static bool letChase(Ped target)
		{
			Ped player = Game.Player.Character;

			if (target.Exists() && player.Exists())
			{

				target.Task.FollowToOffsetFromEntity(player, new Vector3(0.2f, 0.2f, 0.0f), 2.0f);
				return true;
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

		public static bool letOn(Entity tool, Entity target)
		{

			Ped player = Game.Player.Character;
			Vehicle car = tool as Vehicle;
			Ped ped = target as Ped;
			if (ped.Exists() && player.Exists() && car.Exists())
			{
				Log.Message(Log.Level.Debug, "letDogOnCar.");
				float distanceToCar = target.Position.DistanceTo(car.Position);
				bool isFrontRightDoorOpen = car.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen;
				if (distanceToCar <= 5.0f && isFrontRightDoorOpen)
				{
					GTA.UI.Notification.Show("RightFront door is open, letting chop into the car...");
					ped.Task.ClearAllImmediately();
					ped.Task.EnterVehicle(car, VehicleSeat.RightFront);
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
		/*
		public static bool getInNearbyDoor(Entity door)
		{
			var player = Game.Player.Character;
			var nearbyEntities = World.GetNearbyEntities(player.Position, 10f);

			foreach (var entity in nearbyEntities)
			{
				// 你需要找到判断实体是否为门的方法
				// 也许你可以通过模型名称、散列值等来判定
				if (isDoor(door))
				{
					// 尝试打开门，并观察结果
					if  (tryOpenDoor(door))
					{
						// 如果门打开了，给玩家一个去那个位置的任务
						player.Task.GoTo(door.Position);
						return true;
					}
				}
			}
			return false;
		}

		bool isDoor(Entity obj)
		{
			World.getclosest
		}

		*/

	}
}
