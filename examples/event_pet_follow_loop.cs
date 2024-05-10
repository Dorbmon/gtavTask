using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHVDN;
using System.Windows.Forms;
using GTA.Math;

namespace GTA
{ 

	[ScriptAttributes(NoDefaultInstance = true)]
	internal class event_pet_follow_loop : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GeneratePetAndNpc,
			NpcWalkToPet,
			NpcLetPetFollow,
			NpcWalkAway,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle;
		private Ped ped;
		private List<Ped> petList = new List<Ped>();

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 npcGenPos = new Vector3(0, 0, 0);
		private Vector3 npcStopPos = new Vector3(0, 0, 0);
		private Vector3 npcDisPos = new Vector3(0, 0, 0);
		private Vector3 petGenPos = new Vector3(0, 0, 0);
		
		private PedHash _pedHash = new PedHash();
		private List<PedHash> _petHashList = new List<PedHash>();
		private TimeSpan timespan;
		private DateTime startTime;
		private bool isPaused = false;
		private bool timerStarted = false;
		private int loopTime = 0;
		private int executeTime = 0;
		private int petNum = 0;
		Random random = new Random();
		public event_pet_follow_loop()
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
				petNum = config.GetValue("Settings", "PetNum", 1);
			}
			else
			{
				loopTime = 1;
				petNum = 3;
			}
		}

		public override void load()
		{
			GTA.UI.Notification.Show($"load {this.GetType().Name}...");
			LoadSettings();
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -808, 158, 71);
			changePos(ref petGenPos, -820, 164, 71);
			changePos(ref npcGenPos, -810, 164, 71);
			changePos(ref npcStopPos, -816, 165, 71);
			changePos(ref npcDisPos, -819, 175, 71);

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

		public override void destroy()
		{
			ped.Delete();
			for (int i = 0; i < petList.Count; i++)
			{
				petList[i].Delete();
			}
			petList.Clear();
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
						curState = State.GeneratePetAndNpc;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.GeneratePetAndNpc:
					Wait(500);
					PedHashSafe randomPed = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					_pedHash = (PedHash)randomPed;
					ped = World.CreatePed(_pedHash, npcGenPos);

					string petlog = "";
					for(int i = 0; i < petNum; i++)
					{
						AnimalHash randPet = RandomEnumPicker.GetRandomEnumValue<AnimalHash>();
						PedHash petHash = (PedHash)randPet;
						
						float offsetX = (float)(random.NextDouble() * 6.0 - 3.0);
						float offsetY = (float)(random.NextDouble() * 6.0 - 3.0);
						Vector3 petPosition = new Vector3(petGenPos.X + offsetX, petGenPos.Y + offsetY, petGenPos.Z);

						Ped pet = World.CreatePed(petHash, petPosition);
						if (pet.Exists()) petList.Add(pet);
						petlog += $"pet_{i}:hash_name={petHash.ToString()}, hash_code={pet.Model.GetHashCode().ToString()}, generate_position={petPosition.ToString()}, ";
					}
					
					
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate pet and npc, " +
												$"NPC:hash_name={_pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, generate_position={npcGenPos.ToString()}, " +
												petlog +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcWalkToPet;
					break;
				case State.NpcWalkToPet:
					Wait(500);
					ped.Task.GoTo(petGenPos);
					if (ped.Position.DistanceTo(petGenPos) < 2.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc walk to pets, " +
													$"stop_position={petGenPos.ToString()}, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcLetPetFollow;
					}
					break;
				case State.NpcLetPetFollow:
					Wait(500);
					int index = random.Next(petList.Count);
					Ped selectedPet = petList[index];
					Wait(1000);
					if (selectedPet != null && selectedPet.Exists())
					{
						selectedPet.Task.FollowToOffsetFromEntity(ped, new Vector3(0.2f, 0.3f, 0.0f), 3.0f);
						PedHash petHash = (PedHash)selectedPet.Model.GetHashCode();
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc let one pet follow, " +
													$"pet_{index}:hash_name={petHash.ToString()}, hash_code={selectedPet.Model.GetHashCode().ToString()}, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcWalkAway;
					}
					break;
				case State.NpcWalkAway:
					Wait(500);
					ped.Task.GoTo(npcDisPos);
					if (ped.Position.DistanceTo(npcDisPos) < 2.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc walk away with pet, " +
													$"stop_position={npcDisPos.ToString()}, " +
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

					for (int i = 0; i < petNum; i++)
					{
						petList[i].Delete();
					}
					ped.Delete();
					petList.Clear();

					Log.Message(Log.Level.Info, $"before:seconds={seconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Log.Message(Log.Level.Info, $"{this.GetType().Name}::CleanupAndRestart");
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, pet and npc clean up and restart." +
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
				curState = State.GeneratePetAndNpc;
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
