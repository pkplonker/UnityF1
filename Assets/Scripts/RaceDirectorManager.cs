using System;
using System.Collections.Generic;
using System.Drawing;
using OpenF1CSharp;

namespace DefaultNamespace
{
	public class RaceDirectorManager : IUpdateable
	{
		private readonly List<RaceControlData> raceControlData;
		 
		public RaceDirectorManager(List<RaceControlData> raceControlData)
		{
			this.raceControlData = raceControlData;
		}

		public bool Tick(DateTime currentTime)
		{
			
			return true;
		}
	}
}