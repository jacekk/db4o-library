using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
	class StaticData
	{
		public static List<ComboBoxElement> publicationTypes = new List<ComboBoxElement>()
		{
			new ComboBoxElement(){ Key='A',Value="Artykuł" },
			new ComboBoxElement(){ Key='K',Value="Książka" }
		};
	}
}
