using System.Diagnostics.CodeAnalysis;

public struct Rect2I
{
    private Vec2I _position;
    private Vec2I _size;

    /// <summary>
    /// Beginning corner. Typically has values lower than <see cref="P:Godot.Rect2I.End" />.
    /// </summary>
    /// <value>Directly uses a private field.</value>
    public Vec2I Position
    {
      readonly get => this._position;
      set => this._position = value;
    }

    /// <summary>
    /// Size from <see cref="P:Godot.Rect2I.Position" /> to <see cref="P:Godot.Rect2I.End" />. Typically all components are positive.
    /// If the size is negative, you can use <see cref="M:Godot.Rect2I.Abs" /> to fix it.
    /// </summary>
    /// <value>Directly uses a private field.</value>
    public Vec2I Size
    {
      readonly get => this._size;
      set => this._size = value;
    }

    /// <summary>
    /// Ending corner. This is calculated as <see cref="P:Godot.Rect2I.Position" /> plus <see cref="P:Godot.Rect2I.Size" />.
    /// Setting this value will change the size.
    /// </summary>
    /// <value>
    /// Getting is equivalent to <paramref name="value" /> = <see cref="P:Godot.Rect2I.Position" /> + <see cref="P:Godot.Rect2I.Size" />,
    /// setting is equivalent to <see cref="P:Godot.Rect2I.Size" /> = <paramref name="value" /> - <see cref="P:Godot.Rect2I.Position" />
    /// </value>
    public Vec2I End
    {
      readonly get => this._position + this._size;
      set => this._size = value - this._position;
    }

    /// <summary>
    /// The area of this <see cref="T:Godot.Rect2I" />.
    /// See also <see cref="M:Godot.Rect2I.HasArea" />.
    /// </summary>
    public readonly int Area => this._size.X * this._size.Y;

    /// <summary>
    /// Returns a <see cref="T:Godot.Rect2I" /> with equivalent position and size, modified so that
    /// the top-left corner is the origin and width and height are positive.
    /// </summary>
    /// <returns>The modified <see cref="T:Godot.Rect2I" />.</returns>
    public readonly Rect2I Abs() => new Rect2I(this.End.Min(this._position), this._size.Abs());

    /// <summary>
    /// Returns the intersection of this <see cref="T:Godot.Rect2I" /> and <paramref name="b" />.
    /// If the rectangles do not intersect, an empty <see cref="T:Godot.Rect2I" /> is returned.
    /// </summary>
    /// <param name="b">The other <see cref="T:Godot.Rect2I" />.</param>
    /// <returns>
    /// The intersection of this <see cref="T:Godot.Rect2I" /> and <paramref name="b" />,
    /// or an empty <see cref="T:Godot.Rect2I" /> if they do not intersect.
    /// </returns>
    public readonly Rect2I Intersection(Rect2I b)
    {
      Rect2I b1 = b;
      if (!this.Intersects(b1))
        return new Rect2I();
      b1._position = b._position.Max(this._position);
      Vec2I vector2I = b._position + b._size;
      Vec2I with = this._position + this._size;
      b1._size = vector2I.Min(with) - b1._position;
      return b1;
    }

    /// <summary>
    /// Returns <see langword="true" /> if this <see cref="T:Godot.Rect2I" /> completely encloses another one.
    /// </summary>
    /// <param name="b">The other <see cref="T:Godot.Rect2I" /> that may be enclosed.</param>
    /// <returns>
    /// A <see langword="bool" /> for whether or not this <see cref="T:Godot.Rect2I" /> encloses <paramref name="b" />.
    /// </returns>
    public readonly bool Encloses(Rect2I b)
    {
      return b._position.X >= this._position.X && b._position.Y >= this._position.Y && b._position.X + b._size.X <= this._position.X + this._size.X && b._position.Y + b._size.Y <= this._position.Y + this._size.Y;
    }

    /// <summary>
    /// Returns this <see cref="T:Godot.Rect2I" /> expanded to include a given point.
    /// </summary>
    /// <param name="to">The point to include.</param>
    /// <returns>The expanded <see cref="T:Godot.Rect2I" />.</returns>
    public readonly Rect2I Expand(Vec2I to)
    {
      Rect2I rect2I = this;
      Vec2I position = rect2I._position;
      Vec2I vector2I = rect2I._position + rect2I._size;
      if (to.X < position.X)
        position.X = to.X;
      if (to.Y < position.Y)
        position.Y = to.Y;
      if (to.X > vector2I.X)
        vector2I.X = to.X;
      if (to.Y > vector2I.Y)
        vector2I.Y = to.Y;
      rect2I._position = position;
      rect2I._size = vector2I - position;
      return rect2I;
    }

