# Create the NServiceBusTutorials user
rabbitmqctl -n rabbit@TSESTMRMQ01CORP add_user NServiceBusTutorials 1breakthings!
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_user_tags NServiceBusTutorials administrator

# Create the NServiceBusTutorials virtual host
rabbitmqctl -n rabbit@TSESTMRMQ01CORP add_vhost NServiceBusTutorials

# Grant user permissions on the virtual host
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_permissions -p NServiceBusTutorials NServiceBusTutorials ".*" ".*" ".*"
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_permissions -p NServiceBusTutorials eburcham ".*" ".*" ".*"
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_permissions -p NServiceBusTutorials test ".*" ".*" ".*"

# Create federation upstreams
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_parameter -p NServiceBusTutorials federation-upstream federate-gulf '{""uri"": [""amqp://test:1breakthings!@TSESTMRMQ04GULF"", ""amqp://test:1breakthings!@TSESTMRMQ05GULF"", ""amqp://test:1breakthings!@TSESTMRMQ06GULF""], ""max-hops"":10}'

# Add policies for high availablity on exchnages and high availability + federation on queues
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_policy -p NServiceBusTutorials --priority 0 --apply-to "exchanges" ha-exchanges ".*" '{""ha-mode"": ""all""}'
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_policy -p NServiceBusTutorials --priority 0 --apply-to "queues" ha-federation-queues ".*" '{""ha-mode"": ""all"", ""federation-upstream-set"": ""all""}'





# Create the NServiceBusTutorials user
rabbitmqctl -n rabbit@TSESTMRMQ04GULF add_user NServiceBusTutorials 1breakthings!
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_user_tags NServiceBusTutorials administrator

# Create the NServiceBusTutorials virtual host
rabbitmqctl -n rabbit@TSESTMRMQ04GULF add_vhost NServiceBusTutorials

# Grant user permissions on the virtual host
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_permissions -p NServiceBusTutorials NServiceBusTutorials ".*" ".*" ".*"
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_permissions -p NServiceBusTutorials eburcham ".*" ".*" ".*"
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_permissions -p NServiceBusTutorials test ".*" ".*" ".*"

# Create federation upstreams
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_parameter -p NServiceBusTutorials federation-upstream federate-gulf '{""uri"": [""amqp://test:1breakthings!@TSESTMRMQ01CORP"", ""amqp://test:1breakthings!@TSESTMRMQ02CORP"", ""amqp://test:1breakthings!@TSESTMRMQ03CORP""], ""max-hops"":10}'

# Add policies for high availablity on exchnages and high availability + federation on queues
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_policy -p NServiceBusTutorials --priority 0 --apply-to "exchanges" ha-exchanges ".*" '{""ha-mode"": ""all""}'
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_policy -p NServiceBusTutorials --priority 0 --apply-to "queues" ha-federation-queues ".*" '{""ha-mode"": ""all"", ""federation-upstream-set"": ""all""}'
