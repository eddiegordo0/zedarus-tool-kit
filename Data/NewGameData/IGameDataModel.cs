﻿using System;

namespace Zedarus.Toolkit.Data.New.Game
{
	public interface IGameDataModel
	{
		#if UNITY_EDITOR
		int ID { get; }
		void RenderForm(bool included);
		string ListName { get; }
		void CopyValuesFrom(IGameDataModel data);
		#endif
	}
}

