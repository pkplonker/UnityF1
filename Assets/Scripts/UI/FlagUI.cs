using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagUI : MonoBehaviour
{
	[SerializeField]
	private Image sector1;

	[SerializeField]
	private Image sector2;

	[SerializeField]
	private Image sector3;

	[SerializeField]
	private Image sectorTrack;

	private Dictionary<Flag, UnityEngine.Color> flagColorDict;
	private void Start()
	{
		ServiceLocator.Instance.ServiceRegistered += OnServiceRegistered;
		flagColorDict = new Dictionary<Flag, UnityEngine.Color>
		{
			{Flag.Yellow, UnityEngine.Color.yellow},
			{Flag.DoubleYellow, UnityEngine.Color.yellow},
			{Flag.Red, UnityEngine.Color.red},
			{Flag.Green, UnityEngine.Color.green},
			//{Flag.Blue, UnityEngine.Color.blue},
			{Flag.Chequered, UnityEngine.Color.magenta},
			{Flag.Clear, new UnityEngine.Color(0,0,0,0)},
		};
	}

	private void OnServiceRegistered(IService obj)
	{
		if (obj is RaceDirectorManager rdm)
		{
			rdm.FlagChanged += OnFlagChanged;
		}
	}

	private void OnFlagChanged(FlagState flagState)
	{
		if (flagColorDict.TryGetValue(flagState.Flag, out var color))
		{
			switch (flagState.Area)
			{
				case FlagArea.One:
					sector1.color = color;
					break;
				case FlagArea.Two:
					sector2.color = color;
					break;
				case FlagArea.Three:
					sector3.color = color;
					break;
				case FlagArea.Track:
					sectorTrack.color = color;
					break;
				default:
					Debug.Log($"Invalid flagstate{flagState.ToString()}");
					break;
			}
			Debug.Log(
				$"Setting Color {Enum.GetName(typeof(Flag), flagState.Flag)} in {Enum.GetName(typeof(FlagArea), flagState.Area)}");
		}
	}
}