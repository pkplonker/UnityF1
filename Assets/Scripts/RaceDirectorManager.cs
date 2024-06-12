using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenF1CSharp;
using UnityEngine;

public class RaceDirectorManager : IUpdateable, IService
{
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

	private Flag currentFlag;

	public Flag CurrentFlag
	{
		get => currentFlag;
		set
		{
			if (value != currentFlag)
			{
				currentFlag = value;
				FlagChanged?.Invoke(CurrentFlag);
			}
		}
	}

	private readonly List<RaceControlData> raceControlData;
	private int lastIndex = 0;
	public event Action<Flag> FlagChanged;

	public RaceDirectorManager(List<RaceControlData> raceControlData)
	{
		ServiceLocator.Instance.RegisterService(this);
		this.raceControlData = raceControlData;
		// raceControlData
		// 	.GroupBy(x => x.Flag)
		// 	.Select(g => g.First())
		// 	.WherePropertyNotNull(x => x.Flag)
		// 	.ForEach(x => Debug.Log(x.Flag.ToString()));
	}

	public bool Tick(DateTime currentTime)
	{
		var count = raceControlData.Count;
		if (lastIndex >= count - 1) return false;
		while (raceControlData[lastIndex].Date <= currentTime && lastIndex < count - 1)
		{
			var element = raceControlData[lastIndex];
			if (!string.IsNullOrEmpty(element.Flag))
			{
				switch (element.Flag)
				{
					case "GREEN":
						CurrentFlag = Flag.Green;
						break;
					case "YELLOW":
					case "DOUBLE YELLOW":
						CurrentFlag = Flag.Yellow;
						break;
					case "RED":
						CurrentFlag = Flag.Red;
						break;
					case "BLUE":
						CurrentFlag = Flag.Blue;
						break;
					case "CLEAR":
						CurrentFlag = Flag.Clear;
						break;
					case "CHEQUERED":
						CurrentFlag = Flag.Chequered;
						break;
					default:
						CurrentFlag = Flag.Clear;
						Debug.Log($"Unrecognised flag {element.Flag}");
						break;
				}
			}

			lastIndex++;
		}

		return true;
	}

	public void Initialize()
	{
	}
}