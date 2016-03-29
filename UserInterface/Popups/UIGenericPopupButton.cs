﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Zedarus.ToolKit.UI
{
	public class UIGenericPopupButton : MonoBehaviour
	{
		#region Parameters
		[SerializeField] 
		private Text _label;
		[SerializeField]
		private Image _colorElement;
		#endregion

		#region Properties
		private string _initialLabel;
		private Color _initialColor;
		private System.Action _callback = null;
		private bool _closePopupOnPress = false;
		#endregion

		#region Events
		public event System.Action ClosePopup;
		#endregion

		#region Initialization
		public void Init()
		{
			_initialLabel = _label.text;
			_initialColor = _colorElement.color;
		}

		public void Reset()
		{
			_label.text = _initialLabel;
			_colorElement.color = _initialColor;
		}
		#endregion

		#region Callbacks
		public void OnClick()
		{
			if (_callback != null)
				_callback();

			if (_closePopupOnPress)
			{
				if (ClosePopup != null)
					ClosePopup();
			}
		}
		#endregion

		#region Controls
		public void ProcessCustomData(IUIScreenData customData)
		{
			if (customData != null)
			{
				UIGenericPopupButtonData data = (UIGenericPopupButtonData)customData;

				_label.text = data.Label;
				_callback = data.Callback;
				_closePopupOnPress = data.ClosePopupOnPress;

				if (data.Color.HasValue)
					_colorElement.color = data.Color.Value;
			}
		}
		#endregion
	}
}