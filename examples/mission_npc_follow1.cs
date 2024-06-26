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
	internal class mission_npc_follow1 : mission
	{
		enum MissionState
		{
			NotStarted,
			Display,
			ExitVehicle,
			WalkToNpc,
			WalkToSpot1,
			WalkToSpot2,
			CommandNpcToFollow,
			WalkToShelter,
			CommandNpcToStop,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle, spot1, spot2, endtarget;
		private Vector3 shelterPos = new Vector3(0, 0, 0);
		private Vector3 npcPos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 spotPos1 = new Vector3(0, 0, 0);
		private Vector3 spotPos2 = new Vector3(0, 0, 0);
		private Ped npc;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToState = false;
		private bool walkToSpot1State = false;
		private bool walkToSpot2State = false;
		private bool walkToNpcState = false;
		private bool walkToShelterState = false;
		private bool npcFollowState = false;
		private int pause = 150;
		private int endPause = 2400;
		private bool isPaused = false;
		private bool isAuto = false;

		private int npc_handle = 0;
		private int spot1_handle = 0;
		private int spot2_handle = 0;
		private int endtarget_handle = 0;

		public mission_npc_follow1()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_npc_follow...");
			Ped player = Game.Player.Character;

			changePos(ref playerPos, 135, -1056, 29);
			changePos(ref shelterPos, 99, -1070, 29);
			changePos(ref npcPos, 152, -1049, 29);
			changePos(ref spotPos1, 150, -1061, 29);
			changePos(ref spotPos2, 148, -1066, 29);
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

			npc = World.CreatePed(PedHash.Downtown01AMY, npcPos);
			npc_handle = npc.Handle;
			spot1 = World.CreateVehicle(VehicleHash.Sanchez, spotPos1);
			spot1_handle = spot1.Handle;
			spot2 = World.CreateVehicle(VehicleHash.Sanchez, spotPos2);
			spot2_handle = spot2.Handle;
			endtarget = World.CreateVehicle(VehicleHash.Sanchez, shelterPos);
			endtarget_handle = endtarget.Handle;
			Log.Message(Log.Level.Info, "mission_npc_follow1::load, npc_handle:", npc_handle.ToString(),
				", spot1_handle:", spot1_handle.ToString(),
				", spot2_handle:", spot2_handle.ToString(),
				", endtarget_handle:", endtarget_handle.ToString());

			CreateFile();
			if (npc == null)
			{
				GTA.UI.Notification.Show("NPC CREATE FAILED !");
			}
			if (npc != null && spot1 != null && spot2 != null && endtarget != null)
			{
				isLoaded = true;
				curState = MissionState.WalkToNpc;
			}
			
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
			if (spot1 != null) { spot1.Delete(); }
			if (spot2 != null) { spot2.Delete(); }
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
			/*
			 WalkToNpc,
			WalkToSpot,
			CommandNpcToFollow,
			WalkToShelter,
			CommandNpcToStop,
			 */
			if (isPaused)
			{
				return;
			}
			walkTo(curState, npc);
			letFollow(curState, npc);
			walkTo(curState, spot1);
			walkTo(curState, spot2);
			walkTo(curState, endtarget);
			letStopFollow(curState, npc);
			checkResult(curState);
		}

		private void walkTo(MissionState state, Entity target)
		{

			Ped player = Game.Player.Character;
			if (state != MissionState.WalkToNpc && state != MissionState.WalkToSpot1
				&& state != MissionState.WalkToShelter && state != MissionState.WalkToSpot2)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (state == MissionState.WalkToNpc)
			{
				if (target != npc) return;
			}
			if (state == MissionState.WalkToSpot1)
			{
				if (target != spot1) return;
			}
			if (state == MissionState.WalkToSpot2)
			{
				if (target != spot2) return;
			}
			if (state == MissionState.WalkToShelter)
			{
				if (target != endtarget) return;
			}
			if (isAuto)if (!walkToState) walkToState = PlayerActions.walkTo(target);

			float distance = Vector3.Distance(player.Position, target.Position);
			//Log.Message(Log.Level.Debug, "walkTo, ", " state: ", state.ToString(), " target: ", target.ToString()," distance: ", distance.ToString());
			GTA.UI.Screen.ShowSubtitle($"state: {state.ToString()}, distance: {distance}");
			if (distance < 3.0f)
			{
				if (state == MissionState.WalkToNpc)
				{
					curState = MissionState.CommandNpcToFollow;
					GTA.UI.Notification.Show("Walk to npc completed. Command npc to follow.");
					walkToState = false;
					
				}
				else if (state == MissionState.WalkToShelter)
				{
					curState = MissionState.CommandNpcToStop;
					GTA.UI.Notification.Show("Walk to shelter completed. Command npc to stop.");
					walkToState = false;
					
				}
				else if (state == MissionState.WalkToSpot1)
				{
					curState = MissionState.WalkToSpot2;
					GTA.UI.Notification.Show("Walk to spot1 completed. Walk to spot2.");
					walkToState = false;
					
				}
				else if (state == MissionState.WalkToSpot2)
				{
					curState = MissionState.WalkToShelter;
					GTA.UI.Notification.Show("Walk to spot2 completed. Walk to shelter.");
					walkToState = false;
					
				}
			}
			counter = 0;
		}

		private void letFollow(MissionState state, Entity target)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.CommandNpcToFollow)
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
				if (isAuto)if (!npcFollowState) npcFollowState = PlayerActions.letFollow(ped);
			}
			if (npcFollowState)
			{
				curState = MissionState.WalkToSpot1;
				GTA.UI.Notification.Show("Command npc to follow completed. Walk to spot1.");
			}
			counter = 0;
		}

		private void letStopFollow(MissionState state, Entity target)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.CommandNpcToStop)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			float distance = Vector3.Distance(endtarget.Position, target.Position);
			GTA.UI.Screen.ShowSubtitle($"state: {state.ToString()}, distance: {distance}");
			Log.Message(Log.Level.Debug, "letStopFollow, ", " state: ", state.ToString(), " target: ", target.ToString(), " distance: ", distance.ToString());
			if (distance < 3.0f)
			{
				Ped ped = target as Ped;
				if (isAuto)if (ped != null) { PlayerActions.letStopFollow(ped); }
				curState = MissionState.Completed;
				GTA.UI.Notification.Show("Command npc to stop follow completed. Mission completed.");
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
			float npc_shlt_dist = Vector3.Distance(shelterPos, npc.Position);
			if (npc_shlt_dist < 5.0f)
			{
				isMissionSucceed = true;
			}
			counter = 0;
		}
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
					curState = MissionState.WalkToSpot1;
					GTA.UI.Notification.Show("Mission started. Walk to spot1.");
					counter = 0;

					break;

				case MissionState.WalkToSpot1:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (spot1 != null)
					{
						if (isAuto)if (!walkToSpot1State) walkToSpot1State = PlayerActions.walkTo(spot1);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"spot1 is null!");
					}


					float sp1_distance = Vector3.Distance(player.Position, spot1.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {sp1_distance}");
					if (sp1_distance < 5.0f)
					{
						curState = MissionState.WalkToNpc;
						GTA.UI.Notification.Show("Walk to spot1 completed. Walk to npc.");
					}
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
						if (isAuto)if (!walkToNpcState) walkToNpcState = PlayerActions.walkTo(npc);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"npc is null!");
					}


					float distance = Vector3.Distance(player.Position, npc.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 5.0f)
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

					if (isAuto)if (!npcFollowState) npcFollowState = PlayerActions.letFollow(npc);
					if (isAuto)if (!walkToSpot2State) walkToSpot2State = PlayerActions.walkTo(spot2);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					*/
					if (Vector3.Distance(npc.Position, spot2.Position) < 5.0f)
					{
						curState = MissionState.WalkToShelter;
						GTA.UI.Notification.Show("Command npc to follow completed. walk to shelter.");
					}
					counter = 0;
					break;

				case MissionState.WalkToShelter:
					if (counter < pause)
					{
						counter++;
						return;
					}

					float slt_dist = Vector3.Distance(player.Position, endtarget.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {slt_dist}");

					if (isAuto)if (!npcFollowState) npcFollowState = PlayerActions.letFollow(npc);
					if (isAuto)if (!walkToShelterState) walkToShelterState = PlayerActions.walkTo(endtarget);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					*/
					if (Vector3.Distance(npc.Position, endtarget.Position) < 5.0f)
					{
						if (isAuto)PlayerActions.letStopFollow(npc);
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
