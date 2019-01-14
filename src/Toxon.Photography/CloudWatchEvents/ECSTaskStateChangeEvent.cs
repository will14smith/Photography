using System;
using System.Collections.Generic;
using System.Text;

namespace Toxon.Photography.CloudWatchEvents
{
    /// <summary>
    /// /// AWS ECS task state change event
    /// http://docs.aws.amazon.com/config/latest/developerguide/evaluate-config_develop-rules.html
    /// http://docs.aws.amazon.com/config/latest/developerguide/evaluate-config_develop-rules_example-events.html
    /// https://docs.aws.amazon.com/AmazonECS/latest/developerguide/ecs_cwe_events.html
    /// </summary>
    public class ECSTaskStateChangeEvent : CloudWatchEvent<Task>
    {

    }
}
