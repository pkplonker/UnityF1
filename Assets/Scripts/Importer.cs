using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using OpenF1CSharp;
using Debug = UnityEngine.Debug;

public class Importer : MonoBehaviour, IService
{
	private List<LocationData> locationData;

	private List<LapData> lapData;
	private LineRenderer lr;
	public float PlaybackSpeed = 5;

	private int driver = 4;

	public int SessionKey { get; private set; } = 9523; // monaco 2024
	private List<Driver> drivers = new();
	private List<IUpdateable> updateables = new();
	private bool init;
	private SessionData sessionData;
	private DateTime currentTime;
	private List<RaceControlData> raceControlData;
	private int currentLapNumber = -1;
	public event Action<int> LapNumberChanged;

	public int CurrentLapNumber
	{
		get => currentLapNumber;
		private set
		{
			if (value != currentLapNumber)
			{
				currentLapNumber = value;
				LapNumberChanged?.Invoke(currentLapNumber);
			}
		}
	}

	public int MaxLap { get; private set; }

	private List<IntervalData> intervalData;
	private List<PositionData> positionData;

	public DateTime CurrentTime
	{
		get => currentTime;
		private set
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
	public event Action<List<Driver>> DriversRegistered;

	public void Start()
	{
		ServiceLocator.Instance.RegisterService(this);
		GetData();
	}

	private async void GetData()
	{
		try
		{
			if (await GetSessionData()) return;
			await GenerateLapData();
			await GenerateLocationData();

			await GenerateRaceControlData();
			var (sector1, sector2, sector3) = GetSectorTimes();

			updateables.Add(new RaceDirectorManager(raceControlData, sector1, sector1 + sector2,
				sector1 + sector2 + sector3, sessionData.DateStart.Value));
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to get map generation data {e}");
			throw;
		}

		for (var i = 0; i < locationData.Count; i++)
		{
			var data1 = locationData[i];
			data1.Date = locationData[i].Date?.AddHours(-1);
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
		CurrentLapNumber = 0;
		DriversRegistered?.Invoke(drivers);
	}

	private (int sector1, int sector2, int sector3) GetSectorTimes()
	{
		var sector1 = lapData.Max(x => x.SegmentsSector1.Count);
		var sector2 = lapData.Max(x => x.SegmentsSector1.Count);
		var sector3 = lapData.Max(x => x.SegmentsSector1.Count);
		return (sector1, sector2, sector3);
	}

	private async Task GenerateRaceControlData()
	{
		var rawRaceControlData = await OpenF1QueryManager.Instance.Get(new RaceControlQuery()
			.Filter(nameof(RaceControlData.SessionKey), SessionKey).GenerateQuery());
		raceControlData = JsonConvert.DeserializeObject<List<RaceControlData>>(rawRaceControlData);
	}

	private async Task GenerateLocationData()
	{
		var rawLocationData = await OpenF1QueryManager.Instance.Get(new LocationQuery()
			.Filter(nameof(LocationData.DriverNumber), driver)
			.Filter(nameof(LocationData.SessionKey), SessionKey)
			.GenerateQuery());

		locationData = JsonConvert.DeserializeObject<List<LocationData>>(rawLocationData);

		locationData = locationData.Where(x => x.Date.Value > sessionData.DateStart.Value).ToList();
	}

	private async Task GenerateLapData()
	{
		var rawLapData = await OpenF1QueryManager.Instance.Get(new LapQuery()
			.Filter(nameof(LapData.SessionKey), SessionKey)
			.GenerateQuery());

		lapData = JsonConvert.DeserializeObject<List<LapData>>(rawLapData);
		MaxLap = lapData.Max(x => x.LapNumber.Value);
	}

	private async Task<bool> GetSessionData()
	{
		var rawSessionData = await OpenF1QueryManager.Instance.Get(new SessionQuery()
			.Filter(nameof(SessionData.SessionKey), SessionKey)
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
			.Filter(nameof(DriverData.SessionKey), SessionKey)
			.GenerateQuery());
		List<DriverData> driverData = new();
		driverData = JsonConvert.DeserializeObject<List<DriverData>>(driversRawData);

		var rawIntervalData = await OpenF1QueryManager.Instance.Get(new IntervalQuery()
			.Filter(nameof(IntervalData.SessionKey), SessionKey).GenerateQuery());
		intervalData = new List<IntervalData>();
		intervalData = JsonConvert.DeserializeObject<List<IntervalData>>(rawIntervalData);

		var rawPositionData = await OpenF1QueryManager.Instance.Get(new PositionQuery()
			.Filter(nameof(PositionData.SessionKey), SessionKey).GenerateQuery());
		positionData = new List<PositionData>();
		positionData = JsonConvert.DeserializeObject<List<PositionData>>(rawPositionData);

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
			tasks[i] = Task.Run(() => driverMono.Init(lapData,
				intervalData.Where(x => x.DriverNumber == driverData[i1].DriverNumber).ToList(),
				positionData.Where(x => x.DriverNumber == driverData[i1].DriverNumber).ToList(), driverData[i1],
				sessionData));
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

		CurrentTime = CurrentTime.AddSeconds(Time.deltaTime * PlaybackSpeed);
		bool allFalse = true;
		foreach (var driver in drivers.WhereNotNull())
		{
			if (driver.Tick(CurrentTime))
			{
				allFalse = false;
			}

			var ln = driver.CurrentLap + 1;
			if (driver.CurrentLap > CurrentLapNumber)
			{
				CurrentLapNumber = ln;
			}
		}

		if (allFalse)
		{
			init = false;
		}

		updateables.WhereNotNull().ForEach(x => x.Tick(CurrentTime));
	}

	public void Initialize() { }
}