using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using OpenF1CSharp;
using Debug = UnityEngine.Debug;

public class Importer : MonoBehaviour
{
	private List<LocationData> locationData;

	private List<LapData> lapData;
	private LineRenderer lr;

	private int driver = 4;

	private int sessionKey = 9523; // monaco 2024
	private List<Driver> drivers = new();
	private bool init;
	private SessionData sessionData;
	private DateTime currentTime;

	public DateTime CurrentTime
	{
		get => currentTime;
		set
		{
			if (currentTime != value)
			{
				currentTime = value;
				TimeUpdated?.Invoke(CurrentTime);
			}
		}
	}

	private const float THICKNESS = 100;
	public event Action<DateTime> TimeUpdated;

	public void Start()
	{
		GetData();
	}

	private async void GetData()
	{
		try
		{
			if (await GetSessionData()) return;
			var rawLapData = await OpenF1QueryManager.Instance.Get(new LapQuery()
				.Filter(nameof(LapData.SessionKey), sessionKey)
				.GenerateQuery());

			lapData = JsonConvert.DeserializeObject<List<LapData>>(rawLapData);

			var rawLocationData = await OpenF1QueryManager.Instance.Get(new LocationQuery()
				.Filter(nameof(LocationData.DriverNumber), driver)
				.Filter(nameof(LocationData.SessionKey), sessionKey)
				.GenerateQuery());

			locationData = JsonConvert.DeserializeObject<List<LocationData>>(rawLocationData);

			locationData = locationData.Where(x => x.Date.Value > sessionData.DateStart.Value).ToList();
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to get map generation data {e}");
			throw;
		}

		for (int i = 0; i < locationData.Count; i++)
		{
			var data1 = locationData[i];
			data1.Date = locationData[i].Date.Value.AddHours(-1);
			locationData[i] = data1;
		}

		if (!locationData.Any() || !lapData.Any())
		{
			Debug.LogError("Failed to get data");
			return;
		}

		var tasks = await GenerateDrivers(lapData);

		await Task.WhenAll(tasks);
		GenerateCircuit();
		init = true;
		CurrentTime = sessionData.DateStart.Value;
	}

	private async Task<bool> GetSessionData()
	{
		var rawSessionData = await OpenF1QueryManager.Instance.Get(new SessionQuery()
			.Filter(nameof(SessionData.SessionKey), sessionKey)
			.GenerateQuery());
		List<SessionData> sd = new();

		sd = JsonConvert.DeserializeObject<List<SessionData>>(rawSessionData);

		if (sd.Any())
		{
			sessionData = sd.First();
		}
		else
		{
			Debug.LogError("Failed to get sessionData");
			return true;
		}

		return false;
	}

	private async Task<Task[]> GenerateDrivers(List<LapData> lapData)
	{
		var driversRawData = await OpenF1QueryManager.Instance.Get(new DriverQuery()
			.Filter(nameof(DriverData.SessionKey), sessionKey)
			.GenerateQuery());
		List<DriverData> driverData = new();

		driverData = JsonConvert.DeserializeObject<List<DriverData>>(driversRawData);

		if (!driverData?.Any() ?? false)
		{
			return Array.Empty<Task>();
		}

		var count = driverData?.Count ?? 0;
		var tasks = new Task[count];
		for (int i = 0; i < count; i++)
		{
			var go = new GameObject(driverData[i].NameAcronym);
			go.transform.SetParent(transform);
			var driverMono = go.AddComponent<Driver>();
			var i1 = i;
			tasks[i] = Task.Run(() => driverMono.Init(lapData, driverData[i1], sessionData));
			drivers.Add(driverMono);
		}

		return tasks;
	}

	private void GenerateCircuit()
	{
		var driverLapData = lapData.Where(x => x.DriverNumber == driver).ToList();
		var data = locationData.Where(x => x.Date < driverLapData[10].DateStart && x.Date > driverLapData[9].DateStart)
			.Select(x => new Vector3(x.X.Value, 0, x.Y.Value));
		lr = GetComponent<LineRenderer>();
		lr.positionCount = data.Count();
		lr.SetPositions(data.ToArray());
		lr.startWidth = THICKNESS;
		lr.endWidth = THICKNESS;
	}

	private void Update()
	{
		if (!init) return;
		if (locationData == null) return;

		CurrentTime = CurrentTime.AddSeconds(Time.deltaTime);

		foreach (var d in drivers)
		{
			if (d != null)
			{
				d.Tick(CurrentTime);
			}
		}
	}
}