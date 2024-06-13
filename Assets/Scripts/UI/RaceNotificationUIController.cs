using System;
using System.Collections;
using System.Collections.Generic;
using OpenF1CSharp;
using UnityEngine;

public class RaceNotificationUIController : MonoBehaviour
{
	[SerializeField]
	private GameObject container;

	[SerializeField]
	private int maxElements = 5;

	private RaceControlData[] notificationDatas;
	private RaceNotificationUI[] notifications;

	private int index = 0;

	[SerializeField]
	private float lifetime = 10f;

	[SerializeField]
	private GameObject notificationPrefab;

	private void Start()
	{
		ServiceLocator.Instance.ServiceRegistered += OnServiceRegistered;

		notificationDatas = new RaceControlData[maxElements];
		notifications = new RaceNotificationUI[maxElements];
	}

	private void OnServiceRegistered(IService obj)
	{
		if (obj is RaceDirectorManager rdm)
		{
			rdm.RaceDirectorNotification += OnRaceDirectorNotification;
		}
	}

	private void OnRaceDirectorNotification(RaceControlData obj)
	{
		index++;
		var actualIndex = index % notificationDatas.Length;
		notificationDatas[actualIndex] = obj;
		var go = Instantiate(notificationPrefab, container.transform);
		if (notifications[actualIndex] != null)
		{
			Destroy(notifications[actualIndex].gameObject);
		}

		var ui = go.GetComponent<RaceNotificationUI>();
		ui.Init(notificationDatas[actualIndex], lifetime);
		notifications[actualIndex] = ui;
	}
}