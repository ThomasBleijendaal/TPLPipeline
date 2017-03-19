using System;
using System.Collections.Generic;

namespace TPLPipeline.TestApp
{

	public class Logger
	{
		public static DateTime begin;
		public static Dictionary<string, int> order = new Dictionary<string, int>();

		public static void Start()
		{
			begin = DateTime.Now;
		}

		public static void Log(IEnumerable<IJobElement> items, string action)
		{
			foreach (var item in items)
			{
				Log(item, action);
			}
		}

		public static void Log(IJobElement item, string action)
		{
			//Console.WriteLine($"{item.Job.Id} :: {action} {item.Element}.");

			if (!order.ContainsKey(action))
			{
				order.Add(action, 1);
			}
			else
			{
				order[action]++;
			}

			//item.AddEvent((int)(DateTime.Now - begin).TotalMilliseconds, order[action], action);
		}

		public static void Log(IPipelineJob item, string action)
		{
			Log(item.Elements(), action);
		}
	}
}
