using System.Collections;
using OpenF1CSharp;
using TMPro;
using UnityEngine;

public class RaceNotificationUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	private RaceControlData raceControlData;
	private float lifetime;

	public void Init(RaceControlData notificationData, float lifetime)
	{
		this.raceControlData = notificationData;
		this.lifetime = lifetime;
		this.text.text =
			$"{(raceControlData.Date.HasValue ? raceControlData.Date.Value.ToString("hh\\:mm\\:ss") : "")} {raceControlData.Message}";
		StartCoroutine(KillTimer());
	}

	private IEnumerator KillTimer()
	{
		yield return new WaitForSeconds(lifetime);
		Destroy(gameObject);
	}
}