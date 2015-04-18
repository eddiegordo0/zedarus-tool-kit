﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using Zedarus.Traffico.Helpers;
using Zedarus.Traffico.Data.PlayerData;
using Serialization;
using Zedarus.ToolKit;
using Zedarus.Traffico.Settings;

namespace Zedarus.ToolKit.API
{
	#if API_ICLOUD_P31
	public class ICloudWrapper : APIWrapper<ICloudWrapper>, ISyncWrapperInterface 
	{
		#region Parameters
		private DateTime _lastSync;
		#endregion
		
		#region Events
		public event Action SyncFinished;
		#endregion
		
		#region Parameters
		#endregion
		
		#region Setup
		protected override void Setup() 
		{
			#if UNITY_IPHONE
			_lastSync = new DateTime(1986, 1, 1);
			iCloudBinding.getUbiquityIdentityToken();
			Sync();
			#endif
		}
		#endregion
		
		#region Controls
		public void Sync() 
		{
			#if UNITY_IPHONE
			ZedLogger.Log("Sync iCloud", LoggerContext.iCloud);
			iCloudBinding.synchronize();
			#endif
		}
		
		public bool SavePlayerData(PlayerData data)
		{
			#if UNITY_IPHONE
			if (!iCloudBinding.documentStoreAvailable())
			{
				ZedLogger.Log("Documetns store is not available", LoggerContext.iCloud);
				return false;
			}
			
			PlayerData remoteData = GetPlayerData();
			if (remoteData != null)
				data.Merge(remoteData);
			
			byte[] bytes = UnitySerializer.Serialize(data);
			bool result = P31CloudFile.writeAllBytes(APIManager.Instance.Settings.iCloudFilename, bytes);
			
			ZedLogger.Log("File written in the cloud: " + result, LoggerContext.iCloud);
			return result;
			#else
			return false;
			#endif
		}
		
		public PlayerData GetPlayerData()
		{
			#if UNITY_IPHONE
			if (P31CloudFile.exists(APIManager.Instance.Settings.iCloudFilename))
			{
				byte[] bytes = P31CloudFile.readAllBytes(APIManager.Instance.Settings.iCloudFilename);
				PlayerData data = UnitySerializer.Deserialize<PlayerData>(bytes);
				return data;
			}
			
			ZedLogger.Log("Could not restore player data from cloud", LoggerContext.iCloud);
			return null;
			#else
			return null;
			#endif
		}
		#endregion
		
		#region Event Listeners
		protected override void CreateEventListeners() 
		{
			#if UNITY_IPHONE
			iCloudManager.keyValueStoreDidChangeEvent += keyValueStoreDidChangeEvent;
			iCloudManager.ubiquityIdentityDidChangeEvent += ubiquityIdentityDidChangeEvent;
			iCloudManager.entitlementsMissingEvent += entitlementsMissingEvent;
			iCloudManager.documentStoreUpdatedEvent += documentStoreUpdatedEvent;
			#endif
		}
		
		protected override void RemoveEventListeners() 
		{
			#if UNITY_IPHONE
			iCloudManager.keyValueStoreDidChangeEvent -= keyValueStoreDidChangeEvent;
			iCloudManager.ubiquityIdentityDidChangeEvent -= ubiquityIdentityDidChangeEvent;
			iCloudManager.entitlementsMissingEvent -= entitlementsMissingEvent;
			iCloudManager.documentStoreUpdatedEvent -= documentStoreUpdatedEvent;
			#endif
		}
		#endregion

		#if UNITY_IPHONE
		#region Event Handlers
		private void keyValueStoreDidChangeEvent(List<object> keys)
		{
			ZedLogger.Log("keyValueStoreDidChangeEvent. changed keys:", LoggerContext.iCloud);
			foreach(var key in keys)
				ZedLogger.Log(key, LoggerContext.iCloud);
		}
		
		private void ubiquityIdentityDidChangeEvent()
		{
			ZedLogger.Log("ubiquityIdentityDidChangeEvent", LoggerContext.iCloud);
		}
		
		private void entitlementsMissingEvent()
		{
			ZedLogger.Log("entitlementsMissingEvent", LoggerContext.iCloud);
		}
		
		private void documentStoreUpdatedEvent(List<iCloudManager.iCloudDocument> files)
		{
			ZedLogger.Log("documentStoreUpdatedEvent. changed files: ", LoggerContext.iCloud);
			
			bool neededFileDownloaded = false;
			
			foreach(var doc in files)
			{
				ZedLogger.Log(doc, LoggerContext.iCloud);
				
				if (doc.filename.Equals(APIManager.Instance.Settings.iCloudFilename))
				{
					if (doc.isDownloaded)
					{
						int result = DateTime.Compare(doc.contentChangedDate, _lastSync);
						ZedLogger.Log("Comparing " + doc.contentChangedDate + " to " + _lastSync + ", result: " + result, LoggerContext.iCloud);
						// Only sync if file change date is different from the last one:
						if (result > 0)
						{
							neededFileDownloaded = true;
							_lastSync = doc.contentChangedDate;
						}
					}
					else
						iCloudBinding.isFileDownloaded(doc.filename);
				}
			}
			
			if (neededFileDownloaded)
				SendSyncFinishedEvent();
		}
		#endregion
		#endif
		
		#region Event Senders
		private void SendSyncFinishedEvent()
		{
			ZedLogger.Log("Sending sync finished event", LoggerContext.iCloud);
			if (SyncFinished != null)
				SyncFinished();
		}
		#endregion
	}
	#endif
}