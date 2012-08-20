//
// Mono.ILASM.ILTokenizingException
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright 2004 Novell, Inc (http://www.novell.com)
//
using System;
using System.Runtime.Serialization;

namespace Mono.ILDasm.Tests {
	[Serializable]
	internal class ILTokenizingException : Exception {
		public string Token { get; private set; }

		public ILTokenizingException (Location location, string token)
		{
			Token = token;
		}

		public ILTokenizingException (Location location, string token, Exception inner)

		{
		}

		protected ILTokenizingException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
