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
	public DriverData DriverData;
	private List<LocationData> locationData;
	private List<LapData> lapData;
	private int lastLocationIndex;
	public int CurrentLap = 0;
	private List<IntervalData> intervalData;
	private int lastIntervalIndex;
	private List<PositionData> positionData;
	private int lastPositionIndex;
	public int Position { get; private set; }
	public string Interval { get; private set; }
	public string Tyre { get; private set; }
	public string GapToLeader { get; private set; } = "0";
	public static event Action IntervalsUpdated; 
	public async Task Init(List<LapData> lapData, List<IntervalData> intervalData, List<PositionData> positionData,
		DriverData driver,
		SessionData sessionData)
	{
		try
		{
			this.positionData = positionData;
			this.Position = this.positionData.First().Position.Value;
			this.intervalData = intervalData;
			this.DriverData = driver;
			this.lapData = lapData.Where(x => x.DriverNumber == driver.DriverNumber).ToList();
			MainThreadDispatcher.Instance.Enqueue(() =>
			{
				var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				go.transform.SetParent(transform);
				go.transform.localScale = new Vector3(250, 250, 250);
				var mr = go.GetComponent<MeshRenderer>();
				mr.materials[0].color = driver.TeamColour.HexToColor();
			});

			await GenerateLocationData(driver, sessionData);
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to get data for {driver.BroadcastName} {driver.DriverNumber} {e.Message}");
			MainThreadDispatcher.Instance.Enqueue(() => Destroy(gameObject));
		}
	}

	private async Task GenerateLocationData(DriverData driver, SessionData sessionData)
	{
		var rawLocationData = await OpenF1QueryManager.Instance.Get(new LocationQuery()
			.Filter(nameof(LocationData.DriverNumber), driver.DriverNumber)
			.Filter(nameof(LocationData.SessionKey), driver.SessionKey)
			.GenerateQuery());
		locationData = JsonConvert.DeserializeObject<List<LocationData>>(rawLocationData);
		
		var filteredLocationData = locationData
			.Where(x => x.Date.HasValue && x.Date.Value > sessionData.DateStart.Value)
			.ToList();

		var sortedFilteredLocationData = filteredLocationData
			.OrderBy(x => x.Date.Value)
			.ToList();

		locationData = sortedFilteredLocationData.OrderBy(x => x.Date.Value).ToList();
	}

	public bool Tick(DateTime currentTime)
	{
		var locationCount = locationData.Count;
		if (lastLocationIndex >= locationCount - 1) return false;
		while (locationData[lastLocationIndex].Date <= currentTime && lastLocationIndex < locationCount - 1)
		{
			var element = locationData[lastLocationIndex];
			transform.position = new Vector3(element.X.Value, 0, element.Y.Value);
			lastLocationIndex++;
		}
		
		var intervalCount = intervalData.Count;
		bool intervalUpdated = false;
		while (intervalData[lastIntervalIndex].Date <= currentTime && lastIntervalIndex < intervalCount - 1)
		{
			var element = intervalData[lastIntervalIndex];
			Interval = element.Interval;
			GapToLeader = element.GapToLeader;

			lastIntervalIndex++;
			intervalUpdated = true;
		}
		
		var positionCount = positionData.Count;
		while (positionData[lastPositionIndex].Date <= currentTime && lastPositionIndex < positionCount - 1)
		{
			Position = positionData[lastPositionIndex].Position.Value;
			lastPositionIndex++;
		}

		if (intervalUpdated)
		{
			IntervalsUpdated?.Invoke();
		}
		var currentLap = lapData.LastOrDefault(x => x.DateStart < locationData[lastLocationIndex].Date);
		CurrentLap = currentLap.LapNumber ?? 0;
		return true;
	}

}