    /// <summary>
    /// Returns the center of the <see cref="T:Godot.Rect2I" />, which is equal
    /// to <see cref="P:Godot.Rect2I.Position" /> + (<see cref="P:Godot.Rect2I.Size" /> / 2).
    /// If <see cref="P:Godot.Rect2I.Size" /> is an odd number, the returned center
    /// value will be rounded towards <see cref="P:Godot.Rect2I.Position" />.
    /// </summary>
    /// <returns>The center.</returns>
    public readonly Vec2I GetCenter() => this._position + this._size / 2;

    /// <summary>
    /// Returns a copy of the <see cref="T:Godot.Rect2I" /> grown by the specified amount
    /// on all sides.
    /// </summary>
    /// <seealso cref="M:Godot.Rect2I.GrowIndividual(System.Int32,System.Int32,System.Int32,System.Int32)" />
    /// <seealso cref="M:Godot.Rect2I.GrowSide(Godot.Side,System.Int32)" />
    /// <param name="by">The amount to grow by.</param>
    /// <returns>The grown <see cref="T:Godot.Rect2I" />.</returns>
    public readonly Rect2I Grow(int by)
    {
      Rect2I rect2I = this;
      rect2I._position.X -= by;
      rect2I._position.Y -= by;
      rect2I._size.X += by * 2;
      rect2I._size.Y += by * 2;
      return rect2I;
    }

    /// <summary>
    /// Returns a copy of the <see cref="T:Godot.Rect2I" /> grown by the specified amount
    /// on each side individually.
    /// </summary>
    /// <seealso cref="M:Godot.Rect2I.Grow(System.Int32)" />
    /// <seealso cref="M:Godot.Rect2I.GrowSide(Godot.Side,System.Int32)" />
    /// <param name="left">The amount to grow by on the left side.</param>
    /// <param name="top">The amount to grow by on the top side.</param>
    /// <param name="right">The amount to grow by on the right side.</param>
    /// <param name="bottom">The amount to grow by on the bottom side.</param>
    /// <returns>The grown <see cref="T:Godot.Rect2I" />.</returns>
    public readonly Rect2I GrowIndividual(int left, int top, int right, int bottom)
    {
      Rect2I rect2I = this;
      rect2I._position.X -= left;
      rect2I._position.Y -= top;
      rect2I._size.X += left + right;
      rect2I._size.Y += top + bottom;
      return rect2I;
    }

    /// <summary>
    /// Returns <see langword="true" /> if the <see cref="T:Godot.Rect2I" /> has
    /// area, and <see langword="false" /> if the <see cref="T:Godot.Rect2I" />
    /// is linear, empty, or has a negative <see cref="P:Godot.Rect2I.Size" />.
    /// See also <see cref="P:Godot.Rect2I.Area" />.
    /// </summary>
    /// <returns>
    /// A <see langword="bool" /> for whether or not the <see cref="T:Godot.Rect2I" /> has area.
    /// </returns>
    public readonly bool HasArea() => this._size.X > 0 && this._size.Y > 0;

    /// <summary>
    /// Returns <see langword="true" /> if the <see cref="T:Godot.Rect2I" /> contains a point,
    /// or <see langword="false" /> otherwise.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>
    /// A <see langword="bool" /> for whether or not the <see cref="T:Godot.Rect2I" /> contains <paramref name="point" />.
    /// </returns>
    public readonly bool HasPoint(Vec2I point)
    {
      return point.X >= this._position.X && point.Y >= this._position.Y && point.X < this._position.X + this._size.X && point.Y < this._position.Y + this._size.Y;
    }

