using System.Threading;
using UnityEngine;

namespace Codeglue
{
	public static class SynchronizationContextUtils
	{
		public static SynchronizationContext UnitySynchronizationContext { get; private set; }
		public static int UnityThreadID { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			UnitySynchronizationContext = SynchronizationContext.Current;
			UnityThreadID = Thread.CurrentThread.ManagedThreadId;
		}
	}
}