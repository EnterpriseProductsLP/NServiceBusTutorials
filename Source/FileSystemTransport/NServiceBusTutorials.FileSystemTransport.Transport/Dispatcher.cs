﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Transport;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal class Dispatcher : IDispatchMessages
    {
        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, ContextBag context)
        {
            foreach (var operation in outgoingMessages.UnicastTransportOperations)
            {
                var destinationBasePath = DirectoryBuilder.BuildBasePath(operation.Destination);
                var nativeMessageId = Guid.NewGuid().ToString();
                var bodyPath = Path.Combine(destinationBasePath, ".bodies", $"{nativeMessageId}.xml");

                var bodyDirectory = Path.GetDirectoryName(bodyPath);
                if (bodyDirectory != null)
                {
                    if (!Directory.Exists(bodyDirectory))
                    {
                        Directory.CreateDirectory(bodyDirectory);
                    }
                }

                File.WriteAllBytes(bodyPath, operation.Message.Body);

                var messageContents = new List<string>
                {
                    bodyPath,
                    HeaderSerializer.Serialize(operation.Message.Headers)
                };

                var messagePath = Path.Combine(destinationBasePath, $"{nativeMessageId}.txt");

                // Write to a temp file first so an atomic move can be done.
                // This avoids the file being locked when the receiver triest to process it.
                var tempFilePath = Path.GetTempFileName();
                File.WriteAllLines(tempFilePath, messageContents);
                File.Move(tempFilePath, messagePath);
            }

            return Task.CompletedTask;
        }
    }
}