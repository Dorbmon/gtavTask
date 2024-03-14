using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Math;
using GTA.Native;

namespace GTA
{
	[ScriptAttributes(NoDefaultInstance = true)]
	internal class mission_cross_intersection : mission
	{
		private bool isMissionSucceed = false;
		private Vehicle vehicle;
		private Blip vehicleBlip;
		private Vector3 intersectPos = new Vector3(1, 1, 1);
		private Vector3 endPos = new Vector3(1, 1, 1);
		public mission_cross_intersection()
		{
			Tick += OnTick;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_cross_intersection...");

			setIntersectionPos(13, -1618, 29);
			setEndPos(-40, -2570, 29);
			Game.Player.Character.Position = intersectPos;
			vehicle = World.CreateVehicle(VehicleHash.BestiaGTS, new Vector3(11, -1620, 29));
			vehicleBlip = vehicle.AddBlip();
			vehicleBlip.Color = BlipColor.Blue;
			

			GTA.UI.Notification.Show("You have been teleported to the intersection. Get in the vehicle and follow the rules.");

		}

		public override void destroy()
		{
			if (vehicle != null)
			{
				vehicle.Delete();
			}
		}

		public override bool is_mission_finished()
		{
			return isMissionSucceed;
		}

		public void setIntersectionPos(float x, float y, float z)
		{
			intersectPos = new Vector3(x, y, z);
		}

		public void setEndPos(float x, float y, float z)
		{
			endPos = new Vector3(x, y, z);
		}

		//private bool IsTrafficLightGreen()
		//{
		//	return Function.Call<bool>(Hash.IS_VEHICLE_STOPPED_AT_TRAFFIC_LIGHTS && Has, vehicle);
		//}

		public void showPlayerPos()
		{
			Vector3 playerPosition = Game.Player.Character.Position;
			GTA.UI.Screen.ShowSubtitle($"Player Position: {playerPosition.X}, {playerPosition.Y}, {playerPosition.Z}");
		}

		public override void OnTick(object sender, EventArgs e)
		{
			if (vehicle != null && Game.Player.Character.IsInVehicle(vehicle))
			{
				bool hasCollisions = Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, vehicle.Handle);

				if (!hasCollisions && Game.Player.Character.Position.DistanceTo(endPos) < 5)
				{
					isMissionSucceed = true;
				}
				else
				{
					GTA.UI.Notification.Show("DROVE DANGEROUSLY! ");
				}
			}

			
		}
	}
}
