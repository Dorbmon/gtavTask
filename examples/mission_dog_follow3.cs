using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
//using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using GTA.Native;
using SHVDN;

namespace GTA
{
	[ScriptAttributes(NoDefaultInstance = true)]
	internal class mission_dog_follow3 : mission
	{
		enum MissionState
		{
			NotStarted,
			Display,
			ExitVehicle,

			WalkToSpot1,
			WalkToDog,
			CommandDogToFollow,
			WalkToVehicle,
			CommandDogToStop,
			OpenFrontRightDoor,
			DogInVehicle,
			PlayerToDriverSeat,
			PlayerInVehicle,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle;
		private Vector3 carPos = new Vector3(0, 0, 0);
		private Vector3 dogPos1 = new Vector3(0, 0, 0);
		private Vector3 dogPos2 = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 personPos1 = new Vector3(0, 0, 0);
		private Vector3 personPos2 = new Vector3(0, 0, 0);
		private Vector3 personPos3 = new Vector3(0, 0, 0);
		private Model dogModel;
		private Model personModel;
		private Ped dog, dog2, person1, person2, person3;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToState = false;
		private bool walkToCarState = false;
		private bool walkToDogState = false;
		private bool walkToVehicileState = false;
		private bool dogFollowState = false;
		private bool dogOnVehicle = false;
		private bool playerInVehicleState = false;
		private int pause = 150;
		private int endPause = 2400;
		private bool isPaused = false;



		public mission_dog_follow3()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_dog_follow...");
			Ped player = Game.Player.Character;

			changePos(ref playerPos, 327, -1842, 27);
			changePos(ref carPos, 333, -1833, 27);
			changePos(ref dogPos1, 343, -1849, 28);
		
			// 设置游戏时间为下午3点30分
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			// 设置天气为晴朗
			World.Weather = Weather.Clear;

