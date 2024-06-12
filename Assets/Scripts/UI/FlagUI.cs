using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = System.Drawing.Color;

public class FlagUI : MonoBehaviour
{
	[SerializeField]
	private Image image;

	private Dictionary<RaceDirectorManager.Flag, UnityEngine.Color> flagColorDict;

	private void Start()
	{
		ServiceLocator.Instance.ServiceRegistered += OnServiceRegistered;
		flagColorDict = new Dictionary<RaceDirectorManager.Flag, UnityEngine.Color>
		{
			{RaceDirectorManager.Flag.Yellow, UnityEngine.Color.yellow},
			{RaceDirectorManager.Flag.DoubleYellow, UnityEngine.Color.yellow},
			{RaceDirectorManager.Flag.Red, UnityEngine.Color.red},
			{RaceDirectorManager.Flag.Green, UnityEngine.Color.green},
			{RaceDirectorManager.Flag.Blue, UnityEngine.Color.blue},
			{RaceDirectorManager.Flag.Chequered, UnityEngine.Color.magenta},
			{RaceDirectorManager.Flag.Clear, UnityEngine.Color.white},
		};
	}

	private void OnServiceRegistered(IService obj)
	{
		if (obj is RaceDirectorManager rdm)
		{
			rdm.FlagChanged += OnFlagChanged;
		}
	}

	private void OnFlagChanged(RaceDirectorManager.Flag flag)
	{
		if (flagColorDict.TryGetValue(flag, out var color))
		{
			image.color = color;
			Debug.Log($"Setting Color {Enum.GetName(typeof(RaceDirectorManager.Flag), flag)}");
		}
	}
}