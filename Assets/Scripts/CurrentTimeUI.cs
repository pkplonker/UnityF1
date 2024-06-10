using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentTimeUI : MonoBehaviour
{
	[SerializeField]
	private Importer importer;

	[SerializeField]
	private TextMeshProUGUI text;

	private void Awake()
	{
		importer.TimeUpdated += OnTimeUpdated;
	}

	private void OnTimeUpdated(DateTime date)
	{
		text.text = date.TimeOfDay.ToString("hh\\:mm\\:ss");
	}
}