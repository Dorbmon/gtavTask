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
	internal class event_vehicle_loop : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GenerateCarAndNpc,
			NpcDriveFirst,
			NpcDriveSecond,
			NpcExitAndWalk,
			NpcDisappear,
			NpcSpawn,
			NpcWalkToVehicle,
			NpcDriveBack,
			NpcDriveBack2,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle;
		private Ped ped;
		private Ped ped2;

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 npcGenPos = new Vector3(0, 0, 0);
		private Vector3 npcWalkPos = new Vector3(0, 0, 0);
		private Vector3 carGenPos = new Vector3(0, 0, 0);
		private Vector3 carDisPos = new Vector3(0, 0, 0);
		private Vector3 carStopPos = new Vector3(0, 0, 0);
		private Vector3 carThoughPos = new Vector3(0, 0, 0);
		private Vector3 carThoughPos2 = new Vector3(0, 0, 0);
		private Vector3 carEndPos = new Vector3(0, 0, 0);
		
		private PedHash _pedHash = new PedHash();
		private VehicleHash _vehicleHash = new VehicleHash();

		private TimeSpan timespan;
		private DateTime startTime;
		private bool isPaused = false;
		private bool timerStarted = false;
		private int loopTime = 0;
		private int executeTime = 0;

		public event_vehicle_loop()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}

		private void LoadSettings()
		{
			string configPath = $"scripts\\{this.GetType().Name}.ini";

			if (System.IO.File.Exists(configPath))
			{
				ScriptSettings config = ScriptSettings.Load(configPath);
				loopTime = config.GetValue("Settings", "LoopTime", 1);
			}
			else
			{
				loopTime = 1;  
			}
		}

		public override void load()
		{
			GTA.UI.Notification.Show($"load {this.GetType().Name}...");
			LoadSettings();
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -808, 158, 71);
			changePos(ref carGenPos, -839, 156, 67);
			changePos(ref carDisPos, -834, 156, 67);
			changePos(ref carStopPos, -818, 172, 71);
			
			changePos(ref carThoughPos, -829, 180, 70);
			changePos(ref carThoughPos2, -815, 159, 72);
			changePos(ref npcGenPos, -810, 164, 71);
			changePos(ref npcWalkPos, -812, 167, 72);
			changePos(ref carEndPos, -842, 154, 66);
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;
			Game.Player.Character.Position = playerPos;

			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 50.0f))
			{
				if (ped != Game.Player.Character)
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 50.0f))
			{
				vehicle.Delete();
			}
			startTime = DateTime.Now;
			curState = State.Waiting;
		}



		public override bool is_mission_finished()
		{
			return executeTime >= loopTime;
		}
		public void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F12)
			{
				isPaused = !isPaused;
				GTA.UI.Notification.Show("Mission Paused");
			}
		}


		public override void Update()
		{
			timespan = DateTime.Now - startTime;
			switch (curState)
			{	
				case State.Waiting:
					if (!timerStarted)
					{
						startTime = DateTime.Now;
						timerStarted = true;
						Log.Message(Log.Level.Info, $"{this.GetType().Name}::{curState.ToString()}, Starting wait period...");
					}
					else if ((DateTime.Now - startTime).TotalSeconds > 30)
					{
						curState = State.GenerateCarAndNpc;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.GenerateCarAndNpc:
					Wait(500);
					PedHashSafe randomPed = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					muscleHash randomVehicle = RandomEnumPicker.GetRandomEnumValue<muscleHash>();
					_vehicleHash = (VehicleHash)randomVehicle;
					_pedHash = (PedHash)randomPed;
					vehicle = World.CreateVehicle(_vehicleHash, carGenPos);
					
					Random random = new Random();
					float offsetX = (float)(random.NextDouble() * 10.0 - 5.0); 
					float offsetY = (float)(random.NextDouble() * 10.0 - 5.0); 
					Vector3 pedPosition = new Vector3(carGenPos.X + offsetX, carGenPos.Y + offsetY, carGenPos.Z);
					ped = World.CreatePed(_pedHash, pedPosition);
					
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate car and npc, " +
												$"car:hash_name={_vehicleHash.ToString()}, hash_code={vehicle.Model.GetHashCode().ToString()}, generate_position={carGenPos.ToString()}, " +
												$"primary_color={vehicle.Mods.PrimaryColor.ToString()}, secondary_color={vehicle.Mods.SecondaryColor.ToString()}, " +
												$"NPC:hash_name={_pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, generate_position={pedPosition.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcDriveFirst;
					break;
				case State.NpcDriveFirst:
					Wait(500);
					DriveTo(ped, vehicle, carThoughPos);
					Wait(5000);
					if (vehicle.Position.DistanceTo(carThoughPos) < 8.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, drive to first stop point, " +
													$"stop_position={carThoughPos.ToString()}, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcDriveSecond;
					}
					break;
				case State.NpcDriveSecond:
					Wait(500);
					DriveTo(ped, vehicle, carStopPos);
					Wait(500);
					if (vehicle.Position.DistanceTo(carStopPos) < 8.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, drive to second stop point, " +
													$"stop_position={carStopPos.ToString()}, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}.");
						curState = State.NpcExitAndWalk;
					}
					break;
				case State.NpcExitAndWalk:
					Wait(500);
					ped.Task.LeaveVehicle();
					Wait(500);
					ped.Task.GoTo(npcGenPos);
					Wait(5000);
					if (ped.Position.DistanceTo(npcGenPos) < 2.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, npc exit the car, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}.");
						curState = State.NpcDisappear;
					}
					break;
				case State.NpcDisappear:
					Wait(500);
					if (ped.Position.DistanceTo(npcGenPos) < 2.0f)
					{
						ped.Delete();
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc disappear, " +
												$"NPC:hash_name={_pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, disapper_position={npcGenPos.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcSpawn;
					}
					break;
				case State.NpcSpawn:
					Wait(500);
					PedHashSafe randomPed1 = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					_pedHash = (PedHash)randomPed1;
					ped = World.CreatePed(_pedHash, npcGenPos);
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate new npc, " +
												$"NPC:hash_name={_pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, generate_position={npcGenPos.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					curState = State.NpcWalkToVehicle;
					break;
				case State.NpcWalkToVehicle:
					Wait(500);
					ped.Task.GoTo(vehicle);
					Wait(5000);
					Vector3 carPos = vehicle.Position;
					if (ped.Position.DistanceTo(carPos) < 5.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc walk to vehicle, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcDriveBack;
					}
					
					break;
				case State.NpcDriveBack:
					Wait(500);
					DriveTo(ped, vehicle, carThoughPos2);
					Wait(10000);
					if (vehicle.Position.DistanceTo(carThoughPos2) < 8.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, " +
												$"npc drive car back stage 1, stop_position={carThoughPos2.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcDriveBack2;
					}
					break;
				case State.NpcDriveBack2:
					Wait(500);
					DriveTo(ped, vehicle, carEndPos);
					Wait(5000);
					if (vehicle.Position.DistanceTo(carEndPos) < 8.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}," +
												$" npc drive car back stage 2, stop_position={carEndPos.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.CleanupAndRestart;
					}
					break;
				case State.CleanupAndRestart:
					int totalSeconds = (int)timespan.TotalSeconds;
					Log.Message(Log.Level.Info, $"before:seconds={totalSeconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					int secondsToNext30Multiple = 30 - (totalSeconds % 30);
					if (secondsToNext30Multiple == 30)
					{
						secondsToNext30Multiple = 0;
					}
					Wait(secondsToNext30Multiple * 1000);
					timespan = DateTime.Now - startTime;
					int seconds = (int)timespan.TotalSeconds;

					vehicle.Delete();
					ped.Delete();
					
					Log.Message(Log.Level.Info, $"before:seconds={seconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Log.Message(Log.Level.Info, $"{this.GetType().Name}::CleanupAndRestart");
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, car and npc clean up and restart." +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					addExecuteTime();
					break;
			}
		}

		private void DriveTo(Ped ped, Vehicle vehicle, Vector3 destination)
		{
			if (!ped.IsInVehicle(vehicle))
			{
				ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);
			}
			else
			{
				ped.Task.DriveTo(vehicle, destination, 5.0f, 10f);
			}
		}


		private void addExecuteTime()
		{
			executeTime++;
			if (executeTime < loopTime)
			{
				curState = State.GenerateCarAndNpc;
			}
			else
			{
				isMissionSucceed = true;
			}
		}

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}
		

	}
}
