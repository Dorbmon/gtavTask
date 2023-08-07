//
// Copyright (C) 2007-2010 SlimDX Group
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GTA.Math
{
	/// <summary>
	/// Defines a 4x4 matrix.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Matrix : IEquatable<Matrix>
	{
		/// <summary>
		/// Gets or sets the element of the matrix that exists in the first row and first column.
		/// </summary>
		public float M11;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the first row and second column.
		/// </summary>
		public float M12;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the first row and third column.
		/// </summary>
		public float M13;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the first row and fourth column.
		/// </summary>
		public float M14;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the second row and first column.
		/// </summary>
		public float M21;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the second row and second column.
		/// </summary>
		public float M22;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the second row and third column.
		/// </summary>
		public float M23;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the second row and fourth column.
		/// </summary>
		public float M24;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the third row and first column.
		/// </summary>
		public float M31;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the third row and second column.
		/// </summary>
		public float M32;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the third row and third column.
		/// </summary>
		public float M33;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the third row and fourth column.
		/// </summary>
		public float M34;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the fourth row and first column.
		/// </summary>
		public float M41;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the fourth row and second column.
		/// </summary>
		public float M42;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the fourth row and third column.
		/// </summary>
		public float M43;

		/// <summary>
		/// Gets or sets the element of the matrix that exists in the fourth row and fourth column.
		/// </summary>
		public float M44;

		/// <summary>
		/// Initializes a new instance of the <see cref="Matrix"/> structure.
		/// </summary>
		/// <param name="values">The values to assign to the components of the matrix. This must be an array with sixteen elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than sixteen elements.</exception>
		public Matrix(float[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (values.Length != 16)
			{
				throw new ArgumentOutOfRangeException(nameof(values), "There must be sixteen and only sixteen input values for Matrix.");
			}

			M11 = values[0];
			M12 = values[1];
			M13 = values[2];
			M14 = values[3];

			M21 = values[4];
			M22 = values[5];
			M23 = values[6];
			M24 = values[7];

			M31 = values[8];
			M32 = values[9];
			M33 = values[10];
			M34 = values[11];

			M41 = values[12];
			M42 = values[13];
			M43 = values[14];
			M44 = values[15];
		}

		/// <summary>
		/// The identity <see cref="Matrix"/>.
		/// </summary>
		public static Matrix Identity => new Matrix() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };

		/// <summary>
		/// Gets or sets the component at the specified index.
		/// </summary>
		/// <value>The value of the matrix component, depending on the index.</value>
		/// <param name="row">The row of the matrix to access.</param>
		/// <param name="column">The column of the matrix to access.</param>
		/// <returns>The value of the component at the specified index.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 3].</exception>
		public float this[int row, int column]
		{
			get
			{
				if (row < 0 || row > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(row), "Rows and columns for matrices run from 0 to 3, inclusive.");
				}

				if (column < 0 || column > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(column), "Rows and columns for matrices run from 0 to 3, inclusive.");
				}

				int index = row * 4 + column;

				switch (index)
				{
					case 0: return M11;
					case 1: return M12;
					case 2: return M13;
					case 3: return M14;
					case 4: return M21;
					case 5: return M22;
					case 6: return M23;
					case 7: return M24;
					case 8: return M31;
					case 9: return M32;
					case 10: return M33;
					case 11: return M34;
					case 12: return M41;
					case 13: return M42;
					case 14: return M43;
					case 15: return M44;
				}

				return 0.0f;
			}

			set
			{
				if (row < 0 || row > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(row), "Rows and columns for matrices run from 0 to 3, inclusive.");
				}

				if (column < 0 || column > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(column), "Rows and columns for matrices run from 0 to 3, inclusive.");
				}

				int index = row * 4 + column;
				switch (index)
				{
					case 0: M11 = value; break;
					case 1: M12 = value; break;
					case 2: M13 = value; break;
					case 3: M14 = value; break;
					case 4: M21 = value; break;
					case 5: M22 = value; break;
					case 6: M23 = value; break;
					case 7: M24 = value; break;
					case 8: M31 = value; break;
					case 9: M32 = value; break;
					case 10: M33 = value; break;
					case 11: M34 = value; break;
					case 12: M41 = value; break;
					case 13: M42 = value; break;
					case 14: M43 = value; break;
					case 15: M44 = value; break;
				}
			}
		}

		/// <summary>
		/// Converts the matrix to an array of floats.
		/// </summary>
		public static Matrix FromArray(float[] floatArray) => new Matrix(floatArray);

		/// <summary>
		/// Gets a value indicating whether this instance is an identity matrix.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if this instance is an identity matrix; otherwise, <see langword="false" />.
		/// </value>
		public bool IsIdentity => Equals(Identity);

		/// <summary>
		/// Gets a value indicating whether this instance has an inverse matrix.
		/// </summary>
		public bool HasInverse => Determinant() != 0.0f;

		/// <summary>
		/// Calculates the determinant of the matrix.
		/// </summary>
		/// <returns>The determinant of the matrix.</returns>
		public float Determinant()
		{
			float temp1 = (M33 * M44) - (M34 * M43);
			float temp2 = (M32 * M44) - (M34 * M42);
			float temp3 = (M32 * M43) - (M33 * M42);
			float temp4 = (M31 * M44) - (M34 * M41);
			float temp5 = (M31 * M43) - (M33 * M41);
			float temp6 = (M31 * M42) - (M32 * M41);

			return ((((M11 * (((M22 * temp1) - (M23 * temp2)) + (M24 * temp3))) - (M12 * (((M21 * temp1) -
				(M23 * temp4)) + (M24 * temp5)))) + (M13 * (((M21 * temp2) - (M22 * temp4)) + (M24 * temp6)))) -
				(M14 * (((M21 * temp3) - (M22 * temp5)) + (M23 * temp6))));
		}

		private float Det3x3(float M11, float M12, float M13, float M21, float M22, float M23, float M31, float M32, float M33)
		{
			return M11 * (M22 * M33 - M23 * M32) - M12 * (M21 * M33 - M23 * M31) + M13 * (M21 * M32 - M22 * M31);
		}

		/// <summary>
		/// Inverts the matrix.
		/// </summary>
		public void Inverse()
		{
			float Det = Determinant();

			if (Det == 0.0f)
			{
				return;
			}

			float invDet = 1.0f / Det;
			float tM11 = Det3x3(M22, M23, M24, M32, M33, M34, M42, M43, M44) * invDet;
			float tM21 = -Det3x3(M21, M23, M24, M31, M33, M34, M41, M43, M44) * invDet;
			float tM31 = Det3x3(M21, M22, M24, M31, M32, M34, M41, M42, M44) * invDet;
			float tM41 = -Det3x3(M21, M22, M23, M31, M32, M33, M41, M42, M43) * invDet;

			float tM12 = -Det3x3(M12, M13, M14, M32, M33, M34, M42, M43, M44) * invDet;
			float tM22 = Det3x3(M11, M13, M14, M31, M33, M34, M41, M43, M44) * invDet;
			float tM32 = -Det3x3(M11, M12, M14, M31, M32, M34, M41, M42, M44) * invDet;
			float tM42 = Det3x3(M11, M12, M13, M31, M32, M33, M41, M42, M43) * invDet;

			float tM13 = Det3x3(M12, M13, M14, M22, M23, M24, M42, M43, M44) * invDet;
			float tM23 = -Det3x3(M11, M13, M14, M21, M23, M24, M41, M43, M44) * invDet;
			float tM33 = Det3x3(M11, M12, M14, M21, M22, M24, M41, M42, M44) * invDet;
			float tM43 = -Det3x3(M11, M12, M13, M21, M22, M23, M41, M42, M43) * invDet;

			float tM14 = -Det3x3(M12, M13, M14, M22, M23, M24, M32, M33, M34) * invDet;
			float tM24 = Det3x3(M11, M13, M14, M21, M23, M24, M31, M33, M34) * invDet;
			float tM34 = -Det3x3(M11, M12, M14, M21, M22, M24, M31, M32, M34) * invDet;
			float tM44 = Det3x3(M11, M12, M13, M21, M22, M23, M31, M32, M33) * invDet;

			M11 = tM11;
			M12 = tM12;
			M13 = tM13;
			M14 = tM14;

			M21 = tM21;
			M22 = tM22;
			M23 = tM23;
			M24 = tM24;

			M31 = tM31;
			M32 = tM32;
			M33 = tM33;
			M34 = tM34;

			M41 = tM41;
			M42 = tM42;
			M43 = tM43;
			M44 = tM44;
		}

		/// <summary>
		/// Determines the sum of two matrices.
		/// </summary>
		/// <param name="left">The first matrix to add.</param>
		/// <param name="right">The second matrix to add.</param>
		/// <returns>The sum of the two matrices.</returns>
		public static Matrix Add(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = left.M11 + right.M11;
			result.M12 = left.M12 + right.M12;
			result.M13 = left.M13 + right.M13;
			result.M14 = left.M14 + right.M14;
			result.M21 = left.M21 + right.M21;
			result.M22 = left.M22 + right.M22;
			result.M23 = left.M23 + right.M23;
			result.M24 = left.M24 + right.M24;
			result.M31 = left.M31 + right.M31;
			result.M32 = left.M32 + right.M32;
			result.M33 = left.M33 + right.M33;
			result.M34 = left.M34 + right.M34;
			result.M41 = left.M41 + right.M41;
			result.M42 = left.M42 + right.M42;
			result.M43 = left.M43 + right.M43;
			result.M44 = left.M44 + right.M44;
			return result;
		}

		/// <summary>
		/// Determines the difference between two matrices.
		/// </summary>
		/// <param name="left">The first matrix to subtract.</param>
		/// <param name="right">The second matrix to subtract.</param>
		/// <returns>The difference between the two matrices.</returns>
		public static Matrix Subtract(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = left.M11 - right.M11;
			result.M12 = left.M12 - right.M12;
			result.M13 = left.M13 - right.M13;
			result.M14 = left.M14 - right.M14;
			result.M21 = left.M21 - right.M21;
			result.M22 = left.M22 - right.M22;
			result.M23 = left.M23 - right.M23;
			result.M24 = left.M24 - right.M24;
			result.M31 = left.M31 - right.M31;
			result.M32 = left.M32 - right.M32;
			result.M33 = left.M33 - right.M33;
			result.M34 = left.M34 - right.M34;
			result.M41 = left.M41 - right.M41;
			result.M42 = left.M42 - right.M42;
			result.M43 = left.M43 - right.M43;
			result.M44 = left.M44 - right.M44;
			return result;
		}

		/// <summary>
		/// Determines the product of two matrices.
		/// </summary>
		/// <param name="left">The first matrix to multiply.</param>
		/// <param name="right">The second matrix to multiply.</param>
		/// <returns>The product of the two matrices.</returns>
		public static Matrix Multiply(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
			result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
			result.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
			result.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
			result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
			result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
			result.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
			result.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
			result.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
			result.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
			result.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
			result.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
			result.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
			result.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
			result.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
			result.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
			return result;
		}

		/// <summary>
		/// Scales a matrix by the given value.
		/// </summary>
		/// <param name="left">The matrix to scale.</param>
		/// <param name="right">The amount by which to scale.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix Multiply(Matrix left, float right)
		{
			Matrix result;
			result.M11 = left.M11 * right;
			result.M12 = left.M12 * right;
			result.M13 = left.M13 * right;
			result.M14 = left.M14 * right;
			result.M21 = left.M21 * right;
			result.M22 = left.M22 * right;
			result.M23 = left.M23 * right;
			result.M24 = left.M24 * right;
			result.M31 = left.M31 * right;
			result.M32 = left.M32 * right;
			result.M33 = left.M33 * right;
			result.M34 = left.M34 * right;
			result.M41 = left.M41 * right;
			result.M42 = left.M42 * right;
			result.M43 = left.M43 * right;
			result.M44 = left.M44 * right;
			return result;
		}

		/// <summary>
		/// Determines the quotient of two matrices.
		/// </summary>
		/// <param name="left">The first matrix to divide.</param>
		/// <param name="right">The second matrix to divide.</param>
		/// <returns>The quotient of the two matrices.</returns>
		public static Matrix Divide(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = left.M11 / right.M11;
			result.M12 = left.M12 / right.M12;
			result.M13 = left.M13 / right.M13;
			result.M14 = left.M14 / right.M14;
			result.M21 = left.M21 / right.M21;
			result.M22 = left.M22 / right.M22;
			result.M23 = left.M23 / right.M23;
			result.M24 = left.M24 / right.M24;
			result.M31 = left.M31 / right.M31;
			result.M32 = left.M32 / right.M32;
			result.M33 = left.M33 / right.M33;
			result.M34 = left.M34 / right.M34;
			result.M41 = left.M41 / right.M41;
			result.M42 = left.M42 / right.M42;
			result.M43 = left.M43 / right.M43;
			result.M44 = left.M44 / right.M44;
			return result;
		}

		/// <summary>
		/// Scales a matrix by the given value.
		/// </summary>
		/// <param name="left">The matrix to scale.</param>
		/// <param name="right">The amount by which to scale.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix Divide(Matrix left, float right)
		{
			Matrix result;
			float inv = 1.0f / right;

			result.M11 = left.M11 * inv;
			result.M12 = left.M12 * inv;
			result.M13 = left.M13 * inv;
			result.M14 = left.M14 * inv;
			result.M21 = left.M21 * inv;
			result.M22 = left.M22 * inv;
			result.M23 = left.M23 * inv;
			result.M24 = left.M24 * inv;
			result.M31 = left.M31 * inv;
			result.M32 = left.M32 * inv;
			result.M33 = left.M33 * inv;
			result.M34 = left.M34 * inv;
			result.M41 = left.M41 * inv;
			result.M42 = left.M42 * inv;
			result.M43 = left.M43 * inv;
			result.M44 = left.M44 * inv;
			return result;
		}

		/// <summary>
		/// Negates a matrix.
		/// </summary>
		/// <param name="matrix">The matrix to be negated.</param>
		/// <returns>The negated matrix.</returns>
		public static Matrix Negate(Matrix matrix)
		{
			Matrix result;
			result.M11 = -matrix.M11;
			result.M12 = -matrix.M12;
			result.M13 = -matrix.M13;
			result.M14 = -matrix.M14;
			result.M21 = -matrix.M21;
			result.M22 = -matrix.M22;
			result.M23 = -matrix.M23;
			result.M24 = -matrix.M24;
			result.M31 = -matrix.M31;
			result.M32 = -matrix.M32;
			result.M33 = -matrix.M33;
			result.M34 = -matrix.M34;
			result.M41 = -matrix.M41;
			result.M42 = -matrix.M42;
			result.M43 = -matrix.M43;
			result.M44 = -matrix.M44;
			return result;
		}

		/// <summary>
		/// Calculates the inverse of a matrix if it exists.
		/// </summary>
		/// <returns>The inverse of the matrix.</returns>
		public static Matrix Inverse(Matrix matrix)
		{
			matrix.Inverse();
			return matrix;
		}

		/// <summary>
		/// Performs a linear interpolation between two matrices.
		/// </summary>
		/// <param name="start">Start matrix.</param>
		/// <param name="end">End matrix.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>The linear interpolation of the two matrices.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned.
		/// </remarks>
		public static Matrix Lerp(Matrix start, Matrix end, float amount)
		{
			Matrix result;
			result.M11 = start.M11 + ((end.M11 - start.M11) * amount);
			result.M12 = start.M12 + ((end.M12 - start.M12) * amount);
			result.M13 = start.M13 + ((end.M13 - start.M13) * amount);
			result.M14 = start.M14 + ((end.M14 - start.M14) * amount);
			result.M21 = start.M21 + ((end.M21 - start.M21) * amount);
			result.M22 = start.M22 + ((end.M22 - start.M22) * amount);
			result.M23 = start.M23 + ((end.M23 - start.M23) * amount);
			result.M24 = start.M24 + ((end.M24 - start.M24) * amount);
			result.M31 = start.M31 + ((end.M31 - start.M31) * amount);
			result.M32 = start.M32 + ((end.M32 - start.M32) * amount);
			result.M33 = start.M33 + ((end.M33 - start.M33) * amount);
			result.M34 = start.M34 + ((end.M34 - start.M34) * amount);
			result.M41 = start.M41 + ((end.M41 - start.M41) * amount);
			result.M42 = start.M42 + ((end.M42 - start.M42) * amount);
			result.M43 = start.M43 + ((end.M43 - start.M43) * amount);
			result.M44 = start.M44 + ((end.M44 - start.M44) * amount);
			return result;
		}

		/// <summary>
		/// Creates a matrix that rotates around the x-axis.
		/// </summary>
		/// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
		/// <returns>The created rotation matrix.</returns>
		public static Matrix RotationX(float angle)
		{
			Matrix result;
			float cos = (float)System.Math.Cos(angle);
			float sin = (float)(System.Math.Sin(angle));

			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = cos;
			result.M23 = sin;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = -sin;
			result.M33 = cos;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix that rotates around the y-axis.
		/// </summary>
		/// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
		/// <returns>The created rotation matrix.</returns>
		public static Matrix RotationY(float angle)
		{
			Matrix result;
			float cos = (float)(System.Math.Cos(angle));
			float sin = (float)(System.Math.Sin(angle));

			result.M11 = cos;
			result.M12 = 0.0f;
			result.M13 = -sin;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = sin;
			result.M32 = 0.0f;
			result.M33 = cos;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix that rotates around the z-axis.
		/// </summary>
		/// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
		/// <returns>The created rotation matrix.</returns>
		public static Matrix RotationZ(float angle)
		{
			Matrix result;
			float cos = (float)(System.Math.Cos(angle));
			float sin = (float)(System.Math.Sin(angle));

			result.M11 = cos;
			result.M12 = sin;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = -sin;
			result.M22 = cos;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix that rotates around an arbitrary axis.
		/// </summary>
		/// <param name="axis">The axis around which to rotate.</param>
		/// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
		/// <returns>The created rotation matrix.</returns>
		public static Matrix RotationAxis(Vector3 axis, float angle)
		{
			if (axis.LengthSquared() != 1.0f)
			{
				axis.Normalize();
			}

			Matrix result;
			float x = axis.X;
			float y = axis.Y;
			float z = axis.Z;
			float cos = (float)(System.Math.Cos(angle));
			float sin = (float)(System.Math.Sin(angle));
			float xx = x * x;
			float yy = y * y;
			float zz = z * z;
			float xy = x * y;
			float xz = x * z;
			float yz = y * z;

			result.M11 = xx + (cos * (1.0f - xx));
			result.M12 = (xy - (cos * xy)) + (sin * z);
			result.M13 = (xz - (cos * xz)) - (sin * y);
			result.M14 = 0.0f;
			result.M21 = (xy - (cos * xy)) - (sin * z);
			result.M22 = yy + (cos * (1.0f - yy));
			result.M23 = (yz - (cos * yz)) + (sin * x);
			result.M24 = 0.0f;
			result.M31 = (xz - (cos * xz)) + (sin * y);
			result.M32 = (yz - (cos * yz)) - (sin * x);
			result.M33 = zz + (cos * (1.0f - zz));
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a rotation matrix from a rotation.
		/// </summary>
		/// <param name="rotation">The quaternion to use to build the matrix.</param>
		/// <returns>The created rotation matrix.</returns>
		public static Matrix RotationQuaternion(Quaternion rotation)
		{
			Matrix result;

			float xx = rotation.X * rotation.X;
			float yy = rotation.Y * rotation.Y;
			float zz = rotation.Z * rotation.Z;
			float xy = rotation.X * rotation.Y;
			float zw = rotation.Z * rotation.W;
			float zx = rotation.Z * rotation.X;
			float yw = rotation.Y * rotation.W;
			float yz = rotation.Y * rotation.Z;
			float xw = rotation.X * rotation.W;
			result.M11 = 1.0f - (2.0f * (yy + zz));
			result.M12 = 2.0f * (xy + zw);
			result.M13 = 2.0f * (zx - yw);
			result.M14 = 0.0f;
			result.M21 = 2.0f * (xy - zw);
			result.M22 = 1.0f - (2.0f * (zz + xx));
			result.M23 = 2.0f * (yz + xw);
			result.M24 = 0.0f;
			result.M31 = 2.0f * (zx + yw);
			result.M32 = 2.0f * (yz - xw);
			result.M33 = 1.0f - (2.0f * (yy + xx));
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a rotation matrix with a specified yaw, pitch, and roll.
		/// </summary>
		/// <param name="yaw">Yaw around the y-axis, in radians.</param>
		/// <param name="pitch">Pitch around the x-axis, in radians.</param>
		/// <param name="roll">Roll around the z-axis, in radians.</param>
		/// <returns>The created rotation matrix.</returns>
		public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
		{
			var quaternion = Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
			return RotationQuaternion(quaternion);
		}

		/// <summary>
		/// Creates a matrix that scales along the x-axis, y-axis, and y-axis.
		/// </summary>
		/// <param name="x">Scaling factor that is applied along the x-axis.</param>
		/// <param name="y">Scaling factor that is applied along the y-axis.</param>
		/// <param name="z">Scaling factor that is applied along the z-axis.</param>
		/// <returns>The created scaling matrix.</returns>
		public static Matrix Scaling(float x, float y, float z)
		{
			Matrix result;
			result.M11 = x;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = y;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = z;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;
			return result;
		}

		/// <summary>
		/// Creates a matrix that scales along the x-axis, y-axis, and y-axis.
		/// </summary>
		/// <param name="scale">Scaling factor for all three axes.</param>
		/// <returns>The created scaling matrix.</returns>
		public static Matrix Scaling(Vector3 scale)
		{
			Matrix result;
			result.M11 = scale.X;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = scale.Y;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = scale.Z;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;
			return result;
		}

		/// <summary>
		/// Creates a translation matrix using the specified offsets.
		/// </summary>
		/// <param name="x">X-coordinate offset.</param>
		/// <param name="y">Y-coordinate offset.</param>
		/// <param name="z">Z-coordinate offset.</param>
		/// <returns>The created translation matrix.</returns>
		public static Matrix Translation(float x, float y, float z)
		{
			Matrix result;
			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;
			result.M41 = x;
			result.M42 = y;
			result.M43 = z;
			result.M44 = 1.0f;
			return result;
		}

		/// <summary>
		/// Creates a translation matrix using the specified offsets.
		/// </summary>
		/// <param name="amount">The offset for all three coordinate planes.</param>
		/// <returns>The created translation matrix.</returns>
		public static Matrix Translation(Vector3 amount)
		{
			Matrix result = Identity;
			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;
			result.M41 = amount.X;
			result.M42 = amount.Y;
			result.M43 = amount.Z;
			result.M44 = 1.0f;
			return result;
		}

		/// <summary>
		/// Calculates the transpose of the specified matrix.
		/// </summary>
		/// <param name="matrix">The matrix whose transpose is to be calculated.</param>
		/// <returns>The transpose of the specified matrix.</returns>
		public static Matrix Transpose(Matrix matrix)
		{
			Matrix result;
			result.M11 = matrix.M11;
			result.M12 = matrix.M21;
			result.M13 = matrix.M31;
			result.M14 = matrix.M41;
			result.M21 = matrix.M12;
			result.M22 = matrix.M22;
			result.M23 = matrix.M32;
			result.M24 = matrix.M42;
			result.M31 = matrix.M13;
			result.M32 = matrix.M23;
			result.M33 = matrix.M33;
			result.M34 = matrix.M43;
			result.M41 = matrix.M14;
			result.M42 = matrix.M24;
			result.M43 = matrix.M34;
			result.M44 = matrix.M44;
			return result;
		}

		/// <summary>
		/// Negates a matrix.
		/// </summary>
		/// <param name="matrix">The matrix to negate.</param>
		/// <returns>The negated matrix.</returns>
		public static Matrix operator -(Matrix matrix)
		{
			Matrix result;
			result.M11 = -matrix.M11;
			result.M12 = -matrix.M12;
			result.M13 = -matrix.M13;
			result.M14 = -matrix.M14;
			result.M21 = -matrix.M21;
			result.M22 = -matrix.M22;
			result.M23 = -matrix.M23;
			result.M24 = -matrix.M24;
			result.M31 = -matrix.M31;
			result.M32 = -matrix.M32;
			result.M33 = -matrix.M33;
			result.M34 = -matrix.M34;
			result.M41 = -matrix.M41;
			result.M42 = -matrix.M42;
			result.M43 = -matrix.M43;
			result.M44 = -matrix.M44;
			return result;
		}

		/// <summary>
		/// Adds two matrices.
		/// </summary>
		/// <param name="left">The first matrix to add.</param>
		/// <param name="right">The second matrix to add.</param>
		/// <returns>The sum of the two matrices.</returns>
		public static Matrix operator +(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = left.M11 + right.M11;
			result.M12 = left.M12 + right.M12;
			result.M13 = left.M13 + right.M13;
			result.M14 = left.M14 + right.M14;
			result.M21 = left.M21 + right.M21;
			result.M22 = left.M22 + right.M22;
			result.M23 = left.M23 + right.M23;
			result.M24 = left.M24 + right.M24;
			result.M31 = left.M31 + right.M31;
			result.M32 = left.M32 + right.M32;
			result.M33 = left.M33 + right.M33;
			result.M34 = left.M34 + right.M34;
			result.M41 = left.M41 + right.M41;
			result.M42 = left.M42 + right.M42;
			result.M43 = left.M43 + right.M43;
			result.M44 = left.M44 + right.M44;
			return result;
		}

		/// <summary>
		/// Subtracts two matrices.
		/// </summary>
		/// <param name="left">The first matrix to subtract.</param>
		/// <param name="right">The second matrix to subtract.</param>
		/// <returns>The difference between the two matrices.</returns>
		public static Matrix operator -(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = left.M11 - right.M11;
			result.M12 = left.M12 - right.M12;
			result.M13 = left.M13 - right.M13;
			result.M14 = left.M14 - right.M14;
			result.M21 = left.M21 - right.M21;
			result.M22 = left.M22 - right.M22;
			result.M23 = left.M23 - right.M23;
			result.M24 = left.M24 - right.M24;
			result.M31 = left.M31 - right.M31;
			result.M32 = left.M32 - right.M32;
			result.M33 = left.M33 - right.M33;
			result.M34 = left.M34 - right.M34;
			result.M41 = left.M41 - right.M41;
			result.M42 = left.M42 - right.M42;
			result.M43 = left.M43 - right.M43;
			result.M44 = left.M44 - right.M44;
			return result;
		}

		/// <summary>
		/// Divides two matrices.
		/// </summary>
		/// <param name="left">The first matrix to divide.</param>
		/// <param name="right">The second matrix to divide.</param>
		/// <returns>The quotient of the two matrices.</returns>
		public static Matrix operator /(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = left.M11 / right.M11;
			result.M12 = left.M12 / right.M12;
			result.M13 = left.M13 / right.M13;
			result.M14 = left.M14 / right.M14;
			result.M21 = left.M21 / right.M21;
			result.M22 = left.M22 / right.M22;
			result.M23 = left.M23 / right.M23;
			result.M24 = left.M24 / right.M24;
			result.M31 = left.M31 / right.M31;
			result.M32 = left.M32 / right.M32;
			result.M33 = left.M33 / right.M33;
			result.M34 = left.M34 / right.M34;
			result.M41 = left.M41 / right.M41;
			result.M42 = left.M42 / right.M42;
			result.M43 = left.M43 / right.M43;
			result.M44 = left.M44 / right.M44;
			return result;
		}

		/// <summary>
		/// Scales a matrix by a given value.
		/// </summary>
		/// <param name="left">The matrix to scale.</param>
		/// <param name="right">The amount by which to scale.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix operator /(Matrix left, float right)
		{
			Matrix result;
			float invRight = 1.0f / right;
			result.M11 = left.M11 * invRight;
			result.M12 = left.M12 * invRight;
			result.M13 = left.M13 * invRight;
			result.M14 = left.M14 * invRight;
			result.M21 = left.M21 * invRight;
			result.M22 = left.M22 * invRight;
			result.M23 = left.M23 * invRight;
			result.M24 = left.M24 * invRight;
			result.M31 = left.M31 * invRight;
			result.M32 = left.M32 * invRight;
			result.M33 = left.M33 * invRight;
			result.M34 = left.M34 * invRight;
			result.M41 = left.M41 * invRight;
			result.M42 = left.M42 * invRight;
			result.M43 = left.M43 * invRight;
			result.M44 = left.M44 * invRight;
			return result;
		}

		/// <summary>
		/// Multiplies two matrices.
		/// </summary>
		/// <param name="left">The first matrix to multiply.</param>
		/// <param name="right">The second matrix to multiply.</param>
		/// <returns>The product of the two matrices.</returns>
		public static Matrix operator *(Matrix left, Matrix right)
		{
			Matrix result;
			result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
			result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
			result.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
			result.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
			result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
			result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
			result.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
			result.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
			result.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
			result.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
			result.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
			result.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
			result.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
			result.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
			result.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
			result.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
			return result;
		}

		/// <summary>
		/// Scales a matrix by a given value.
		/// </summary>
		/// <param name="left">The matrix to scale.</param>
		/// <param name="right">The amount by which to scale.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix operator *(Matrix left, float right)
		{
			Matrix result;
			result.M11 = left.M11 * right;
			result.M12 = left.M12 * right;
			result.M13 = left.M13 * right;
			result.M14 = left.M14 * right;
			result.M21 = left.M21 * right;
			result.M22 = left.M22 * right;
			result.M23 = left.M23 * right;
			result.M24 = left.M24 * right;
			result.M31 = left.M31 * right;
			result.M32 = left.M32 * right;
			result.M33 = left.M33 * right;
			result.M34 = left.M34 * right;
			result.M41 = left.M41 * right;
			result.M42 = left.M42 * right;
			result.M43 = left.M43 * right;
			result.M44 = left.M44 * right;
			return result;
		}

		/// <summary>
		/// Scales a matrix by a given value.
		/// </summary>
		/// <param name="right">The matrix to scale.</param>
		/// <param name="left">The amount by which to scale.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix operator *(float left, Matrix right) => right * left;

		/// <summary>
		/// Tests for equality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <see langword="false" />.</returns>
		public static bool operator ==(Matrix left, Matrix right) => left.Equals(right);

		/// <summary>
		/// Tests for inequality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <see langword="false" />.</returns>
		public static bool operator !=(Matrix left, Matrix right) => !left.Equals(right);

		/// <summary>
		/// Converts the matrix to an array of floats.
		/// </summary>
		public float[] ToArray() => new[] { M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 };

		/// <summary>
		/// Converts the value of the object to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of the value of this instance.</returns>
		public override string ToString()
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string m11Str = M11.ToString(currentCulture);
			string m12Str = M12.ToString(currentCulture);
			string m13Str = M13.ToString(currentCulture);
			string m14Str = M14.ToString(currentCulture);

			string m21Str = M21.ToString(currentCulture);
			string m22Str = M22.ToString(currentCulture);
			string m23Str = M23.ToString(currentCulture);
			string m24Str = M24.ToString(currentCulture);

			string m31Str = M31.ToString(currentCulture);
			string m32Str = M32.ToString(currentCulture);
			string m33Str = M33.ToString(currentCulture);
			string m34Str = M34.ToString(currentCulture);

			string m41Str = M41.ToString(currentCulture);
			string m42Str = M42.ToString(currentCulture);
			string m43Str = M43.ToString(currentCulture);
			string m44Str = M44.ToString(currentCulture);

			return $"[M11:{m11Str} M12:{m12Str} M13:{m13Str} M14:{m14Str}] [M21:{m21Str} M22:{m22Str} M23:{m23Str} M24:{m24Str}] [M31:{m31Str} M32:{m32Str} M33:{m33Str} M34:{m34Str}] [M41:{m41Str} M42:{m42Str} M43:{m43Str} M44:{m44Str}]";
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = M11.GetHashCode();
				hashCode = (hashCode * 397) ^ M12.GetHashCode();
				hashCode = (hashCode * 397) ^ M13.GetHashCode();
				hashCode = (hashCode * 397) ^ M14.GetHashCode();
				hashCode = (hashCode * 397) ^ M21.GetHashCode();
				hashCode = (hashCode * 397) ^ M22.GetHashCode();
				hashCode = (hashCode * 397) ^ M23.GetHashCode();
				hashCode = (hashCode * 397) ^ M24.GetHashCode();
				hashCode = (hashCode * 397) ^ M31.GetHashCode();
				hashCode = (hashCode * 397) ^ M32.GetHashCode();
				hashCode = (hashCode * 397) ^ M33.GetHashCode();
				hashCode = (hashCode * 397) ^ M34.GetHashCode();
				hashCode = (hashCode * 397) ^ M41.GetHashCode();
				hashCode = (hashCode * 397) ^ M42.GetHashCode();
				hashCode = (hashCode * 397) ^ M43.GetHashCode();
				hashCode = (hashCode * 397) ^ M44.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">Object to make the comparison with.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified object; <see langword="false" /> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((Matrix)obj);
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to the specified object.
		/// </summary>
		/// <param name="other">Object to make the comparison with.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified object; <see langword="false" /> otherwise.</returns>
		public bool Equals(Matrix other)
		{
			return (M11 == other.M11 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
				M21 == other.M21 && M22 == other.M22 && M23 == other.M23 && M24 == other.M24 &&
				M31 == other.M31 && M32 == other.M32 && M33 == other.M33 && M34 == other.M34 &&
				M41 == other.M41 && M42 == other.M42 && M43 == other.M43 && M44 == other.M44);
		}

		/// <summary>
		/// Determines whether the specified object instances are considered equal.
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns><see langword="true" /> if <paramref name="value1"/> is the same instance as <paramref name="value2"/> or
		/// if both are <see langword="null" /> references or if <c>value1.Equals(value2)</c> returns <see langword="true" />; otherwise, <see langword="false" />.</returns>
		public static bool Equals(ref Matrix value1, ref Matrix value2) => value1.Equals(value2);
	}
}
