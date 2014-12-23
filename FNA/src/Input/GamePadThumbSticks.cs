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
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public struct GamePadThumbSticks
	{
		#region Public Properties

		public Vector2 Left
		{
			get
			{
				return left;
			}
			internal set
			{
				if (value.LengthSquared() > 1f)
				{
					left = Vector2.Normalize(value);
				}
				else
				{
					left = value;
				}
			}
		}
		public Vector2 Right
		{
			get
			{
				return right;
			}
			internal set
			{
				if (value.LengthSquared() > 1f)
				{
					right = Vector2.Normalize(value);
				}
				else
				{
					right = value;
				}
			}
		}

		#endregion

		#region Private Variables

		private Vector2 left;
		private Vector2 right;

		#endregion

		#region Public Constructor

		public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition) : this()
		{
			Left = leftPosition;
			Right = rightPosition;
		}

		#endregion

		#region Internal Methods

		internal void ApplyDeadZone(GamePadDeadZone dz, float size)
		{
			switch (dz)
			{
				case GamePadDeadZone.None:
					break;
				case GamePadDeadZone.IndependentAxes:
					if (Math.Abs(left.X) < size)
					{
						left.X = 0f;
					}
					if (Math.Abs(left.Y) < size)
					{
						left.Y = 0f;
					}
					if (Math.Abs(right.X) < size)
					{
						right.X = 0f;
					}
					if (Math.Abs(right.Y) < size)
					{
						right.Y = 0f;
					}
					break;
				case GamePadDeadZone.Circular:
					if (left.LengthSquared() < size * size)
					{
						left = Vector2.Zero;
					}
					if (right.LengthSquared() < size * size)
					{
						right = Vector2.Zero;
					}
					break;
			}
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadThumbSticks"/>
		/// are equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator ==(GamePadThumbSticks left, GamePadThumbSticks right)
		{
			return (left.left == right.left) && (left.right == right.right);
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadThumbSticks"/>
		/// are not equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are not equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator !=(GamePadThumbSticks left, GamePadThumbSticks right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>
		/// True if <paramref name="obj"/> is a <see cref="GamePadThumbSticks"/> and has the
		/// same value as this instance; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is GamePadThumbSticks) && (this == (GamePadThumbSticks) obj);
		}

		public override int GetHashCode ()
		{
			return this.Left.GetHashCode() + 37 * this.Right.GetHashCode();
		}

		#endregion
	}
}
