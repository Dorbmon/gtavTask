using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
//using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using GTA.Native;
using System.IO;
using SHVDN;

namespace GTA
{
	[ScriptAttributes(NoDefaultInstance = true)]
	internal class mission_dog_follow1 : mission
	{
		enum MissionState
		{
			NotStarted,
			//Display,
			ExitVehicle,
			WalkToDog,
			CommandDogToFollow,
			WalkToVehicle,
			CommandDogToStop,
			OpenFrontRightDoor,
			DogInVehicle,
			PlayerToDriverSeat,
			PlayerInVehicle,
			Completed,
			Display
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
		private bool walkToDogState = false;
		private bool walkToVehicileState = false;
		private bool dogFollowState = false;
		private bool dogOnVehicle = false;
		private bool playerInVehicleState = false;
		private int pause = 80;
		private int endPause = 2400;
		private bool isPaused = false;
		private bool isAuto = false;

		private int dog_handle = 0;
		private int vehicle_handle = 0;
		
		public mission_dog_follow1()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_dog_follow...");
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -1935, 588, 121);
			changePos(ref carPos, -1940, 582, 119.3f);
			changePos(ref dogPos1, -1946, 592, 120);
			//changePos(ref dogPos2, -1939, 584, 119);
			// 按序号增加了3个默认person位置。坐标和面向角度已设置好
			//changePos(ref personPos1, -1932, 593, 121);
			//changePos(ref personPos2, -1946, 590, 119);/*原本设置的目的地坐标：-1938.1f, 582, 119.5f*/
			//changePos(ref personPos3, -1939, 583, 119);
			// 设置游戏时间为下午3点30分
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;
			Game.Player.Character.Position = playerPos;

			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 20.0f))
			{
				if (ped != Game.Player.Character) 
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 20.0f))
			{
				vehicle.Delete();
			}

			vehicle = World.CreateVehicle(VehicleHash.BestiaGTS, carPos);
			vehicle.Heading = 60;
			vehicle_handle = vehicle.Handle;

			dogModel = new Model(PedHash.Chop);
			if (dogModel.IsValid)
			{
				dogModel.Request(500);
				dog = World.CreatePed(dogModel, dogPos1);
				dog.Heading = 180;
				/*
				dog2 = World.CreatePed(dogModel, dogPos2);
				dog2.Heading = 180;
				if (dog == null || dog2 == null)
				{
					GTA.UI.Notification.Show("CHOP CREATE FAILED !");
				}
				*/
			}
			dog_handle = dog.Handle;
			Log.Message(Log.Level.Info, "mission_dog_follow1::load, dog_handle:", dog_handle.ToString(),
						", vehicle_handle:", vehicle_handle.ToString());

			CreateFile();

			if (vehicle != null && dog != null)
			{
				isLoaded = true;
				curState = MissionState.WalkToDog;
			}
			
			/*
			Thread.Sleep(500);
			personModel = new Model(PedHash.Michael);
			if (personModel.IsValid)
			{
				personModel.Request(500);
				person1 = World.CreatePed(personModel, personPos1);
				person1.Heading = 92.2199F;

				person2 = World.CreatePed(personModel, personPos2);
				person2.Heading = 0F;
				//从设置的地方走向狗并交互
				//Function.Call(Hash.TASK_CHAT_TO_PED, person2.Handle, dog.Handle, -1, 0f, 0f, 0f, 0f, 0f);
				
				person3 = World.CreatePed(personModel, personPos3);
				person3.Heading = 162.963F;
			}
			*/

		}

		protected override string GetMissionInfo()
		{
			string content = $"vehicle: {vehicle_handle}\ndog: {dog_handle}";
			return content;
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
			if (state != MissionState.WalkToDog && state != MissionState.WalkToVehicle)
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

			if (isAuto)
			{
				if (!walkToState) walkToState = PlayerActions.walkTo(target.Handle);

			}
			float distance = Vector3.Distance(player.Position, target.Position);
			//Log.Message(Log.Level.Debug, "walkTo, ", " state: ", state.ToString(), " target: ", target.ToString()," distance: ", distance.ToString());
			//GTA.UI.Screen.ShowSubtitle($"state: {state.ToString()}, distance: {distance}");
			if (distance < 3.0f)
			{
				if (state == MissionState.WalkToDog)
				{
					curState = MissionState.CommandDogToFollow;
					GTA.UI.Notification.Show("Walk to dog completed. Command dog to follow.");
					walkToState = false;
					return;
				}
				else if (state == MissionState.WalkToVehicle)
				{
					curState = MissionState.CommandDogToStop;
					GTA.UI.Notification.Show("Walk to vehicle completed. Command dog to stop.");
					walkToState = false;
					return;
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
				if(isAuto)
				{
					if (!dogFollowState) dogFollowState = PlayerActions.letFollow(ped);
				}
			}
			if (dogFollowState)
			{
				curState = MissionState.WalkToVehicle;
				GTA.UI.Notification.Show("Command dog to follow completed. Walk to vehicle.");
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
			
			if (distance < 5.0f)
			{
				if (isAuto)
				{
					PlayerActions.letStopFollow(ped);
				}
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
			if (isAuto)
			{
				if (!playerInVehicleState) playerInVehicleState = PlayerActions.getOnVehicle(target);
			}
			
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
			if (isAuto)
			{
				PlayerActions.openVehicleFrontRightDoor();
			}
			bool isRightFrontDoorOpen = vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen;

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
			if (isAuto)
			{
				if (!dogOnVehicle) dogOnVehicle = PlayerActions.letOn(tool, target);
			}
			if (dog.IsInVehicle())
			{
				curState = MissionState.Completed;
				GTA.UI.Notification.Show("Mission completed.");
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

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
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
					curState = MissionState.ExitVehicle;
					GTA.UI.Notification.Show("Mission started. Exit Vehicle.");
					counter = 0;

					break;
				//場景展示
				case MissionState.Display:
					if (counter < 1000)
					{
						counter++;
						return;
					}
					person1.Task.PlayAnimation("amb@world_human_stand_impatient@male@no_sign@base", "base", 1.0f, -1, 0.01f );
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

					
					if (!walkToVehicileState) walkToVehicileState = PlayerActions.walkTo(vehicle);
					
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
					if (player.CurrentVehicle == dog.CurrentVehicle )
					{
						isMissionSucceed = true;
					}
					break;
			}
		}
		*/

	}
}
