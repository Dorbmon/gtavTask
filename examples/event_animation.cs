using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHVDN;
using static GTA.missionManager;
using GTA.Math;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Forms;
using System.IO;

namespace GTA
{
	internal class event_animation : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GenerateNpc,
			NpcMoveIn,
			NpcAnimate,
			NpcMoveOut,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;
		private static List<Tuple<string, string>> animationList = new List<Tuple<string, string>>();
		private Ped ped;

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 pedGenPos = new Vector3(0, 0, 0);
		private Vector3 pedStopPos = new Vector3(0, 0, 0);
		private Vector3 pedEndPos = new Vector3(0, 0, 0);
		private PedHash _pedHash = new PedHash();

		private TimeSpan timespan;
		private DateTime startTime;
		private bool isPaused = false;
		private bool timerStarted = false;
		private int loopTime = 0;
		private int executeTime = 0;

		public event_animation()
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
			changePos(ref pedGenPos, -826, 154, 69);
			changePos(ref pedStopPos, -839, 156, 67);
			changePos(ref pedEndPos, -842, 162, 68);
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;
			Game.Player.Character.Position = playerPos;

			animationList = LoadAnimations($"scripts\\animations.txt"); // 假设你的文件名是 animations.txt
			Log.Message(Log.Level.Info, $"{this.GetType().Name}::{curState.ToString()}, animationList_size={animationList.Count.ToString()}");

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
			animationList.Clear();
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
						curState = State.GenerateNpc;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.GenerateNpc:
					Wait(500);

					PedHashSafe randomPed1 = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					_pedHash = (PedHash)randomPed1;
					ped = World.CreatePed(_pedHash, pedGenPos);
					Wait(500);
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate npc, " +
												$"NPC:hash_name={_pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, generate_position={pedGenPos.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcMoveIn;
					break;
				case State.NpcMoveIn:
					Wait(500);
					ped.Task.RunTo(pedStopPos);
					if (ped.Position.DistanceTo(pedStopPos) < 5.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc move in, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.NpcAnimate;
					}
					break;
				case State.NpcAnimate:
					Wait(500);
					for (int i = 0; i < 6; i++)
					{
						var randomAnimation = GetRandomAnimation(animationList);
						ped.Task.PlayAnimation(randomAnimation.Item1, randomAnimation.Item2, 1.0f, 1.0f, -1, AnimationFlags.Loop, 0.5f);
						Wait(5 * 1000);
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc do animation, " +
													$"NPC:hash_name={_pedHash.ToString()}, hash_code={ped.Model.GetHashCode()}, " +
													$"anim_dict={randomAnimation.Item1.ToString()}, anim_name={randomAnimation.Item2.ToString()}, " +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						Wait(1000);
					}
					curState = State.NpcMoveOut;
					break;
				case State.NpcMoveOut:
					Wait(500);
					ped.Task.RunTo(pedEndPos);
					if (ped.Position.DistanceTo(pedEndPos) < 5.0f)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc move out, " +
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

					ped.Delete();

					Log.Message(Log.Level.Info, $"after:seconds={seconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Log.Message(Log.Level.Info, $"{this.GetType().Name}::CleanupAndRestart");
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc clean up and restart." +
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
				curState = State.GenerateNpc;
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

		static List<Tuple<string, string>> LoadAnimations(string filePath)
		{
			var animations = new List<Tuple<string, string>>();
			foreach (var line in File.ReadAllLines(filePath))
			{
				var parts = line.Split(' ');
				if (parts.Length == 2)
				{
					animations.Add(new Tuple<string, string>(parts[0], parts[1]));
				}
			}
			return animations;
		}

		static Tuple<string, string> GetRandomAnimation(List<Tuple<string, string>> animations)
		{
			Random random = new Random();
			int index = random.Next(animations.Count);
			return animations[index];
		}
	}
}
