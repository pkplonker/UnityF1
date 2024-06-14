using System;
using OpenF1CSharp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntervalTimeElementUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI position;

	[SerializeField]
	private TextMeshProUGUI driverName;

	[SerializeField]
	private TextMeshProUGUI interval;

	[SerializeField]
	private TextMeshProUGUI tyre;

	[SerializeField]
	private Image image;

	public Driver Driver { get; private set; }
	public bool ShowGapToLeader { get; set; } = true;

	public void Init(Driver driver)
	{
		this.Driver = driver;
		image.color = driver.DriverData.TeamColour.HexToColor();
		driverName.text = driver.DriverData.NameAcronym;
	}

	public void Update()
	{
		position.text = Driver.Position.ToString();
		var gap = ShowGapToLeader ? Driver.GapToLeader : Driver.Interval;
		if (gap == "0.0") gap = "Leader";
		interval.text = gap;
		tyre.text = Driver.Tyre;
	}
}