    /// <summary>
    /// Returns <see langword="true" /> if the <see cref="T:Godot.Rect2I" /> overlaps with <paramref name="b" />
    /// (i.e. they have at least one point in common).
    /// </summary>
    /// <param name="b">The other <see cref="T:Godot.Rect2I" /> to check for intersections with.</param>
    /// <returns>A <see langword="bool" /> for whether or not they are intersecting.</returns>
    public readonly bool Intersects(Rect2I b)
    {
      return this._position.X < b._position.X + b._size.X && this._position.X + this._size.X > b._position.X && this._position.Y < b._position.Y + b._size.Y && this._position.Y + this._size.Y > b._position.Y;
    }

    /// <summary>
    /// Returns a larger <see cref="T:Godot.Rect2I" /> that contains this <see cref="T:Godot.Rect2I" /> and <paramref name="b" />.
    /// </summary>
    /// <param name="b">The other <see cref="T:Godot.Rect2I" />.</param>
    /// <returns>The merged <see cref="T:Godot.Rect2I" />.</returns>
    public readonly Rect2I Merge(Rect2I b)
    {
      Rect2I rect2I;
      rect2I._position = b._position.Min(this._position);
      rect2I._size = (b._position + b._size).Max(this._position + this._size);
      rect2I._size -= rect2I._position;
      return rect2I;
    }

    /// <summary>
    /// Constructs a <see cref="T:Godot.Rect2I" /> from a position and size.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="size">The size.</param>
    public Rect2I(Vec2I position, Vec2I size)
    {
      this._position = position;
      this._size = size;
    }

    /// <summary>
    /// Constructs a <see cref="T:Godot.Rect2I" /> from a position, width, and height.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rect2I(Vec2I position, int width, int height)
    {
      this._position = position;
      this._size = new Vec2I(width, height);
    }

    /// <summary>
    /// Constructs a <see cref="T:Godot.Rect2I" /> from x, y, and size.
    /// </summary>
    /// <param name="x">The position's X coordinate.</param>
    /// <param name="y">The position's Y coordinate.</param>
    /// <param name="size">The size.</param>
    public Rect2I(int x, int y, Vec2I size)
    {
      this._position = new Vec2I(x, y);
      this._size = size;
    }

    /// <summary>
    /// Constructs a <see cref="T:Godot.Rect2I" /> from x, y, width, and height.
    /// </summary>
    /// <param name="x">The position's X coordinate.</param>
    /// <param name="y">The position's Y coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rect2I(int x, int y, int width, int height)
    {
      this._position = new Vec2I(x, y);
      this._size = new Vec2I(width, height);
    }

    /// <summary>
    /// Returns <see langword="true" /> if the
    /// <see cref="T:Godot.Rect2I" />s are exactly equal.
    /// </summary>
    /// <param name="left">The left rect.</param>
    /// <param name="right">The right rect.</param>
    /// <returns>Whether or not the rects are equal.</returns>
    public static bool operator ==(Rect2I left, Rect2I right) => left.Equals(right);

    /// <summary>
    /// Returns <see langword="true" /> if the
    /// <see cref="T:Godot.Rect2I" />s are not equal.
    /// </summary>
    /// <param name="left">The left rect.</param>
    /// <param name="right">The right rect.</param>
    /// <returns>Whether or not the rects are not equal.</returns>
    public static bool operator !=(Rect2I left, Rect2I right) => !left.Equals(right);
    
    /// <summary>
    /// Returns <see langword="true" /> if this rect and <paramref name="obj" /> are equal.
    /// </summary>
    /// <param name="obj">The other object to compare.</param>
    /// <returns>Whether or not the rect and the other object are equal.</returns>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is Rect2I other && this.Equals(other);
    }

    /// <summary>
    /// Returns <see langword="true" /> if this rect and <paramref name="other" /> are equal.
    /// </summary>
    /// <param name="other">The other rect to compare.</param>
    /// <returns>Whether or not the rects are equal.</returns>
    public readonly bool Equals(Rect2I other)
    {
      return this._position.Equals(other._position) && this._size.Equals(other._size);
    }

    /// <summary>
    /// Serves as the hash function for <see cref="T:Godot.Rect2I" />.
    /// </summary>
    /// <returns>A hash code for this rect.</returns>
    public override int GetHashCode()
    {
      return HashCode.Combine<Vec2I, Vec2I>(this._position, this._size);
    }

    /// <summary>
    /// Converts this <see cref="T:Godot.Rect2I" /> to a string.
    /// </summary>
    /// <returns>A string representation of this rect.</returns>
    public override readonly string ToString() => $"({_position}) ({_size})";
}