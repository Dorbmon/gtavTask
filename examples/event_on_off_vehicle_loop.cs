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
	internal class event_on_off_vehicle_loop : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GenerateCarAndNpc,
			NpcGetOnCar,
			Npc2GetOffCar,
			Npc2WalkToCar1,
			Npc2GetOnCar1,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle1;
		private Vehicle vehicle2;
		private Ped ped1;
		private Ped ped2;

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 carGenPos1 = new Vector3(0, 0, 0);
		private Vector3 carGenPos2 = new Vector3(0, 0, 0);
		
		private PedHash _pedHash1 = new PedHash();
		private PedHash _pedHash2 = new PedHash();
		private VehicleHash _vehicleHash1 = new VehicleHash();
		private VehicleHash _vehicleHash2 = new VehicleHash();

		private TimeSpan timespan;
		private DateTime startTime;
		private bool isPaused = false;
		private bool timerStarted = false;
		private int loopTime = 0;
		private int executeTime = 0;

		public event_on_off_vehicle_loop()
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
			changePos(ref carGenPos1, -839, 156, 67);
			changePos(ref carGenPos2, -830, 158, 68);
			
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

					PedHashSafe randomPed1 = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					PedHashSafe randomPed2 = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					muscleHash randomVehicle1 = RandomEnumPicker.GetRandomEnumValue<muscleHash>();
					muscleHash randomVehicle2 = RandomEnumPicker.GetRandomEnumValue<muscleHash>();
					_vehicleHash1 = (VehicleHash)randomVehicle1;
					_vehicleHash2 = (VehicleHash)randomVehicle2;
					_pedHash1 = (PedHash)randomPed1;
					_pedHash2 = (PedHash)randomPed2;
					vehicle1 = World.CreateVehicle(_vehicleHash1, carGenPos1);
					vehicle2 = World.CreateVehicle(_vehicleHash2, carGenPos2);
					Log.Message(Log.Level.Info, $"car1:hash_name={_vehicleHash1.ToString()}, car2:hash_name={_vehicleHash2.ToString()} ");
					Wait(500);
					Random random = new Random();
					float offsetX = (float)(random.NextDouble() * 10.0 - 5.0); 
					float offsetY = (float)(random.NextDouble() * 10.0 - 5.0); 
					Vector3 pedPosition1 = new Vector3(carGenPos1.X + offsetX, carGenPos1.Y + offsetY, carGenPos1.Z);
					Vector3 pedPosition2 = new Vector3(carGenPos2.X + offsetX, carGenPos2.Y + offsetY, carGenPos2.Z);
					ped1 = World.CreatePed(_pedHash1, pedPosition1);
					ped2 = World.CreatePed(_pedHash2, pedPosition2);
					Log.Message(Log.Level.Info, $"NPC1:hash_name={_pedHash1.ToString()}, NPC2:hash_name={_pedHash2.ToString()},  ");
					
					Wait(500);
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate car and npc, " +
												$"car1:hash_name={_vehicleHash1.ToString()}, hash_code={vehicle1.Model.GetHashCode().ToString()}, generate_position={carGenPos1.ToString()}, " +
												$"primary_color={vehicle1.Mods.PrimaryColor.ToString()}, secondary_color={vehicle1.Mods.SecondaryColor.ToString()}, " +
												$"car2:hash_name={_vehicleHash2.ToString()}, hash_code={vehicle2.Model.GetHashCode().ToString()}, generate_position={carGenPos2.ToString()}, " +
												$"primary_color={vehicle2.Mods.PrimaryColor.ToString()}, secondary_color={vehicle2.Mods.SecondaryColor.ToString()}, " +
												$"NPC1:hash_name={_pedHash1.ToString()}, hash_code={ped1.Model.GetHashCode()}, generate_position={pedPosition1.ToString()}, " +
												$"NPC2:hash_name={_pedHash2.ToString()}, hash_code={ped2.Model.GetHashCode()}, generate_position={pedPosition2.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcGetOnCar;
					break;
				case State.NpcGetOnCar:
					Wait(500);
					ped1.Task.EnterVehicle(vehicle1, VehicleSeat.Driver);
					ped2.Task.EnterVehicle(vehicle2, VehicleSeat.Driver);
					Wait(5000);
					if (ped1.IsInVehicle(vehicle1) && ped2.IsInVehicle(vehicle2))
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc1 get in vehicle1, npc2 get in vehicle2" +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.Npc2GetOffCar;
					}
					break;
				case State.Npc2GetOffCar:
					Wait(500);
					ped2.Task.LeaveVehicle();
					Wait(500);
					if (!ped2.IsInVehicle(vehicle2))
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, npc2 get off vehicle2, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}.");
						curState = State.Npc2WalkToCar1;
					}
					break;
				case State.Npc2WalkToCar1:
					Wait(500);
					ped2.Task.GoTo(carGenPos1);
					Wait(2000);
					if (ped2.Position.DistanceTo(carGenPos1) < 5.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, npc2 walk to vehicle1, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}.");
						curState = State.Npc2GetOnCar1;
					}
					break;
				case State.Npc2GetOnCar1:
					Wait(500);
					ped2.Task.EnterVehicle(vehicle1, VehicleSeat.RightFront);
					Wait(5000);
					if (ped2.IsInVehicle(vehicle1))
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc2 get on vehicle2, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.CleanupAndRestart;
					}
					break;
				case State.CleanupAndRestart:
					Wait(1000);
					timespan = DateTime.Now - startTime;
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

					vehicle1.Delete();
					vehicle2.Delete();
					ped1.Delete();
					ped2.Delete();
					
					Log.Message(Log.Level.Info, $"after:seconds={seconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Log.Message(Log.Level.Info, $"{this.GetType().Name}::CleanupAndRestart");
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, car and npc clean up and restart." +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					addExecuteTime();
					break;
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
