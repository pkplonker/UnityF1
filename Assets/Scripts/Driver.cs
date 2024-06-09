using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenF1CSharp;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DefaultNamespace
{
	public class Driver : MonoBehaviour
	{
		public DriverData driverData;
		private OpenF1Reader openF1Reader;
		private List<LocationData> locationData;
		private List<LapData> lapData;
		private int index;

		public async Task Init(List<LapData> lapData, DriverData driver, SessionData sessionData)
		{
			try
			{
				this.driverData = driver;
				this.lapData = lapData.Where(x => x.DriverNumber == driver.DriverNumber).ToList();
				MainThreadDispatcher.Instance.Enqueue(() =>
				{
					var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					go.transform.SetParent(transform);
					go.transform.localScale = new Vector3(250, 250, 250);
					var mr = go.GetComponent<MeshRenderer>();
					mr.materials[0].color = driver.TeamColour.HexToColor();
				});

				openF1Reader = new OpenF1Reader();

				var rawLocationData = await openF1Reader.Query(new LocationQuery()
					.Filter(nameof(LocationData.DriverNumber), driver.DriverNumber)
					.Filter(nameof(LocationData.SessionKey), driver.SessionKey)
					.GenerateQuery());
				locationData = JsonConvert.DeserializeObject<List<LocationData>>(rawLocationData);
				locationData = locationData.Where(x => x.Date.Value > sessionData.DateStart.Value).ToList();
				
				PostProcessLocationData();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to get data for {driver.BroadcastName} {driver.DriverNumber} {e.Message}");
				MainThreadDispatcher.Instance.Enqueue(() => Destroy(gameObject));
			}
		}

		private void PostProcessLocationData()
		{
			for (var i = 0; i < locationData.Count; i++)
			{
				var data1 = locationData[i];
				data1.Date = locationData[i].Date.Value.AddHours(-1);
				locationData[i] = data1;
			}

			locationData = locationData.OrderBy(x => x.Date.Value).ToList();
		}

		public void Tick()
		{
			if (index < locationData?.Count())
			{
				var element = locationData[index];
				transform.position = new Vector3(element.X.Value, 0, element.Y.Value);
				index++;
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}