			Game.Player.Character.Position = playerPos;
			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 200.0f))
			{
				if (ped != Game.Player.Character) // 不删除玩家角色
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 200.0f))
			{
				vehicle.Delete();
			}

			vehicle = World.CreateVehicle(VehicleHash.BestiaGTS, carPos);
			vehicle.Heading = 240;
			dogModel = new Model(PedHash.Shepherd);
			if (dogModel.IsValid)
			{
				dogModel.Request(500);
				dog = World.CreatePed(dogModel, dogPos1);
				dog.Heading = 180;
				
			}

			if (vehicle != null && dog != null )
			{
				isLoaded = true;
				curState = MissionState.WalkToSpot1;
			}
		}

		
		public override void destroy()
		{

			if (vehicle != null)
			{
				vehicle.Delete();
			}
			if (dog != null)
			{
				dog.Delete();
				dogModel.MarkAsNoLongerNeeded();
			}
			if (dog2 != null)
			{
				dog2.Delete();
				dogModel.MarkAsNoLongerNeeded();
			}
			GTA.UI.Notification.Show("mission_dog_follow destroy!");

		}

		public override bool is_mission_finished()
		{
			return isMissionSucceed;
		}
		public void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F12)
			{
				isPaused = !isPaused;
				GTA.UI.Notification.Show("Mission Paused");
			}
		}

		private void OnTick(object sender, EventArgs e)
		{
			if (isPaused)
			{
				return;
			}
			walkTo(curState, vehicle);
			walkTo(curState, dog);
			letFollow(curState, dog);
			walkTo(curState, vehicle);
			letStopFollow(curState, dog);
			getOn(curState, vehicle);
			openCarDoor(curState);
			letGetOn(curState, dog, vehicle);
			checkResult(curState);
		}

		private void walkTo(MissionState state, Entity target)
		{

			Ped player = Game.Player.Character;
			if (state != MissionState.WalkToDog && state != MissionState.WalkToVehicle
				&& state != MissionState.WalkToSpot1)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (state == MissionState.WalkToDog)
			{
				if (target != dog) return;
			}
			if (state == MissionState.WalkToVehicle)
			{
				if (target != vehicle) return;
			}
			if (state == MissionState.WalkToSpot1)
			{
				if (target != vehicle) return;
			}
			
			if (!walkToState) walkToState = PlayerActions.walkTo(target);

			float distance = Vector3.Distance(player.Position, target.Position);
			//Log.Message(Log.Level.Debug, "walkTo, ", " state: ", state.ToString(), " target: ", target.ToString()," distance: ", distance.ToString());
			GTA.UI.Screen.ShowSubtitle($"state: {state.ToString()}, distance: {distance}");
			if (distance < 3.0f)
			{
				if (state == MissionState.WalkToSpot1)
				{
					curState = MissionState.WalkToDog;
					GTA.UI.Notification.Show("Walk to spot1 completed. Walk to dog.");
					walkToState = false;
					
				}
				else if (state == MissionState.WalkToDog)
				{
					curState = MissionState.CommandDogToFollow;
					GTA.UI.Notification.Show("Walk to dog completed. Command dog to follow.");
					walkToState = false;
					
				}
				else if (state == MissionState.WalkToVehicle)
				{
					curState = MissionState.CommandDogToStop;
					GTA.UI.Notification.Show("Walk to vehicle completed. Command dog to stop.");
					walkToState = false;
					
				}
			}
			counter = 0;
		}

		private void letFollow(MissionState state, Entity target)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.CommandDogToFollow)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			Ped ped = target as Ped;
			if (ped != null)
			{
				if (!dogFollowState) dogFollowState = PlayerActions.letFollow(ped);
			}
			if (dogFollowState)
			{
				curState = MissionState.WalkToVehicle;
				GTA.UI.Notification.Show("Command dog to follow completed. Walk to spot2.");
			}
			counter = 0;
		}
		
		private void letStopFollow(MissionState state, Entity target)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.CommandDogToStop)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			Ped ped = target as Ped;
			float distance = Vector3.Distance(vehicle.Position, target.Position);
			GTA.UI.Screen.ShowSubtitle($"state: {state.ToString()}, distance: {distance}");
			Log.Message(Log.Level.Debug, "letStopFollow, ", " state: ", state.ToString(), " target: ", target.ToString(), " distance: ", distance.ToString());
			if (distance < 5.0f)
			{
				PlayerActions.letStopFollow(ped);
				curState = MissionState.PlayerInVehicle;
				GTA.UI.Notification.Show("Command dog to stop follow completed. Player in vehicle.");
			}
			counter = 0;
		}

		private void getOn(MissionState state, Entity target)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.PlayerInVehicle)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (!playerInVehicleState) playerInVehicleState = PlayerActions.getOnVehicle(target);

			if (player.IsInVehicle())
			{
				curState = MissionState.OpenFrontRightDoor;
				GTA.UI.Notification.Show("Player in vehicle completed. Open front right door.");
			}
			counter = 0;
		}

		private void openCarDoor(MissionState state)
		{
			if (state != MissionState.OpenFrontRightDoor)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			PlayerActions.openVehicleFrontRightDoor();
			bool isRightFrontDoorOpen = vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen;
			//Log.Message(Log.Level.Debug, "OpenFrontRightDoor");

			if (isRightFrontDoorOpen)
			{
				curState = MissionState.DogInVehicle;
				GTA.UI.Notification.Show("Open door is completed. Dog get in car.");
			}
			counter = 0;
		}
		private void letGetOn(MissionState state, Entity target, Entity tool)
		{
			if (state != MissionState.DogInVehicle)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (!dogOnVehicle) dogOnVehicle = PlayerActions.letOn(tool, target);

			if (dog.IsInVehicle())
			{
				curState = MissionState.Completed;
				GTA.UI.Notification.Show("Dog in vehicle is completed. Mission completed... Result checking...");
			}
			counter = 0;
		}
		private void checkResult(MissionState state)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.Completed)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (player.CurrentVehicle == dog.CurrentVehicle)
			{
				isMissionSucceed = true;
			}
			counter = 0;
		}
		/*
		private void OnTick_case(object sender, EventArgs e)
		{
			Ped player = Game.Player.Character;
			if (isPaused)
			{
				return;
			}

			switch (curState)
			{

				case MissionState.NotStarted:
					if (!isLoaded)
					{
						return;
					}
					if (counter < pause)
					{
						counter++;
						return;
					}
					curState = MissionState.WalkToCar;
					GTA.UI.Notification.Show("Mission started. Walk To Car.");
					counter = 0;

					break;
				//場景展示
				case MissionState.Display:
					if (counter < 1000)
					{
						counter++;
						return;
					}
					person1.Task.PlayAnimation("amb@world_human_stand_impatient@male@no_sign@base", "base", 1.0f, -1, 0.01f);
					person2.Task.PlayAnimation("amb@world_human_guard_patrol@male@idle_b", "idle_e", 1.0f, -1, 0.01f);
					person3.Task.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 1.0f, -1, 0.01f);
					counter = 0;
					break;
				case MissionState.ExitVehicle:
					//action
					if (vehicle != null)
					{
						PlayerActions.getOffVehicle();
					}
					if (player.CurrentVehicle == null && !player.IsInVehicle())
					{
						curState = MissionState.WalkToDog;
						GTA.UI.Notification.Show("Exit vehicle completed. Walk to the dog.");
					}
					break;

				case MissionState.WalkToCar:
				if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (vehicle != null)
					{
						if (!walkToCarState) walkToCarState = PlayerActions.walkTo(vehicle);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"vehicle is null!");
					}


					float car_distance = Vector3.Distance(player.Position, vehicle.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {car_distance}");
					if (car_distance < 8.0f)
					{
						curState = MissionState.WalkToDog;
						GTA.UI.Notification.Show("Walk to car completed. walk to dog now.");
					}
					counter = 0;
					break;
				case MissionState.WalkToDog:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (dog != null)
					{
						if (!walkToDogState) walkToDogState = PlayerActions.walkTo(dog);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"dog is null!");
					}


					float distance = Vector3.Distance(player.Position, dog.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 2.0f)
					{
						curState = MissionState.CommandDogToFollow;
						GTA.UI.Notification.Show("Walk to dog completed. Command dog to follow.");
					}
					counter = 0;
					break;

				case MissionState.CommandDogToFollow:
					if (counter < pause)
					{
						counter++;
						return;
					}

					float dist = Vector3.Distance(player.Position, dog.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {dist}");

					if (!dogFollowState) dogFollowState = PlayerActions.letFollow(dog);
					if (!walkToVehicileState) walkToVehicileState = PlayerActions.walkTo(vehicle);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					
					if (Vector3.Distance(dog.Position, vehicle.Position) < 5.0f)
					{
						PlayerActions.letStopFollow(dog);
						curState = MissionState.PlayerInVehicle;
						GTA.UI.Notification.Show("Command dog to follow completed. Player in vehicle.");
					}
					counter = 0;
					break;

				case MissionState.PlayerInVehicle:
					if (counter < pause)
					{
						counter++;
						return;
					}

					if (!playerInVehicleState) playerInVehicleState = PlayerActions.getOnNearbyVehicle();

					if (player.IsInVehicle())
					{
						curState = MissionState.OpenFrontRightDoor;
						GTA.UI.Notification.Show("Player in vehicle completed. Open front right door.");
					}
					counter = 0;
					break;

				case MissionState.OpenFrontRightDoor:
					if (counter < pause)
					{
						counter++;
						return;
					}

					PlayerActions.openVehicleFrontRightDoor();
					bool isRightFrontDoorOpen = vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen;
					Log.Message(Log.Level.Debug, "OpenFrontRightDoor");

					if (isRightFrontDoorOpen)
					{
						curState = MissionState.DogInVehicle;
						GTA.UI.Notification.Show("Open door is completed. Dog get in car.");
					}
					counter = 0;
					break;

				case MissionState.DogInVehicle:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (!dogOnVehicle) dogOnVehicle = PlayerActions.letOn(vehicle, dog);

					if (dog.IsInVehicle())
					{
						curState = MissionState.Completed;
						GTA.UI.Notification.Show("Dog in vehicle is completed. Mission completed... Result checking...");
					}
					counter = 0;
					break;

				case MissionState.Completed:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (player.CurrentVehicle == dog.CurrentVehicle)
					{
						isMissionSucceed = true;
					}
					break;
			}
		}
		*/
		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}

		private void playerAct()
		{

		}
	}
}
