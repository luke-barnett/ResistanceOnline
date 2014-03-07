using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace ResistanceOnline.Core.Test.SpecFlow
{
	public static class ContextAccess
	{
		public static GamePlay GamePlay
		{
			get
			{
                return ScenarioContext.Current.Get<GamePlay>("GamePlay");
			}
			set
			{
                ScenarioContext.Current["GamePlay"] = value;
			}
		}
	}
}