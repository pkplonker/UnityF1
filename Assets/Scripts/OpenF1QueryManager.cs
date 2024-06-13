using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using OpenF1CSharp;
using UnityEngine;

public class OpenF1QueryManager
{
	private const string SAVE_LOCATION = "CachedOpenF1Data";

	private static OpenF1QueryManager instance;

	public static OpenF1QueryManager Instance
	{
		get { return instance ??= new OpenF1QueryManager(); }
	}

	private OpenF1Reader openF1Reader = new OpenF1Reader();
	private Dictionary<string, string> cachedData = new ();

	private OpenF1QueryManager() { }

	public async Task<string> Get(string request)
	{
		if (cachedData.TryGetValue(request, out var value))
		{
			return value;
		}

		var hash = GetHashString(request);
		var path = Path.Join(Application.dataPath, SAVE_LOCATION, hash);
		if (File.Exists(path))
		{
			var data = File.ReadLines(path);
			if (data.Any())
			{
				cachedData.TryAdd(hash, data.First());
				return data.First();
			}
		}

		Debug.Log($"Sending new request {request}");
		string result;
		try
		{
			result = await openF1Reader.Query(request);
		}
		catch (Exception e)
		{
			Debug.Log($"Failed to execute query {e} - {request}");
			return string.Empty;
		}

		if (!string.IsNullOrEmpty(result))
		{
			SaveNewResult(hash, result);
		}
		else
		{
			Debug.Log($"Query result empty - {request}");
		}

		return result;
	}

	private void SaveNewResult(string hash, string data)
	{
		cachedData.TryAdd(hash, data);
		SaveText(hash, data);
	}

	private void SaveText(string hashFileName, string content)
	{
		var dir = Path.Join(Application.dataPath, SAVE_LOCATION);
		Directory.CreateDirectory(dir);
		var path = Path.Join(dir, hashFileName);

		File.WriteAllText(path, content);

#if UNITY_EDITOR
		MainThreadDispatcher.Instance.Enqueue(() => UnityEditor.AssetDatabase.Refresh());
#endif

		Debug.Log("File saved to: " + path);
	}

	private static byte[] GetHash(string inputString)
	{
		using HashAlgorithm algorithm = SHA256.Create();
		return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
	}

	private static string GetHashString(string inputString)
	{
		var sb = new StringBuilder();
		foreach (var b in GetHash(inputString))
			sb.Append(b.ToString("X2"));

		return sb.ToString();
	}
}