using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
using DDD.Scripts.Core;

namespace Wuprui.Core.DataStorage
{
	public class Storage
	{
		private static Storage dataStorage = new Storage();
		public static Storage Instance => dataStorage;

		private Dictionary<string, object> storage = new Dictionary<string, object>();
		private Dictionary<string, Action<object>> observers = new Dictionary<string, Action<object>>();

		private Storage() { }

		public void AddObserver(string dataKey, Action<object> callback)
		{
            if (observers.TryGetValue(dataKey, out Action<object> cb))
            {
                cb += callback;
            }
            else
            {
                observers[dataKey] = callback;
            }
        }

		public void RemoveObserver(string dataKey, Action<object> callback)
		{
			if (observers.TryGetValue(dataKey, out Action<object> cb))
			{
				cb -= callback;
			}
		}

		public Storage Add<T>(string dataKey, object data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			storage[dataKey] = data;

			if (observers.TryGetValue(dataKey, out Action<object> callback))
			{
				callback?.Invoke(data);
			}

			return dataStorage;
		}

		public bool TryGet<T>(string dataKey, out T value)
		{
			object objValue;

			if (storage.TryGetValue(dataKey, out objValue))
			{
				if (objValue is T tValue)
				{
					value = tValue;
					return true;
				}
			}

			DDDDebug.LogWarning($"Data with key {dataKey} was not added to repository yet. Default value returned");

			value = default;

			return false;
		}

		public T Get<T>(string dataKey)
		{
			object objValue;

			if (storage.TryGetValue(dataKey, out objValue))
			{
				if (objValue is T tValue)
				{
					return tValue;
				}
			}

			Debug.LogWarning($"Data with key {dataKey} was not added to repository yet. Default value returned");

			return default;
		}

		public void Remove(string dataKey)
		{
			storage.Remove(dataKey);
			observers.Remove(dataKey);
		}

		public void Clear()
		{
			storage.Clear();
			observers.Clear();
		}

		public void ClearByLabels(params Type[] labelTypes)
		{
			var keysToRemove = labelTypes
				.SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public))
				.Select(field => field.GetValue(null).ToString())
				.ToList();

			keysToRemove.ForEach(key =>
			{
				storage.Remove(key);
				observers.Remove(key);
			});
		}

		public void ClearByLabelsExcept(params Type[] args)
		{
			var excepted = args
				.SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Select(fi => fi.GetValue(null)
				.ToString()))
				.ToList();

			storage = storage
				.Where(kv => excepted.Contains(kv.Key))
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			observers = observers
				.Where(kv => excepted.Contains(kv.Key))
				.ToDictionary(kv => kv.Key, kv => kv.Value);
		}
	}
}