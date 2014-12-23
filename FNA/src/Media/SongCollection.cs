#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
	{
		#region Public Properties

		public Song this[int index]
		{
			get
			{
				return innerlist[index];
			}
		}

		public int Count
		{
			get
			{
				return innerlist.Count;
			}
		}

		public bool IsReadOnly
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private List<Song> innerlist;

		#endregion

		#region Internal Constructor

		internal SongCollection(List<Song> songs)
		{
			IsReadOnly = false;
			innerlist = songs;
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			innerlist.Clear();
		}

		#endregion

		#region Public Methods

		public void Add(Song item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if (innerlist.Count == 0)
			{
				innerlist.Add(item);
				return;
			}

			for (int i = 0; i < innerlist.Count; i += 1)
			{
				if (item.TrackNumber < innerlist[i].TrackNumber)
				{
					innerlist.Insert(i, item);
					return;
				}
			}

			innerlist.Add(item);
		}

		public void Clear()
		{
			innerlist.Clear();
		}

		public SongCollection Clone()
		{
			return new SongCollection(new List<Song>(innerlist));
		}

		public bool Contains(Song item)
		{
			return innerlist.Contains(item);
		}

		public void CopyTo(Song[] array, int arrayIndex)
		{
			innerlist.CopyTo(array, arrayIndex);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		public IEnumerator<Song> GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		public int IndexOf(Song item)
		{
			return innerlist.IndexOf(item);
		}

		public bool Remove(Song item)
		{
			return innerlist.Remove(item);
		}

		#endregion
	}
}
