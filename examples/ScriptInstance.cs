using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

namespace ScriptInstance
{
	
	public class RSeg
	{
		public String model;
		public List<(float, float)> points;
		public RSeg(string model, List<(float, float)> points)
		{
			this.model = model;
			this.points = points;
		}
	}
	// Main script is auto-started and creates AI scripts using key presses.
	// T key to spawn AIone
	// Y key to spawn AItwo
	// G key to change AIone animation 
	// H key to SetWait(6) for AItwo
	// J key to pause AIone
	public class Main : Script
	{
		private AI AIone = null;
		private AI AItwo = null;

		public Main()
		{
			KeyDown += OnKeyDown;

			Interval = 1000;
		
		}
		private void pause()
		{
			Game.Pause(true);
		}
		private void unpause()
		{
			Game.Pause(false);
		}
		private void setVisibility(bool visible, int type)
		{
			int alpha_level;
			if (visible) alpha_level = 255;
			else alpha_level = 0;
			var props = World.GetAllProps();
			bool unkb = true;
			switch (type)
			{
				case (1):
					{
						foreach (var i in props)
						{
							if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, i))
							{
								GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_VISIBLE, i, visible, unkb);
							}

							if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_ON_SCREEN, i))
							{
								Vector2 p;
								//World3DToScreen2D(i.Position, out p);
								//GTA.UI.Notification.Show("get 2d:" + p.ToString());
							}
						}

						break;
					}
				case (2):
					{
						var peds = World.GetAllPeds();
						foreach (var i in peds)
						{
							if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, i))
							{
								GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_VISIBLE, i, visible, unkb);
							}
						}

						break;
					}
				case (3):
					{
						var cars = World.GetAllVehicles();
						foreach (var i in cars)
						{
							if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, i))
							{
								GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_VISIBLE, i.Handle, visible, unkb);
								//GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_ALPHA, i, alpha_level, true);
							}
						}

						break;
					}
			}
			// start to build segmentation image

		}
		private void setVisibility(bool visible)
		{
			int alpha_level;
			if (visible) alpha_level = 255;
			else alpha_level = 0;
			var props = World.GetAllProps();
			bool unkb = true;
			foreach (var i in props)
			{
				if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, i))
				{
					GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_VISIBLE, i, visible, unkb);
				}
				if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_ON_SCREEN, i))
				{
					Vector2 p;
					//World3DToScreen2D(i.Position, out p);
					//GTA.UI.Notification.Show("get 2d:" + p.ToString());
				}
			}

			var peds = World.GetAllPeds();
			foreach (var i in peds)
			{
				if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, i))
				{
					GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_VISIBLE, i, visible, unkb);
				}
			}

			var cars = World.GetAllVehicles();
			foreach (var i in cars)
			{
				if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, i))
				{
					GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_VISIBLE, i.Handle, visible, unkb);
					//GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_ALPHA, i, alpha_level, true);
				}
			}

			// start to build segmentation image
			
		}
		public static void Cal()
		{
			const float percent = 0.1f;
			float seg_x = 1 / percent;
			float seg_y = 1 / percent;
			Dictionary<String, RSeg> map = new Dictionary<String, RSeg>();
			Dictionary<String, String> hex_link =  new Dictionary<String, String>();
			Dictionary<String, bool> used = new Dictionary<string, bool>();
			Random rnd = new Random();
			String fName = String.Format("test/rx_{0}.txt", rnd.Next().ToString());
			StreamWriter f = new StreamWriter(fName);
			List<String> hexs = new List<String>{ "#FFB6C1", "#FFC0CB", "#DC143C", "#FFF0F5", "#DB7093", "#FF69B4", "#FF1493", "#C71585", "#DA70D6", "#D8BFD8", "#DDA0DD", "#EE82EE", "#FF00FF", "#FF00FF", "#8B008B", "#800080", "#BA55D3", "#9400D3", "#9932CC", "#4B0082", "#8A2BE2", "#9370DB", "#7B68EE", "#6A5ACD", "#483D8B", "#E6E6FA", "#F8F8FF", "#0000FF", "#0000CD", "#191970", "#00008B", "#000080", "#4169E1", "#6495ED", "#B0C4DE", "#778899", "#708090", "#1E90FF", "#F0F8FF", "#4682B4", "#87CEFA", "#87CEEB", "#00BFFF", "#ADD8E6", "#B0E0E6", "#5F9EA0", "#F0FFFF", "#E1FFFF", "#AFEEEE", "#00FFFF", "#D4F2E7", "#00CED1", "#2F4F4F", "#008B8B", "#008080", "#48D1CC", "#20B2AA", "#40E0D0", "#7FFFAA", "#00FA9A", "#00FF7F", "#F5FFFA", "#3CB371", "#2E8B57", "#F0FFF0", "#90EE90", "#98FB98", "#8FBC8F", "#32CD32", "#00FF00", "#228B22", "#008000", "#006400", "#7FFF00", "#7CFC00", "#ADFF2F", "#556B2F", "#F5F5DC", "#FAFAD2", "#FFFFF0", "#FFFFE0", "#FFFF00", "#808000", "#BDB76B", "#FFFACD", "#FFE4B5", "#FFA500", "#FFEFD5", "#FFEBCD", "#FFDEAD", "#FAEBD7", "#D2B48C", "#DEB887", "#FFE4C4", "#FF8C00", "#FAF0E6", "#CD853F", "#FFA07A", "#FF7F50", "#FF4500", "#E9967A", "#FF6347", "#FFE4E1", "#FA8072", "#FFFAFA", "#F08080", "#BC8F8F", "#CD5C5C", "#FF0000", "#A52A2A", "#B22222", "#8B0000", "#800000", "#FFFFFF", "#F5F5F5", "#DCDCDC", "#D3D3D3", "#C0C0C0", "#A9A9A9", "#808080", "#696969", "#000000" };
			//used.Add("#DC143C", true);
			float x_l = 540;
			float y_l = 960;
			for (float x = 0; x < x_l; x+= 1)
			{
				
				for (float y = 0; y < y_l; y += 1)
				{
					//GTA.UI.Notification.Show("done:" + x.ToString() + "," + y.ToString());
					String label;
					
					Vector3 dir;
					Vector3 cam3DPos = ScreenRelToWorld(GameplayCamera.Position, GameplayCamera.Rotation, new Vector2(x / x_l, y / y_l), out dir);
					RaycastResult r = World.Raycast(cam3DPos, dir, 200f, GTA.IntersectFlags.BoundingBox, Game.Player.Character);
					label = "#000000";
					if (r.DidHit && r.HitEntity != null)
					{
						//r.HitEntity.EntityType.ToString();
						var hash = r.HitEntity.Model.Hash.ToString();//r.MaterialHash.ToString();//
						if (hex_link.ContainsKey(hash))
						{
							label = hex_link[hash];
						} else
						{
						r:
							//GTA.UI.Notification.Show("done:" + x.ToString() + "," + y.ToString());
							label = hexs[rnd.Next(hexs.Count - 1)];
							if (used.ContainsKey(label))
							{
								goto r;
							}
							used[label] = true;
							hex_link[hash] = label;
						}

						//if (!map.ContainsKey(hash))
						//{
						//	map.Add(hash, new RSeg(r.HitEntity.Model.Hash.ToString(), new List<(float,float)>()));
						//}
						//map[hash].points.Add((x,y));
						int argb = Int32.Parse(label.Replace("#", ""), NumberStyles.HexNumber);
						Color clr = Color.FromArgb(argb);
						//World.DrawMarker(MarkerType.DebugSphere, r.HitPosition, Vector3.Zero, Vector3.Zero, new Vector3(0.2f, 0.2f, 0.2f), clr);
					}
					f.Write(label + ",");
				}
			}
			GTA.UI.Notification.Show("done:");
			GTA.UI.Notification.Show(fName);
			f.Close();
		}
		public static float DegToRad(float _deg)
		{
			double Radian = (Math.PI / 180) * _deg;
			return (float)Radian;
		}

		public static Vector3 RotationToDirection(Vector3 rotation)
		{
			var z = DegToRad(rotation.Z);
			var x = DegToRad(rotation.X);
			var num = Math.Abs(Math.Cos(x));
			return new Vector3
			{
				X = (float)(-Math.Sin(z) * num),
				Y = (float)(Math.Cos(z) * num),
				Z = (float)Math.Sin(x)
			};
		}
		public static Vector3 ScreenRelToWorld(Vector3 camPos, Vector3 camRot, Vector2 coord, out Vector3 forwardDirection)
		{
			var camForward = RotationToDirection(camRot);
			var rotUp = camRot + new Vector3(1, 0, 0);
			var rotDown = camRot + new Vector3(-1, 0, 0);
			var rotLeft = camRot + new Vector3(0, 0, -1);
			var rotRight = camRot + new Vector3(0, 0, 1);

			var camRight = RotationToDirection(rotRight) - RotationToDirection(rotLeft);
			var camUp = RotationToDirection(rotUp) - RotationToDirection(rotDown);

			var rollRad = -DegToRad(camRot.Y);

			var camRightRoll = camRight * (float)Math.Cos(rollRad) - camUp * (float)Math.Sin(rollRad);
			var camUpRoll = camRight * (float)Math.Sin(rollRad) + camUp * (float)Math.Cos(rollRad);

			var point3D = camPos + camForward * 1.0f + camRightRoll + camUpRoll;
			Vector2 point2D;
			if (!World3DToScreen2D(point3D, out point2D))
			{
				forwardDirection = camForward;
				return camPos + camForward * 1.0f;
			}
			var point3DZero = camPos + camForward * 1.0f;
			Vector2 point2DZero;
			if (!World3DToScreen2D(point3DZero, out point2DZero))
			{
				forwardDirection = camForward;
				return camPos + camForward * 1.0f;
			}

			const double eps = 0.001;
			if (Math.Abs(point2D.X - point2DZero.X) < eps || Math.Abs(point2D.Y - point2DZero.Y) < eps)
			{
				forwardDirection = camForward;
				return camPos + camForward * 1.0f;
			}
			var scaleX = (coord.X - point2DZero.X) / (point2D.X - point2DZero.X);
			var scaleY = (coord.Y - point2DZero.Y) / (point2D.Y - point2DZero.Y);
			var point3Dret = camPos + camForward * 1.0f + camRightRoll * scaleX + camUpRoll * scaleY;
			forwardDirection = camForward + camRightRoll * scaleX + camUpRoll * scaleY;
			return point3Dret;
		}
		public static bool World3DToScreen2D(Vector3 pos, out Vector2 result)
		{
			var x2dp = new GTA.Native.OutputArgument();
			var y2dp = new GTA.Native.OutputArgument();

			bool success = GTA.Native.Function.Call<bool>((GTA.Native.Hash)0x34E82F05DF2974F5, pos.X, pos.Y, pos.Z, x2dp, y2dp); //  GET_SCREEN_COORD_FROM_WORLD_COORD and previously, _WORLD3D_TO_SCREEN2D
			result = new Vector2(x2dp.GetResult<float>(), y2dp.GetResult<float>());
			return success;
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.T)
			{
				recordData();
			}
			if (e.KeyCode == Keys.R)
			{
				unpause();
			}
			if (e.KeyCode == Keys.U)
			{
				GTA.Native.Function.Call((GTA.Native.Hash.SET_MISSION_NAME), true, "test");
			}
			if (e.KeyCode == Keys.I)
			{
				InstantiateScript<mission01>();
			}
			switch (e.KeyCode)
			{
				case (Keys.NumPad1):
					{
						setVisibility(false, 1);
						break;
					}
				case (Keys.NumPad2):
					{
						setVisibility(false, 2);
						break;
					}
				case (Keys.NumPad3):
					{
						setVisibility(false, 3);
						break;
					}
				case (Keys.NumPad4):
					{
						setVisibility(true, 1);
						break;
					}
				case (Keys.NumPad5):
					{
						setVisibility(true, 2);
						break;
					}
				case (Keys.NumPad6):
					{
						setVisibility(true, 3);
						break;
					}
			}
		}
		
		private void recordData()
		{
			pause();
			try
			{
				Cal();
			} catch(Exception ex)
			{
				GTA.UI.Notification.Show("exp:");
				StreamWriter f = new StreamWriter("test/exp.txt");
				f.WriteLine(ex.ToString());
				f.Close();
			}
			unpause();
			//setVisibility(false);
			//GTA.Native.Function.Call(GTA.Native.Hash.TOGGLE_PAUSED_RENDERPHASES, true);	
			//setVisibility(true);
			//unpause();
			//GTA.World.Raycast()
			
		}
	}

	[ScriptAttributes(NoDefaultInstance = true)]
	public class AI : Script
	{
		public AI()
		{
			Tick += OnTick;
			Aborted += OnShutdown;

			Interval = 3000;
		}

		private Ped ped = null;
		private int wait = -1;
		public string animation = "HandsUp";

		public void SetWait(int ms)
		{
			if (ms > wait)
			{
				wait = ms;
			}
		}

		private void OnTick(object sender, EventArgs e)
		{
			if (wait > -1)
			{
				Wait(wait);
				wait = -1;
			}

			if (ped == null)
			{
				ped = World.CreatePed(PedHash.Beach01AMY, Game.LocalPlayerPed.Position + (GTA.Math.Vector3.RelativeFront * 3));
			}

			// Repeat animation if alive
			if (ped != null && ped.IsAlive)
			{
				if (animation == "HandsUp")
				{
					ped.Task.HandsUp(1000);
				}
				else if (animation == "Jump")
				{
					ped.Task.Jump();
				}
			}
		}

		private void OnShutdown(object sender, EventArgs e)
		{
			// Clear pedestrian on script abort
			ped?.Delete();
		}
	}
	
}
