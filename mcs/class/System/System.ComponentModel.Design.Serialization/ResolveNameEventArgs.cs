//
// System.ComponentModel.Design.Serialization.ResolveNameEventArgs.cs
//
// Author:
//   Alejandro S�nchez Acosta (raciel@gnome.org)
//
// (C) Alejandro S�nchez Acosta
//

namespace System.ComponentModel.Design.Serialization
{
	public class ResolveNameEventArgs : EventArgs
	{

		private string name;
	
		public ResolveNameEventArgs (string name) {
			this.name = name;
		}

		public string Name {
			get {
				return this.name;
			}
		}

		public object Value {
			get {
				return this.name;
			}

			set {
				name = (string) value;
			}
		}			
	}
}
