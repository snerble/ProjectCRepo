﻿namespace API.HTTP.Filters
{
	[FilterUrl("/dev/")]
	public sealed class BackendFilter : Filter
	{
		protected override void Main()
		{
			Program.Log.Debug("TODO Implement authorization check.");
		}
	}
}