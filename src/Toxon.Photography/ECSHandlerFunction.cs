using System.ComponentModel.DataAnnotations;
using System.Linq;
using Toxon.Photography.CloudWatchEvents;

namespace Toxon.Photography
{
    public class ECSHandlerFunction
    {
        public void Handle(ECSTaskStateChangeEvent input)
        {
            var task = input.Detail;

            var taskArn = task.TaskDefinitionArn;
            var state = task.LastStatus;

            var container = task.Containers.First();
            var exitCode = container.ExitCode;

            // TODO log / track this
        }
    }
}
