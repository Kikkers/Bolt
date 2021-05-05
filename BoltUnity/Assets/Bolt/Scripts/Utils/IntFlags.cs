namespace Extensions.IntFlags
{
	public static class IntFlags
	{
		public static void SetFlag(this ref int flags, int flag, bool newValue)
		{
			if (newValue)
				flags |= flag;
			else
				flags &= ~flag;
		}

		public static bool HasFlag(this ref int flags, int flag)
		{
			return (flags & flag) != 0;
		}

		public static bool HasAnyFlag(this ref int flags, int flagMask)
		{
			return HasFlag(ref flags, flagMask);
		}

		public static bool HasAllFlags(this ref int flags, int flagMask)
		{
			return (flags & flagMask) == flagMask;
		}
	}
}
