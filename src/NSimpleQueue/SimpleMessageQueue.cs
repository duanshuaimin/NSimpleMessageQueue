﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace NSimpleQueue {
  public class SimpleMessageQueue : ISimpleMessageQueue {
    private readonly InnerSimpleQueue _innerQueue;

    public SimpleMessageQueue(DirectoryInfo directory) {
      if (directory == null)
        throw new ArgumentNullException("directory");

      if (!directory.Exists)
        throw new ArgumentException("directory does not exist");

      _innerQueue = MemoryQueueLookup.Current.CreateOrGetQueue(directory.FullName);
      Formatter = new BinaryFormatter();
    }

    public IFormatter Formatter { get; set; }
    public long Count { get { return _innerQueue.Count; } }

    public static void Create(string directoryPath, IFormatter formatter) {
      if (string.IsNullOrEmpty(directoryPath))
        throw new ArgumentNullException(directoryPath);

      var directory = new DirectoryInfo(directoryPath);

      if (!directory.Exists)
        directory.Create();

      directory.CreateSubdirectory(SimpleMessageQueueConstants.DataDirectoryName);
      directory.CreateSubdirectory(SimpleMessageQueueConstants.MetaDirectoryName);

      // Saveformatter
    }

    public static void Delete(string directoryPath) {
      Directory.Delete(directoryPath, true);
    }

    public ISimpleMessageQueueTransaction BeginTransaction() {
      return _innerQueue.BeginTransaction();
    }

    public SimpleQueueMessage Receive(CancellationToken cancellationToken, ISimpleMessageQueueTransaction transaction)
    {
      return _innerQueue.Receive(cancellationToken, transaction);
    }

    public SimpleQueueMessage Receive(CancellationToken cancellationToken) {
      return _innerQueue.Receive(cancellationToken);
    }

    public void Enqueue(object item) {
      _innerQueue.Enqueue(item);
    }

    public void Dispose() {
      MemoryQueueLookup.Current.UnSubscribe(_innerQueue);
    }

    public static bool Exists(string queuePath)
    {
      return Directory.Exists(queuePath);
    }
  }
}