

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHVDN;
using GTA.Math;
using static SHVDN.NativeMemory;

namespace GTA
{
	[ScriptAttributes(NoDefaultInstance = true)]
	internal class event_npc_choose_car_loop : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GenerateCars,
			GenerateNpcs,
			NpcChooseCars,
			NpcEnterCars,
			NpcDriveAway,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;
		private List<Vehicle> carList = new List<Vehicle>();
		private List<Ped> npcList = new List<Ped>();
		private Ped ped;
		private Vehicle vehicle;

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 carGenPos = new Vector3(0, 0, 0);
		private Vector3 npcGenPos = new Vector3(0, 0, 0);
		private Vector3 carAwayPos = new Vector3(0, 0, 0);
		private TimeSpan timespan;
		private DateTime startTime;
		private bool timerStarted = false;
		private int totalNpcCount = 0;
		private int carCount = 0;
		private int npcCount = 0;
		private int loopTime = 0;
		private int executeTime = 0;
		private int createdNpcCount = 0;

		Random random = new Random();

		public event_npc_choose_car_loop()
		{
			Tick += OnTick;
		}

		private void LoadSettings()
		{
			string configPath = $"scripts\\{this.GetType().Name}.ini";

			if (System.IO.File.Exists(configPath))
			{
				ScriptSettings config = ScriptSettings.Load(configPath);
				loopTime = config.GetValue("Settings", "LoopTime", 1);
				carCount = config.GetValue("Settings", "CarCount", 3);
				npcCount = config.GetValue("Settings", "NpcCount", 2);
			}
			else
			{
				carCount = 3;
				npcCount = 2;
			}

			totalNpcCount = npcCount;
		}

		public override void load()
		{
			GTA.UI.Notification.Show($"Loading {this.GetType().Name}...");
			LoadSettings();
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -808, 158, 71);
			changePos(ref carGenPos, -836, 158, 67);
			changePos(ref npcGenPos, -834, 164, 67);
			changePos(ref carAwayPos, -817, 185, 71);
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
			
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;

			startTime = DateTime.Now;
			curState = State.Waiting;

			
		}

		public override void destroy()
		{
			foreach (Vehicle car in carList)
			{
				car.Delete();
			}
			carList.Clear();

			ped.Delete();
		}

		public override bool is_mission_finished()
		{
			return executeTime >= loopTime; // Only execute once for this event
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
						curState = State.GenerateCars;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.GenerateCars:
					Wait(500);
					GenerateCars(carCount);
					Wait(1000);
					curState = State.GenerateNpcs;
					break;
				case State.GenerateNpcs:
					Wait(500);
					PedHashSafe randomPed = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					PedHash pedHash = (PedHash)randomPed;
					ped = World.CreatePed(pedHash, npcGenPos);
					createdNpcCount++;
					Wait(500);
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate npc, " +
												$"NPC:hash_name={pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, generate_position={npcGenPos.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcChooseCars;
					break;
				case State.NpcChooseCars:
					Wait(500);
					int carIndex = random.Next(carList.Count);
					vehicle = carList[carIndex];
					VehicleHash vhash = (VehicleHash)vehicle.Model.GetHashCode();
					
					Wait(500);
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc choose vehicle, " +
												$"vehicle_{carIndex}:hash_name={vhash.ToString()}, hash_code={vehicle.Model.GetHashCode().ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcEnterCars;
					break;
				case State.NpcEnterCars:
					Wait(500);
					ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);
					Wait(1000);
					if (ped.IsInVehicle(vehicle))
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc enter vehicle, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcDriveAway;
					}
					break;
				case State.NpcDriveAway:
					Wait(500);
					ped.Task.DriveTo(vehicle, carAwayPos, 5.0f, 10f);
					if (ped.Position.DistanceTo(carAwayPos) < 8.0f)
					{
						if (createdNpcCount >= npcCount)
						{
							curState = State.CleanupAndRestart;
						}
						else
						{
							carList.Remove(vehicle);
							ped.Delete();
							vehicle.Delete();
							Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc drive away, " +
												$"end_position={carAwayPos.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
							curState = State.GenerateNpcs;
						}
						
					}
					break;
				case State.CleanupAndRestart:
					Wait(1000);
					timespan = DateTime.Now - startTime;
					int totalSeconds = (int)timespan.TotalSeconds;
					int secondsToNext30Multiple = 30 - (totalSeconds % 30);
					if (secondsToNext30Multiple == 30)
					{
						secondsToNext30Multiple = 0;
					}
					Wait(secondsToNext30Multiple * 1000);
					timespan = DateTime.Now - startTime;
					int seconds = (int)timespan.TotalSeconds;

					ped.Delete();
					vehicle.Delete();
					for (int i = 0; i < carList.Count; i++)
					{
						carList[i].Delete();
					}
					carList.Clear();
					createdNpcCount = 0;

					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc and vehicle clean up and restart." +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					addExecuteTime();
					break;
			}
		
		}

		private void GenerateCars(int count)
		{
			string carLog = "";
			float laneWidth = 3.0f; // 定义车道宽度
			float laneOffset = laneWidth / 2; // 从车道中心开始偏移
			for (int i = 0; i < count; i++)
			{
				Model model = new Model(VehicleHash.Adder); // You can change the vehicle hash as needed
				muscleHash randomVehicle = RandomEnumPicker.GetRandomEnumValue<muscleHash>();
				VehicleHash vhash = (VehicleHash)randomVehicle;

				float offsetX = laneOffset - (count / 2) * laneWidth + i * laneWidth;
				float offsetY = (float)(random.NextDouble() * 4.0 - 2.0);
				Vector3 vPos = new Vector3(carGenPos.X + offsetX, carGenPos.Y + offsetY, carGenPos.Z);

				Vehicle car = World.CreateVehicle(vhash, vPos);
				carList.Add(car);

				carLog += $"vehicle_{i}:hash_name={vhash.ToString()}, hash_code={car.Model.GetHashCode().ToString()}, generate_position={vPos.ToString()}, ";
			}

			Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate vehicles, " +
										carLog +
										$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
		}

		private void addExecuteTime()
		{
			executeTime++;
			if (executeTime < loopTime)
			{
				curState = State.GenerateCars;
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
