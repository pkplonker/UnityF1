using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenF1CSharp;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Driver : MonoBehaviour, IUpdateable
{
	public DriverData driverData;
	private List<LocationData> locationData;
	private List<LapData> lapData;
	private int lastIndex;

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

			var rawLocationData =await OpenF1QueryManager.Instance.Get(new LocationQuery()
				.Filter(nameof(LocationData.DriverNumber), driver.DriverNumber)
				.Filter(nameof(LocationData.SessionKey), driver.SessionKey)
				.GenerateQuery());
			locationData = JsonConvert.DeserializeObject<List<LocationData>>(rawLocationData);
			var filteredLocationData = locationData
				.Where(x => x.Date.HasValue && x.Date.Value > sessionData.DateStart.Value)
				.ToList();

			// Sort the filtered list by Date
			var sortedFilteredLocationData = filteredLocationData
				.OrderBy(x => x.Date.Value)
				.ToList();

			PostProcessLocationData(sortedFilteredLocationData);
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to get data for {driver.BroadcastName} {driver.DriverNumber} {e.Message}");
			MainThreadDispatcher.Instance.Enqueue(() => Destroy(gameObject));
		}
	}

	private void PostProcessLocationData(List<LocationData> sortedFilteredLocationData)
	{
		// for (var i = 0; i < sortedFilteredLocationData.Count; i++)
		// {
		// 	var data1 = sortedFilteredLocationData[i];
		// 	data1.Date = sortedFilteredLocationData[i].Date.Value.AddHours(-1);
		// 	sortedFilteredLocationData[i] = data1;
		// }

		locationData = sortedFilteredLocationData.OrderBy(x => x.Date.Value).ToList();
	}

	public void Tick(DateTime currentTime)
	{
		var count = locationData.Count;
		while (locationData[lastIndex].Date <= currentTime && lastIndex < count)
		{
			var element = locationData[lastIndex];
			transform.position = new Vector3(element.X.Value, 0, element.Y.Value);
			lastIndex++;
		}
	}
}