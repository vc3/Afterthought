using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NotifyPropertyChanged.UnitTest.Target
{
	/// <summary>
	/// Example of a type that already implements <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	[NotifyPropertyChanged]
	public class Soda
	{
		public int Cans { get; set; }

		public string Type { get; set; }
	}
}
