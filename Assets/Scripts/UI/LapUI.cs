using TMPro;
using UnityEngine;

public class LapUI : MonoBehaviour
{
	[SerializeField]
	private Importer importer;

	[SerializeField]
	private TextMeshProUGUI text;

	private void Awake() => importer.LapNumberChanged += OnLapNumberChanged;

	private void Start() => OnLapNumberChanged(0);

	private void OnLapNumberChanged(int lap) => text.text = $"{lap}/{importer.MaxLap}";
}