using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace Codeglue
{
	public interface IMessageData { }

	public interface IMessage : IMessageData { }

	public interface IRequest : IMessageData { }

	public interface IHandler<TMessageData> where TMessageData : IMessageData
	{
		void Handle(TMessageData data);
	}

	/// <summary>
	/// Provides a global messaging system grouped by <see cref="IMessage"/> implementations.
	/// <para>
	/// When requiring data globally from handlers, it's better to use <see cref="Request"/> instead.
	/// </para>
	/// <para>
	/// Messages are structs, which avoids any difficulties related to allocation, boxing and ownership 
	/// </para>
	/// <para>
	/// Handlers need to be classes in order to be tracked.
	/// As with any event system, a handler is itself responsible for registering and unregistering.
	/// </para>
	/// </summary>
	public static class Message
	{
		/// <summary>
		/// Register a handler to start listening to broadcasts of this message type.
		/// </summary>
		public static void Register<TMessage>([NotNull] IHandler<TMessage> handler)
			where TMessage : struct, IMessage
		{
			MessagingProcessor<TMessage>.Register(handler);
		}

		/// <summary>
		/// Check to see if this handler is registered.
		/// </summary>
		public static bool IsRegistered<TMessage>([NotNull] IHandler<TMessage> handler)
			where TMessage : struct, IMessage
		{
			return MessagingProcessor<TMessage>.IsRegistered(handler);
		}

		/// <summary>
		/// Unregister a handler to stop listening to broadcasts of the message type.
		/// </summary>
		public static void Unregister<TMessage>([NotNull] IHandler<TMessage> handler)
			where TMessage : struct, IMessage
		{
			MessagingProcessor<TMessage>.Unregister(handler);
		}

		/// <summary>
		/// Clears all the handlers of this message.
		/// This may not be called when handling a message.
		/// </summary>
		public static void UnregisterAll<TMessage>()
			where TMessage : struct, IMessage
		{
			MessagingProcessor<TMessage>.UnregisterAll();
		}

		/// <summary>
		/// Returns how many handlers are registered.
		/// </summary>
		public static int CountRegistered<TMessage>()
			where TMessage : struct, IMessage
		{
			return MessagingProcessor<TMessage>.CountRegistered();
		}

		/// <summary>
		/// Broadcast a message with the given data.
		/// </summary>
		public static void Broadcast<TMessage>(TMessage data = default)
			where TMessage : struct, IMessage
		{
			List<IHandler<TMessage>> handlers = MessagingProcessor<TMessage>.handlers;

			// don't forward this method to a helper class to avoid boxing of the data
			for (int i = handlers.Count - 1; i >= 0; --i)
			{
				handlers[i].Handle(data);
			}
		}

		/// <summary>
		/// Broadcast a message with the given data on Unity's main thread.
		/// The message may not execute immediately, but will instead wait until the currently active thread is the main one.
		/// This is a non-blocking call and will return immediately, even if the message was not yet broadcast.
		/// </summary>
		public static void BroadcastOnUnityThread<TMessage>(TMessage data = default)
			where TMessage : struct, IMessage
		{
			MessagingProcessor<TMessage>.BroadcastOnUnityThread(_ => Broadcast(data));
		}
	}

	/// <summary>
	/// Provides a global request system grouped by <see cref="IRequest"/> implementations.
	/// <para>
	/// For broadcasting data globally without needing a response, it's better to use <see cref="Message"/> instead.
	/// </para>
	/// <para>
	/// Requests are classes, instances should be owned (and reused if needed) by the broadcaster.
	/// Request handlers can fulfill the the request by modifying this same instance. 
	/// </para>
	/// <para>
	/// Handlers need to be classes in order to be tracked.
	/// As with any event system, a handler is itself responsible for registering and unregistering.
	/// </para>
	/// </summary>
	public static class Request
	{
		/// <summary>
		/// Register a handler to start listening to broadcasts of this request type.
		/// </summary>
		public static void Register<TRequest>([NotNull] IHandler<TRequest> handler)
			where TRequest : class, IRequest
		{
			MessagingProcessor<TRequest>.Register(handler);
		}

		/// <summary>
		/// Check to see if this handler is registered.
		/// </summary>
		public static bool IsRegistered<TRequest>([NotNull] IHandler<TRequest> handler)
			where TRequest : class, IRequest
		{
			return MessagingProcessor<TRequest>.IsRegistered(handler);
		}

		/// <summary>
		/// Unregister a handler to stop listening to broadcasts of the request type.
		/// </summary>
		public static void Unregister<TRequest>([NotNull] IHandler<TRequest> handler)
			where TRequest : class, IRequest
		{
			MessagingProcessor<TRequest>.Unregister(handler);
		}

		/// <summary>
		/// Clears all the handlers of this request.
		/// This may not be called when handling a request.
		/// </summary>
		public static void UnregisterAll<TRequest>()
			where TRequest : class, IRequest
		{
			MessagingProcessor<TRequest>.UnregisterAll();
		}

		/// <summary>
		/// Returns how many handlers are registered.
		/// </summary>
		public static int CountRegistered<TRequest>()
			where TRequest : class, IRequest
		{
			return MessagingProcessor<TRequest>.CountRegistered();
		}

		/// <summary>
		/// Broadcast a request.
		/// The request may not be null
		/// </summary>
		public static void Broadcast<TRequest>([NotNull] TRequest request)
			where TRequest : class, IRequest
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request), "[Messaging] Request cannot be null!");
			}

			List<IHandler<TRequest>> handlers = MessagingProcessor<TRequest>.handlers;
			for (int i = handlers.Count - 1; i >= 0; --i)
			{
				handlers[i].Handle(request);
			}
		}

		/// <summary>
		/// Broadcast a request with the given data on Unity's main thread.
		/// The request broadcast will wait until the currently active thread is the main one.
		/// This is a blocking call and will return only after the request was broadcast.
		/// </summary>
		public static void BroadcastOnUnityThread<TRequest>([NotNull] TRequest request)
			where TRequest : class, IRequest
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request), "[Messaging] Request cannot be null!");
			}

			MessagingProcessor<TRequest>.AwaitBroadcastOnUnityThread(_ => Broadcast(request));
		}
	}

	internal static class MessagingProcessor<TMessageData> where TMessageData : IMessageData
	{
		// List is used for cheap iteration at the cost of O(n) removal, which should happen less often.
		internal static readonly List<IHandler<TMessageData>> handlers = new List<IHandler<TMessageData>>();

		internal static void Register([NotNull] IHandler<TMessageData> handler)
		{
			if (IsRegistered(handler))
			{
				Debug.LogWarning($"[Messaging] Supplied handler {handler} to be added is already registered");
				return;
			}

			handlers.Add(handler);
		}


		internal static bool IsRegistered([NotNull] IHandler<TMessageData> handler)
		{
			ValidateHandler(handler);
			return handlers.Contains(handler);
		}

		internal static void Unregister([NotNull] IHandler<TMessageData> handler)
		{
			ValidateHandler(handler);

			bool wasRemoved = handlers.Remove(handler);

			if (!wasRemoved && Application.isPlaying)
			{
				Debug.LogWarning($"[Messaging] Supplied handler {handler} to be removed was not registered");
			}
		}

		internal static void UnregisterAll() => handlers.Clear();

		internal static int CountRegistered() => handlers.Count;

		internal static void BroadcastOnUnityThread(SendOrPostCallback broadcastCallback)
		{
			SynchronizationContextUtils.UnitySynchronizationContext.Post(broadcastCallback, null);
		}

		internal static void AwaitBroadcastOnUnityThread(SendOrPostCallback broadcastCallback)
		{
			SynchronizationContextUtils.UnitySynchronizationContext.Send(broadcastCallback, null);
		}

		private static void ValidateHandler([NotNull] IHandler<TMessageData> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler), "[Messaging] Supplied handler must be non-null");
			}

			Type handlerType = handler.GetType();
			if (!handlerType.IsClass)
			{
				throw new Exception($"[Messaging] Supplied handler {handlerType} must be a class (reference type)");
			}
		}
	}

	/// <summary>
	/// Convenience registering methods for <see cref="Message"/> and <see cref="Request"/>.
	/// <para>
	/// Uses <see cref="System.Reflection"/> internally with caching to simplify the 
	/// registering/unregistering process, at the cost of low overhead, and less fine control.
	/// </para>
	/// </summary>
	public static class Messaging
	{
		private readonly static Dictionary<Type, RegistrationMethods[]> methodsMap = new Dictionary<Type, RegistrationMethods[]>();

		[ThreadStatic] private static object[] argContainer = new object[1];

		private struct RegistrationMethods
		{
			public MethodInfo registerMethod;
			public MethodInfo unregisterMethod;
		}

		private struct Dummy : IMessageData { }

		private static RegistrationMethods[] FindRegistrationMethods(Type handlerType)
		{
			List<RegistrationMethods> methodsForHandler = new List<RegistrationMethods>();
			Type[] interfaces = handlerType.GetInterfaces();
			foreach (Type i in interfaces)
			{
				if (!i.IsGenericType)
					continue;

				Type baseDef = i.GetGenericTypeDefinition();
				if (baseDef != typeof(IHandler<>))
					continue;

				Type argumentType = i.GetGenericArguments()[0];
				Type processorType = typeof(MessagingProcessor<>).MakeGenericType(argumentType);

				RegistrationMethods singleHandlerMethods = new RegistrationMethods
				{
					registerMethod = processorType.GetMethod(nameof(MessagingProcessor<Dummy>.Register), BindingFlags.NonPublic | BindingFlags.Static),
					unregisterMethod = processorType.GetMethod(nameof(MessagingProcessor<Dummy>.Unregister), BindingFlags.NonPublic | BindingFlags.Static)
				};
				methodsForHandler.Add(singleHandlerMethods);
			}
			return methodsForHandler.ToArray();
		}

		/// <summary>
		/// Registers the supplied object for all handlers its implementing. 
		/// <para>
		/// This is a convenience method, specific handler registering should instead be done using 
		/// <see cref="Message.Register{TMessage}(IHandler{TMessage})"/> and <see cref="Request.Register{TRequest}(IHandler{TRequest})"/>
		/// </para>
		/// </summary>
		public static void RegisterAll(object handlerOwner)
		{
			Type handlerType = handlerOwner.GetType();
			if (!methodsMap.TryGetValue(handlerType, out RegistrationMethods[] registryMethods))
			{
				methodsMap[handlerType] = registryMethods = FindRegistrationMethods(handlerType);
				if (registryMethods.Length == 0)
					Debug.LogWarning($"Failed to register object of type {handlerType.Name}: No IHandler uses!");
			}

			argContainer[0] = handlerOwner;
			foreach (RegistrationMethods methodsForHandler in registryMethods)
			{
				methodsForHandler.registerMethod.Invoke(null, argContainer);
			}
		}

		/// <summary>
		/// Unregisters the supplied object for all handlers its implementing. 
		/// <para>
		/// This is a convenience method, specific handler unregistering should be done using 
		/// <see cref="Message.Unregister{TMessage}(IHandler{TMessage})"/> and <see cref="Request.Unregister{TRequest}(IHandler{TRequest})"/>
		/// </para>
		/// </summary>
		public static void UnregisterAll(object handlerOwner)
		{
			Type handlerType = handlerOwner.GetType();
			if (!methodsMap.TryGetValue(handlerType, out RegistrationMethods[] registryMethods))
			{
				methodsMap[handlerType] = registryMethods = FindRegistrationMethods(handlerType);
			}

			argContainer[0] = handlerOwner;
			foreach (RegistrationMethods methodsForHandler in registryMethods)
			{
				methodsForHandler.unregisterMethod.Invoke(null, argContainer);
			}
		}
	}
}
