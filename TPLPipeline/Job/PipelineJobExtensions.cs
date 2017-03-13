using System.Collections.Generic;
using System.Linq;

namespace TPLPipeLine
{
	public static class PipelineJobExtensions
	{
		public static IEnumerable<T> GetData<T>(this IEnumerable<IPipelineJobElement> items)
		{
			var result = new List<T>();

			foreach (var item in items)
			{
				result.Add(item.GetData<T>());
			}

			return result;
		}

		public static T GetJob<T>(this IEnumerable<IPipelineJobElement> items) where T: IPipelineJob
		{
			return (T)items.First()?.Job;
		}
	}
}
