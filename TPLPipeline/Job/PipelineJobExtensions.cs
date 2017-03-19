using System.Collections.Generic;
using System.Linq;

namespace TPLPipeline
{
	public static class PipelineJobExtensions
	{
		public static IEnumerable<T> GetData<T>(this IEnumerable<IPipelineJobElement<T>> items)
		{
			var result = new List<T>();

			foreach (var item in items)
			{
				result.Add(item.GetData());
			}

			return result;
		}

		public static Tjob GetJob<Tjob>(this IEnumerable<IJobElement> items)
			where Tjob : IPipelineJob
		{
			return (Tjob)items.First().Job;
		}
	}
}
