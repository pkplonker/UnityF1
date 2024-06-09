using UnityEngine;

namespace DefaultNamespace
{
	public static class Extension
	{
		public static Color HexToColor(this string hex)
		{
			Color color;
			if (ColorUtility.TryParseHtmlString("#" + hex, out color))
			{
				return color;
			}
			else
			{
				Debug.LogError("Invalid hex color string");
				return Color.magenta;
			}
		}
	}
}