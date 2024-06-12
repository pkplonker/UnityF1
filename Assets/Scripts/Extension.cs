using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extension
{
	public static Color HexToColor(this string hex)
	{
		Color color;
		if (ColorUtility.TryParseHtmlString("#" + hex, out color))
		{
			return color;
		}

		Debug.LogError("Invalid hex color string");
		return Color.magenta;
	}

	public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
	{
		foreach (var c in collection)
		{
			action.Invoke(c);
		}
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> collection) => collection.Where(x => x != null);

	public static IEnumerable<T> WherePropertyNotNull<T, TProperty>(this IEnumerable<T> collection,
		Func<T, TProperty> propertySelector) =>  collection.Where(x => x != null && propertySelector(x) != null);
}