using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLPipeline
{

	public interface IPipeline<Tjob> where Tjob : IPipelineJob
	{
		void Post(Tjob job);
		Task PostAsync(Tjob job);
	}
}
