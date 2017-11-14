using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Mobcast.Coffee.UI
{
	public class ButtonExDemo : MonoBehaviour
	{
		public Text text;

		public void AddText (string str)
		{
			text.text += str;
		}
	}
}