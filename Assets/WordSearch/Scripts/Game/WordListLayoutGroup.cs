using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BBG.WordSearch
{
	public class WordListLayoutGroup : LayoutGroup
	{
		#region Insepector Variables

		public int spacing;
		public int rows;

		#endregion

		#region Unity Methods

		public override void CalculateLayoutInputHorizontal()
		{
			CalculateLayoutInputForAxis(0);
		}

		public override void CalculateLayoutInputVertical()
		{
			CalculateLayoutInputForAxis(1);
		}

		public override void SetLayoutHorizontal()
		{
			SetLayoutAlongAxis(0);
		}

		public override void SetLayoutVertical()
		{
			SetLayoutAlongAxis(1);
		}

		#endregion

		#region Private Methods

		private void CalculateLayoutInputForAxis(int axis)
		{
			float preferredSize = 0;

			if (axis == 0)
			{
				float columnPreferredSize 	= GetRowPreferredWidth();
				float tempPreferredSize		= 0;

				for (int i = 0; i < transform.childCount; i++)
				{
					RectTransform child = transform.GetChild(i) as RectTransform;

					tempPreferredSize += LayoutUtility.GetPreferredSize(child, 0) + spacing;

					if (tempPreferredSize >= columnPreferredSize)
					{
						preferredSize		= Mathf.Max(preferredSize, tempPreferredSize);
						tempPreferredSize	= 0;
					}
				}
			}
			else if (transform.childCount > 0)
			{
				preferredSize = LayoutUtility.GetPreferredSize(transform.GetChild(0) as RectTransform, 1) * rows + spacing * (rows - 1);
			}

			preferredSize += (axis == 0 ? m_Padding.horizontal : m_Padding.vertical);

			SetLayoutInputForAxis(preferredSize, preferredSize, preferredSize, axis);
		}

		private void SetLayoutAlongAxis(int axis)
		{
			float	preferredWidth	= GetRowPreferredWidth();
			float	xStartOffset	= GetStartOffset(0, 0);
			float	yStartOffset	= GetStartOffset(1, 0);
			Vector2	position		= new Vector2(xStartOffset, yStartOffset);

			for (int i = 0; i < transform.childCount; i++)
			{
				RectTransform child = transform.GetChild(i) as RectTransform;

				float childPreferredWidth = LayoutUtility.GetPreferredSize(child, 0);

				SetChildAlongAxis(child, axis, position[axis], LayoutUtility.GetPreferredSize(child, axis));

				position.x += childPreferredWidth + spacing;

				if (position.x - xStartOffset >= preferredWidth)
				{
					position.x = xStartOffset;
					position.y += LayoutUtility.GetPreferredSize(child, 1) + spacing;
				}
			}
		}

		/// <summary>
		/// Gets the preferred size of all rows, this is the total width of all elements divided by the number of rows
		/// </summary>
		/// <returns>The row preferred width.</returns>
		private float GetRowPreferredWidth()
		{
			float totalSize = 0;

			for (int i = 0; i < transform.childCount; i++)
			{
				RectTransform child = transform.GetChild(i) as RectTransform;

				if (i != 0)
				{
					totalSize += spacing;
				}

				totalSize += LayoutUtility.GetPreferredSize(child, 0);
			}

			return totalSize / (float)rows;
		}

		#endregion
	}
}
