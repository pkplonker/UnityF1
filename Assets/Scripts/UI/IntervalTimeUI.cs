using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenF1CSharp;
using UnityEngine;

public class IntervalTimeUI : MonoBehaviour
{
	[SerializeField]
	private GameObject Prefab;

	[SerializeField]
	private GameObject Container;

	private List<IntervalTimeElementUI> elements = new();
	private List<Driver> drivers;
	private Dictionary<Driver, IntervalTimeElementUI> driverElementMap;
	private bool init;
	private void Awake() => ServiceLocator.Instance.ServiceRegistered += OnServiceRegistered;

	private void OnServiceRegistered(IService obj)
	{
		if (obj is not Importer importer) return;
		importer.DriversRegistered += GenerateElements;
		Driver.IntervalsUpdated += OnIntervalUpdated;
	}

	private void OnIntervalUpdated() => UpdateIntervals(drivers);

	private void GenerateElements(List<Driver> drivers)
	{
		this.drivers = new List<Driver>(drivers);
		foreach (var driver in drivers)
		{
			var element = Instantiate(Prefab, Container.transform).GetComponent<IntervalTimeElementUI>();
			element.Init(driver);
			elements.Add(element);
		}

		driverElementMap = new Dictionary<Driver, IntervalTimeElementUI>();
		foreach (var element in elements)
		{
			driverElementMap[element.Driver] = element;
		}

		init = true;
		UpdateIntervals(this.drivers);
	}

	private void UpdateIntervals(List<Driver> drivers)
	{
		if (!init) return;
		drivers.Sort((d1, d2) => d1.Position.CompareTo(d2.Position));
		for (var i = 0; i < drivers.Count; i++)
		{
			if (driverElementMap.TryGetValue(drivers[i], out var element))
			{
				element.transform.SetSiblingIndex(i);
			}
		}
	}

	public void SetGapToLeader(bool state) => elements.ForEach(x => x.ShowGapToLeader = state);
}