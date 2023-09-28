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
	internal class mission_npc_follow2 : mission
	{
		enum MissionState
		{
			NotStarted,
			ExitVehicle,
			WalkToNpc,
			CommandNpcToFollow,
			WalkToSpot,
			WalkToShelter,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle, spot1, endtarget;
		private Vector3 shelterPos = new Vector3(0, 0, 0);
		private Vector3 npcPos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 spotPos = new Vector3(0, 0, 0);
		private Ped npc;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToSpotState = false;
		private bool walkToNpcState = false;
		private bool walkToShelterState = false;
		private bool npcFollowState = false;
		private int pause = 150;
		private bool isPaused = false;



		public mission_npc_follow2()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_npc_follow...");
			Ped player = Game.Player.Character;

			changePos(ref playerPos, -462, -1706, 19);
			changePos(ref shelterPos, -430, -1692, 20);
			changePos(ref npcPos, -456, -1694, 18);
			changePos(ref spotPos, -445, -1709, 18);
			// 设置游戏时间为下午3点30分
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			// 设置天气为晴朗
			World.Weather = Weather.Clear;

			Game.Player.Character.Position = playerPos;
			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 20.0f))
			{
				if (ped != Game.Player.Character) // 不删除玩家角色
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 20.0f))
			{
				vehicle.Delete();
			}

			npc = World.CreatePed(PedHash.Downtown01AFM, npcPos);
			spot1 = World.CreateVehicle(VehicleHash.Alpha, spotPos);
			endtarget = World.CreateVehicle(VehicleHash.Sentinel, shelterPos);
			if (npc == null)
			{
				GTA.UI.Notification.Show("NPC CREATE FAILED !");
			}

			isLoaded = true;
		}

		public override void destroy()
		{

			if (vehicle != null)
			{
				vehicle.Delete();
			}
			if (npc != null)
			{
				npc.Delete();
			}
			GTA.UI.Notification.Show("mission_npc_follow destroy!");

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
					if (counter < 10)
					{
						counter++;
						return;
					}
					curState = MissionState.WalkToNpc;
					GTA.UI.Notification.Show("Mission started. WalkToNpc.");
					counter = 0;

					break;

				case MissionState.WalkToNpc:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (npc != null)
					{
						if (!walkToNpcState) walkToNpcState = PlayerActions.walkToEntity(npc);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"npc is null!");
					}


					float distance = Vector3.Distance(player.Position, npc.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 2.0f)
					{
						curState = MissionState.CommandNpcToFollow;
						GTA.UI.Notification.Show("Walk to npc completed. Command npc to follow.");
					}
					counter = 0;
					break;

				case MissionState.CommandNpcToFollow:
					if (counter < pause)
					{
						counter++;
						return;
					}

					float dist = Vector3.Distance(player.Position, npc.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {dist}");

					if (!npcFollowState) npcFollowState = PlayerActions.letFollow(npc);
					if (!walkToSpotState) walkToSpotState = PlayerActions.walkToEntity(spot1);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					*/
					if (Vector3.Distance(npc.Position, spotPos) < 5.0f)
					{
						curState = MissionState.WalkToShelter;
						GTA.UI.Notification.Show("Command dog to follow completed. walk to shelter.");
					}
					counter = 0;
					break;

				case MissionState.WalkToShelter:
					if (counter < pause)
					{
						counter++;
						return;
					}

					float slt_dist = Vector3.Distance(player.Position, npc.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {slt_dist}");

					if (!npcFollowState) npcFollowState = PlayerActions.letFollow(npc);
					if (!walkToShelterState) walkToShelterState = PlayerActions.walkToEntity(endtarget);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					*/
					if (Vector3.Distance(npc.Position, shelterPos) < 5.0f)
					{
						PlayerActions.letStopFollow(npc);
						curState = MissionState.Completed;
						GTA.UI.Notification.Show("walk to shelter completed. Mission complete.");
					}
					counter = 0; 
					break;

				case MissionState.Completed:
					if (counter < pause)
					{
						counter++;
						return;
					}
					float npc_shlt_dist = Vector3.Distance(shelterPos, npc.Position);
					if (npc_shlt_dist < 5.0f)
					{
						isMissionSucceed = true;
					}
					counter = 0;
					break;
			}
		}

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}

		private void playerAct()
		{

		}
	}
}
