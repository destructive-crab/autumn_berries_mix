 using System.Diagnostics.CodeAnalysis;
 using System.Numerics;

 public struct Vec2I : IEquatable<Vec2I>
  {
    public int X;
    public int Y;
    
    private static readonly Vec2I _minValue = new Vec2I(int.MinValue, int.MinValue);
    private static readonly Vec2I _maxValue = new Vec2I(int.MaxValue, int.MaxValue);
    private static readonly Vec2I _zero = new Vec2I(0, 0);
    private static readonly Vec2I _one = new Vec2I(1, 1);
    private static readonly Vec2I _up = new Vec2I(0, -1);
    private static readonly Vec2I _down = new Vec2I(0, 1);
    private static readonly Vec2I _right = new Vec2I(1, 0);
    private static readonly Vec2I _left = new Vec2I(-1, 0);

    public int this[int index]
    {
      readonly get
      {
        if (index == 0)
          return this.X;
        if (index == 1)
          return this.Y;
        throw new ArgumentOutOfRangeException(nameof (index));
      }
      set
      {
        if (index != 0)
        {
          if (index != 1)
            throw new ArgumentOutOfRangeException(nameof (index));
          this.Y = value;
        }
        else
          this.X = value;
      }
    }

    public readonly Vec2I Abs() => new Vec2I(Math.Abs(this.X), Math.Abs(this.Y));

    public readonly float Aspect() => (float) this.X / (float) this.Y;

    public readonly Vec2I Clamp(Vec2I min, Vec2I max)
    {
      return new Vec2I(Math.Clamp(this.X, min.X, max.X), Math.Clamp(this.Y, min.Y, max.Y));
    }

    public readonly Vec2I Clamp(int min, int max)
    {
      return new Vec2I(Math.Clamp(this.X, min, max), Math.Clamp(this.Y, min, max));
    }

    public readonly int DistanceSquaredTo(Vec2I to) => (to - this).LengthSquared();

    public readonly float DistanceTo(Vec2I to) => (to - this).Length();

    public readonly float Length() => (float)Math.Sqrt((float) (this.X * this.X + this.Y * this.Y));

    public readonly int LengthSquared() => this.X * this.X + this.Y * this.Y;

    public readonly Vec2I Max(Vec2I with)
    {
      return new Vec2I(Math.Max(this.X, with.X), Math.Max(this.Y, with.Y));
    }

    public readonly Vec2I Max(int with)
    {
      return new Vec2I(Math.Max(this.X, with), Math.Max(this.Y, with));
    }

    public readonly Vec2I Min(Vec2I with)
    {
      return new Vec2I(Math.Min(this.X, with.X), Math.Min(this.Y, with.Y));
    }

    public readonly Vec2I Min(int with)
    {
      return new Vec2I(Math.Min(this.X, with), Math.Min(this.Y, with));
    }

    public readonly Vec2I.Axis MaxAxisIndex()
    {
      return this.X >= this.Y ? Vec2I.Axis.X : Vec2I.Axis.Y;
    }

    public readonly Vec2I.Axis MinAxisIndex()
    {
      return this.X >= this.Y ? Vec2I.Axis.Y : Vec2I.Axis.X;
    }

    public static Vec2I MinValue => Vec2I._minValue;

    public static Vec2I MaxValue => Vec2I._maxValue;

    public static Vec2I Zero => Vec2I._zero;

    public static Vec2I One => Vec2I._one;

    public static Vec2I Up => Vec2I._up;

    public static Vec2I Down => Vec2I._down;

    public static Vec2I Right => Vec2I._right;
    
    public static Vec2I Left => Vec2I._left;

    public Vec2I(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    public static Vec2I operator +(Vec2I left, Vec2I right)
    {
      left.X += right.X;
      left.Y += right.Y;
      return left;
    }

    public static Vec2I operator -(Vec2I left, Vec2I right)
    {
      left.X -= right.X;
      left.Y -= right.Y;
      return left;
    }

    public static Vec2I operator -(Vec2I vec)
    {
      vec.X = -vec.X;
      vec.Y = -vec.Y;
      return vec;
    }

    public static Vec2I operator *(Vec2I vec, int scale)
    {
      vec.X *= scale;
      vec.Y *= scale;
      return vec;
    }

    public static Vec2I operator *(int scale, Vec2I vec)
    {
      vec.X *= scale;
      vec.Y *= scale;
      return vec;
    }

    public static Vec2I operator *(Vec2I left, Vec2I right)
    {
      left.X *= right.X;
      left.Y *= right.Y;
      return left;
    }

    public static Vec2I operator /(Vec2I vec, int divisor)
    {
      vec.X /= divisor;
      vec.Y /= divisor;
      return vec;
    }

    public static Vec2I operator /(Vec2I vec, Vec2I divisorv)
    {
      vec.X /= divisorv.X;
      vec.Y /= divisorv.Y;
      return vec;
    }

    public static Vec2I operator %(Vec2I vec, int divisor)
    {
      vec.X %= divisor;
      vec.Y %= divisor;
      return vec;
    }

    public static Vec2I operator %(Vec2I vec, Vec2I divisorv)
    {
      vec.X %= divisorv.X;
      vec.Y %= divisorv.Y;
      return vec;
    }

    public static bool operator ==(Vec2I left, Vec2I right) => left.Equals(right);

    public static bool operator !=(Vec2I left, Vec2I right) => !left.Equals(right);

    public static bool operator <(Vec2I left, Vec2I right)
    {
      return left.X == right.X ? left.Y < right.Y : left.X < right.X;
    }

    public static bool operator >(Vec2I left, Vec2I right)
    {
      return left.X == right.X ? left.Y > right.Y : left.X > right.X;
    }

    public static bool operator <=(Vec2I left, Vec2I right)
    {
      return left.X == right.X ? left.Y <= right.Y : left.X < right.X;
    }
    
    public static bool operator >=(Vec2I left, Vec2I right)
    {
      return left.X == right.X ? left.Y >= right.Y : left.X > right.X;
    }

    public static implicit operator Vector2(Vec2I value)
    {
      return new Vector2((float) value.X, (float) value.Y);
    }

    public static explicit operator Vec2I(Vector2 value)
    {
     return new Vec2I((int) value.X, (int) value.Y);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is Vec2I other && this.Equals(other);
    }

    public readonly bool Equals(Vec2I other) => this.X == other.X && this.Y == other.Y;

    public override readonly int GetHashCode() => HashCode.Combine<int, int>(this.X, this.Y);

    public override string ToString()
    {
      return $"({X} {Y})";
    }

    /// <summary>
    /// Enumerated index values for the axes.
    /// Returned by <see cref="M:Godot.Vector2I.MaxAxisIndex" /> and <see cref="M:Godot.Vector2I.MinAxisIndex" />.
    /// </summary>
    public enum Axis
    {
      /// <summary>The vector's X axis.</summary>
      X,
      /// <summary>The vector's Y axis.</summary>
      Y,
    }

    public float Angle()
    { 
      return (float)((float)Math.Atan2(Y, X) * (180 / Math.PI));
    }
  }