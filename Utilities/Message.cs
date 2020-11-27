﻿using System;
using System.Reflection;

namespace VectorChat.Utilities
{
	/// <summary>
	/// Basic struct which represents a single message. This struct is <c>Serializable</c>
	/// </summary>
	[Serializable]
	public struct Message
	{
		public string content { get; set; }
		public DateTime timestamp { get; set; }
		public string fromID { get; set; }
		public uint groupID { get; set; }

		/// <returns>Formatted string which shows timestamp, user ID and message contents </returns>
		public override string ToString() => $"[{this.timestamp.ToLongTimeString()}] {this.fromID} : {this.content}";

		public override Int32 GetHashCode()
		{
			Int32 hashBase = 15478363;
			unchecked
			{
				foreach (PropertyInfo info in this.GetType().GetProperties())
					hashBase = hashBase * 486187739 + (info.GetValue(this)).GetHashCode();

				return hashBase;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is Message message &&
				   content == message.content &&
				   fromID == message.fromID &&
				   timestamp == message.timestamp;
		}
	}
}
