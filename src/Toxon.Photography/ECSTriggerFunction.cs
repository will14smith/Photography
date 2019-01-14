using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.ECS;
using Amazon.ECS.Model;
using KeyValuePair = Amazon.ECS.Model.KeyValuePair;
using Task = System.Threading.Tasks.Task;

namespace Toxon.Photography
{
    public class ECSTriggerFunction
    {
        private readonly IAmazonECS _ecs;

        private readonly string _taskDefinition;
        private readonly string _clusterName;
        private readonly string _containerName;
        private readonly List<string> _subnets;
        private readonly List<string> _securityGroups;

        public ECSTriggerFunction()
        {
            _ecs = new AmazonECSClient();

            _taskDefinition = Environment.GetEnvironmentVariable("TASK_DEFINITION");
            _clusterName = Environment.GetEnvironmentVariable("CLUSTER_NAME");
            _containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");
            _subnets = Split(Environment.GetEnvironmentVariable("TASK_SUBNETS"));
            _securityGroups = Split(Environment.GetEnvironmentVariable("TASK_SECURITY_GROUPS"));
        }

        private static List<string> Split(string input)
        {
            return input
                .Split(",")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }

        public async Task Handle(Stream input)
        {
            string inputString;
            using (var reader = new StreamReader(input))
            {
                inputString = reader.ReadToEnd();
            }

            await _ecs.RunTaskAsync(new RunTaskRequest
            {
                LaunchType = LaunchType.FARGATE,
                TaskDefinition = _taskDefinition,
                Cluster = _clusterName,
                NetworkConfiguration = new NetworkConfiguration
                {
                    AwsvpcConfiguration = new AwsVpcConfiguration
                    {
                        Subnets = _subnets,
                        SecurityGroups = _securityGroups,
                        AssignPublicIp = AssignPublicIp.ENABLED
                    }
                },
                Overrides = new TaskOverride
                {
                    ContainerOverrides = new List<ContainerOverride>
                    {
                        new ContainerOverride
                        {
                            Name = _containerName,
                            Environment = new List<KeyValuePair>
                            {
                                new KeyValuePair { Name = "INPUT", Value = inputString }
                            }
                        }
                    }
                }
            });
        }
    }
}
