# Create the admin user
rabbitmqctl -n rabbit@TSESTMRMQ01CORP delete_user admin
rabbitmqctl -n rabbit@TSESTMRMQ01CORP add_user admin Administr@tor
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_user_tags admin administrator

rabbitmqctl -n rabbit@TSESTMRMQ04GULF delete_user admin
rabbitmqctl -n rabbit@TSESTMRMQ04GULF add_user admin Administr@tor
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_user_tags admin administrator



# Create the NServiceBusTutorials user
rabbitmqctl -n rabbit@TSESTMRMQ01CORP delete_user NServiceBusTutorials
rabbitmqctl -n rabbit@TSESTMRMQ01CORP add_user NServiceBusTutorials 1breakthings!
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_user_tags NServiceBusTutorials administrator

rabbitmqctl -n rabbit@TSESTMRMQ04GULF delete_user NServiceBusTutorials
rabbitmqctl -n rabbit@TSESTMRMQ04GULF add_user NServiceBusTutorials 1breakthings!
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_user_tags NServiceBusTutorials administrator



# Create the NServiceBusTutorials virtual host
rabbitmqctl -n rabbit@TSESTMRMQ01CORP delete_vhost NServiceBusTutorials
rabbitmqctl -n rabbit@TSESTMRMQ01CORP add_vhost NServiceBusTutorials

rabbitmqctl -n rabbit@TSESTMRMQ04GULF delete_vhost NServiceBusTutorials
rabbitmqctl -n rabbit@TSESTMRMQ04GULF add_vhost NServiceBusTutorials



# Grant user permissions on the virtual host
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_permissions -p NServiceBusTutorials admin ".*" ".*" ".*"
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_permissions -p NServiceBusTutorials NServiceBusTutorials ".*" ".*" ".*"

rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_permissions -p NServiceBusTutorials admin ".*" ".*" ".*"
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_permissions -p NServiceBusTutorials NServiceBusTutorials ".*" ".*" ".*"



# Create federation upstreams
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_parameter -p NServiceBusTutorials federation-upstream federate-gulf '{""uri"": [""amqp://svc-RabbitMQ:RabbitMQpwd@TSESTMRMQ04GULF"", ""amqp://svc-RabbitMQ:RabbitMQpwd@TSESTMRMQ05GULF"", ""amqp://svc-RabbitMQ:RabbitMQpwd@TSESTMRMQ06GULF""], ""max-hops"":10}'

rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_parameter -p NServiceBusTutorials federation-upstream federate-gulf '{""uri"": [""amqp://svc-RabbitMQ:RabbitMQpwd@TSESTMRMQ01CORP"", ""amqp://svc-RabbitMQ:RabbitMQpwd@TSESTMRMQ02CORP"", ""amqp://svc-RabbitMQ:RabbitMQpwd@TSESTMRMQ03CORP""], ""max-hops"":10}'




# Add policies for high availablity on exchnages and high availability + federation on queues
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_policy -p NServiceBusTutorials --priority 0 --apply-to "exchanges" ha-exchanges ".*" '{""ha-mode"": ""all""}'
rabbitmqctl -n rabbit@TSESTMRMQ01CORP set_policy -p NServiceBusTutorials --priority 0 --apply-to "queues" autosynced-ha-federation-queues ".*" '{""ha-mode"": ""all"", ""ha-sync-mode"": ""automatic"", ""federation-upstream-set"": ""all""}'

rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_policy -p NServiceBusTutorials --priority 0 --apply-to "exchanges" ha-exchanges ".*" '{""ha-mode"": ""all""}'
rabbitmqctl -n rabbit@TSESTMRMQ04GULF set_policy -p NServiceBusTutorials --priority 0 --apply-to "queues" autosynced-ha-federation-queues ".*" '{""ha-mode"": ""all"", ""ha-sync-mode"": ""automatic"", ""federation-upstream-set"": ""all""}'
