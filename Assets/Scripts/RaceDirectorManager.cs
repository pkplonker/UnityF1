using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenF1CSharp;
using UnityEngine;

public class RaceDirectorManager : IUpdateable, IService
{
	private FlagState[] CurrentFlags;

	private readonly List<RaceControlData> raceControlData;
	private int lastIndex = 0;
	private readonly int subSector1Count;
	private readonly int subSector2Count;
	private readonly int subSector3Count;

	public event Action<FlagState> FlagChanged;
	public event Action<RaceControlData> RaceDirectorNotification;

	public RaceDirectorManager(List<RaceControlData> raceControlData, int sector1, int sector2, int sector3,
		DateTime startTime)
	{
		subSector1Count = sector1;
		subSector2Count = sector2;
		subSector3Count = sector3;
		ServiceLocator.Instance.RegisterService(this);
		this.raceControlData = raceControlData;

		lastIndex = raceControlData.Count(x => x.Date.Value < startTime);
		CurrentFlags = new FlagState[4]
		{
			new (Flag.Clear, FlagArea.One),
			new (Flag.Clear, FlagArea.Two),
			new (Flag.Clear, FlagArea.Three),
			new (Flag.Clear, FlagArea.Track),
		};
		foreach (var fs in CurrentFlags)
		{
			fs.FlagStateChanged += OnFlagChanged;
		}
	}

	private void OnFlagChanged(FlagState state) => FlagChanged?.Invoke(state);

	public bool Tick(DateTime currentTime)
	{
		var count = raceControlData.Count;
		if (lastIndex >= count - 1) return false;
		while (raceControlData[lastIndex].Date <= currentTime && lastIndex < count - 1)
		{
			var element = raceControlData[lastIndex];
			if (!string.IsNullOrEmpty(element.Message))
			{
				RaceDirectorNotification?.Invoke(element);
			}

			var area = FlagArea.Track;
			if (string.Equals(element.Scope, "Track", StringComparison.InvariantCultureIgnoreCase))
			{
				area = FlagArea.Track;
				SetFlag(CurrentFlags[(int) area], element.Flag);
			}
			else if (string.Equals(element.Scope, "Sector", StringComparison.InvariantCultureIgnoreCase))
			{
				if (element.Sector.HasValue)
				{
					if (element.Sector.Value < subSector1Count)
					{
						area = FlagArea.One;
					}
					else if (element.Sector.Value < subSector2Count)
					{
						area = FlagArea.Two;
					}
					else if (element.Sector.Value < subSector3Count)
					{
						area = FlagArea.Three;
					}
				}

				SetFlag(CurrentFlags[(int) area], element.Flag);
			}
			else if (string.Equals(element.Scope, "Driver", StringComparison.InvariantCultureIgnoreCase))
			{
				//not yet implemented
			}

			lastIndex++;
		}

		return true;
	}

	private void SetFlag(FlagState flagState, string elementFlag)
	{
		if (string.IsNullOrEmpty(elementFlag)) return;
		switch (elementFlag)
		{
			case "GREEN":
				flagState.Flag = Flag.Green;
				break;
			case "YELLOW":
			case "DOUBLE YELLOW":
				flagState.Flag = Flag.Yellow;
				break;
			case "RED":
				flagState.Flag = Flag.Red;
				break;
			case "BLUE":
				flagState.Flag = Flag.Blue;
				break;
			case "CLEAR":
				flagState.Flag = Flag.Clear;
				break;
			case "CHEQUERED":
				flagState.Flag = Flag.Chequered;
				break;
			default:
				flagState.Flag = Flag.Clear;
				Debug.Log($"Unrecognised flag {flagState.Flag}");
				break;
		}
	}

	public void Initialize() { }
}

public enum Flag
{
	Green,
	Yellow,
	DoubleYellow,
	Clear,
	Red,
	Blue,
	Chequered
}

public enum FlagArea
{
	One,
	Two,
	Three,
	Track
}