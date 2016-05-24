﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zedarus.ToolKit.Math;

namespace Zedarus.ToolKit.PoolManagement
{
	public class PoolManager<T> where T : MonoBehaviour, IPoolItem
	{
		private List<T> _pool;
		private List<T> _active;
		private Transform _container;
		private bool _autoReuse;

		public PoolManager(Transform container, int size, bool autoReuse, T[] prefabs, params int[] prefabsShuffleBag)
		{
			_autoReuse = autoReuse;
			_pool = new List<T>();
			_active = new List<T>();

			_container = container;
			T[] children = _container.GetComponentsInChildren<T>();
			foreach (T child in children)
			{
				child.Init();
				_pool.Add(child);
			}

			if (_pool.Count < size && prefabs != null && prefabs.Length > 0)
			{
				ShuffleBag<int> bag = new ShuffleBag<int>();
				for (int i = 0; i < prefabs.Length; i++)
				{
					bag.Add(i, prefabsShuffleBag.Length > i ? prefabsShuffleBag[i] : 1);
				}

				int attempts = size + 200;
				while (_pool.Count < size && attempts > 0)
				{
					T newPoolObject = GameObject.Instantiate(prefabs[bag.Next()]) as T;
					if (newPoolObject != null)
					{
						newPoolObject.transform.parent = container;
						newPoolObject.Init();
						_pool.Add(newPoolObject);
					}
					else
						attempts--;
				}

				bag.Clear();
				bag = null;
			}

			prefabs = null;
		}

		public PoolManager(Transform container, int size, T prefab, bool autoReuse = false) : this(container, size, autoReuse, new T[] { prefab }, 1) { }

		public virtual void ReturnInactiveItemsToPool()
		{
			for (int i = _active.Count - 1; i >= 0; i--)
			{
				if (_active[i].ReturnToPool)
					ReturnItemToPoolAtIndex(i);
			}
		}

		public virtual T GetItemFromPool()
		{
			if (_pool.Count > 0)
			{
				T item = _pool[0];
				_pool.RemoveAt(0);
				_active.Add(item);
				return item;
			}
			else if (_autoReuse && _active.Count > 0)
			{
				ReturnItemToPool(_active[0]);
				return GetItemFromPool();
			}
			else
				return null;
		}

		public virtual T[] GetItemFromPool(int number)
		{
			List<T> items = new List<T>();
			for (int i = 0; i < number; i++)
			{
				T item = GetItemFromPool();
				if (item)
					items.Add(item);
			}

			return items.ToArray();
		}
		
		public virtual void ReturnItemToPool(T item)
		{
			_active.Remove(item);
			item.Deactivate();
			_pool.Add(item);
		}

		public virtual void ReturnItemToPoolAtIndex(int index)
		{
			T item = _active[index];
			_active.RemoveAt(index);
			item.Deactivate();
			_pool.Add(item);
		}

		public void ReturnAllItemsToPool()
		{
			for (int i = _active.Count - 1; i >= 0; i--)
				ReturnItemToPoolAtIndex(i);
		}

		public List<T> Active
		{
			get { return _active; }
		}
	}
}
