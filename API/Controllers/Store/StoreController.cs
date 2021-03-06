﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Zedarus.ToolKit;
using Zedarus.ToolKit.Events;
using Zedarus.ToolKit.Settings;

namespace Zedarus.ToolKit.API
{
	public class StoreController : APIController
	{
		#region Events
		public event Action<string, bool> ProductPurchaseFinished;
		#endregion

		#region Parameters
		private StoreProduct[] _products;
		private Queue<string> _queuedPurchases;
		private Queue<Action<string, bool>> _queuedPurchasesCallbacks;
		private Dictionary<string, Action<string, bool>> _callbacks;
		#endregion
		
		#region Initialization
		protected override void Setup() 
		{
			_queuedPurchases = new Queue<string>();
			_queuedPurchasesCallbacks = new Queue<Action<string, bool>>();
			_callbacks = new Dictionary<string, Action<string, bool>>();
		}
		
		protected override void CompleteInitialization()
		{
			base.CompleteInitialization();
			CheckQueue();
		}

		public void RegisterProducts(StoreProduct[] products)
		{
			_products = products;

			foreach (IStoreWrapperInterface wrapper in Wrappers)
			{
				wrapper.GetProductsListFromServer(_products);
			}
		}
		#endregion
		
		#region Wrappers Initialization
		protected override IAPIWrapperInterface GetWrapperForAPI(int wrapperAPI)
		{
			switch (wrapperAPI)
			{
				case APIs.IAPs.Unity:
					return UnityStoreWrapper.Instance;
				default:
					return null;
			}
		}
		#endregion

		#region Controls
		public void Purchase(string productID, Action<string, bool> callback)
		{
			if (!IsInitialized)
			{
				_queuedPurchases.Enqueue(productID);
				_queuedPurchasesCallbacks.Enqueue(callback);
				ZedLogger.LogWarning("Store controller is not yet initized, so purchase is queued");
				return;	
			}
			
			StoreProduct product = GetProductWithID(productID);
			if (product != null)
			{
				if (Wrapper != null) 
				{
					if (_callbacks.ContainsKey(productID))
					{
						_callbacks.Remove(productID);
					}

					_callbacks.Add(productID, callback);

					Wrapper.Purchase(product);
				}
			} 
			else
			{
				ZedLogger.LogWarning("Product with ID \"" + productID + "\" not found in the store.");
			}
		}

		public void RestorePurchases() 
		{
			if (Wrapper != null) Wrapper.RestorePurchases();
		}
		
		public string GetPriceForProduct(string productID)
		{
			StoreProduct item = GetProductWithID(productID);
			if (item != null)
				return item.FormattedPrice;
			else
				return "--";
		}
		#endregion
		
		#region Event Listeners
		protected override void CreateEventListeners() 
		{
			base.CreateEventListeners();
			
			foreach (IStoreWrapperInterface wrapper in Wrappers)
			{
				wrapper.PurchaseProcessStarted += OnPurchaseStarted;
				wrapper.PurchaseProcessFinished += OnPurchaseFinished;
				wrapper.ProductsListReceivedFromServer += OnProductsListReceivedFromServer;
			}
		}
		
		protected override void RemoveEventListeners() 
		{
			base.RemoveEventListeners();
			
			foreach (IStoreWrapperInterface wrapper in Wrappers)
			{
				wrapper.PurchaseProcessStarted -= OnPurchaseStarted;
				wrapper.PurchaseProcessFinished -= OnPurchaseFinished;
				wrapper.ProductsListReceivedFromServer -= OnProductsListReceivedFromServer;
			}
		}
		#endregion

		#region Event Handlers
		private void OnPurchaseStarted(string productID)
		{
			
		}
		
		private void OnPurchaseFinished(string productID, bool success)
		{
			if (_callbacks.ContainsKey(productID))
			{
				if (_callbacks[productID] != null)
				{
					_callbacks[productID](productID, success);
				}

				_callbacks.Remove(productID);
			}

			if (ProductPurchaseFinished != null)
			{
				ProductPurchaseFinished(productID, success);
			}
			
			CheckQueue();
		}
		
		private void OnProductsListReceivedFromServer()
		{
			if (Wrapper != null)
			{
				for (int i = 0; i < _products.Length; i++)
				{
					decimal price = Wrapper.GetDecimalPriceForProductWithID(_products[i].ID);
					string formattedPrice = Wrapper.GetLocalisedPriceForProductWithID(_products[i].ID);
					string currency = Wrapper.GetCurrencyIDForProductWithID(_products[i].ID);
					_products[i].UpdatePrice(price, formattedPrice, currency);
				}
			}
		}
		#endregion
		
		#region Helpers
		private StoreProduct GetProductWithID(string id)
		{
			for (int i = 0; i < _products.Length; i++)
			{
				if (_products[i].ID.Equals(id))
					return _products[i];
			}

			return null;
		}
		#endregion
		
		#region Getters
		protected IStoreWrapperInterface Wrapper
		{
			get { return (IStoreWrapperInterface)CurrentWrapperBase; }
		}
		#endregion
		
		#region Purchases Queue
		private void CheckQueue()
		{
			if (_queuedPurchases.Count > 0)
			{
				Purchase(_queuedPurchases.Dequeue(), _queuedPurchasesCallbacks.Dequeue());
			}
		}
		#endregion
	}
}
