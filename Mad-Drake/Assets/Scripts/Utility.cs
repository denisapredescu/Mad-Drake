using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtility
{
    public class IntegerCoordinates<T> : IComparable<IntegerCoordinates<T>>
    {
        public Vector2Int Coord { get; set; }

        public int CompareTo(IntegerCoordinates<T> other)
        {
            if (Coord.x < other.Coord.x)
                return -1;
            else if (Coord.x > other.Coord.x)
                return 1;
            else if (Coord.y < other.Coord.y)
                return -1;
            else if (Coord.y > other.Coord.y)
                return 1;

            return 0;
        }
    }

    public class Interval<T>
    {
        public T Min;
        public T Max;

        public Interval()
        {
            Min = default;
            Max = default;
        }

        public Interval(T min, T max)
        {
            Min = min;
            Max = max;
        }
    }

    //a class that can hold objects of a certain type, with any integer coordinates, on a 2D grid 
    public class InfiniteMatrix<T> where T : IntegerCoordinates<T>
    {
        private readonly List<T> values;
        private readonly Dictionary<Vector2Int, int> positionInArray;

        private readonly Interval<int> intervalX;
        private readonly Interval<int> intervalY;

        private readonly Dictionary<int, Interval<int>> intervalXForCertainY;
        private readonly Dictionary<int, Interval<int>> intervalYForCertainX;

        public InfiniteMatrix()
        {
            values = new List<T>();
            positionInArray = new Dictionary<Vector2Int, int>();
            intervalX = new Interval<int>();
            intervalY = new Interval<int>();
            intervalXForCertainY = new Dictionary<int, Interval<int>>();
            intervalYForCertainX = new Dictionary<int, Interval<int>>();
        }

        //.................................................................................................

        private void ChangeIntervals(Vector2Int coord)
        {
            if (values.Count == 0)
            {
                intervalX.Min = intervalX.Max = coord.x;
                intervalY.Min = intervalY.Max = coord.y;
            }
            else
            {
                intervalX.Min = Math.Min(intervalX.Min, coord.x);
                intervalX.Max = Math.Max(intervalX.Max, coord.x);
                intervalY.Min = Math.Min(intervalY.Min, coord.y);
                intervalY.Max = Math.Max(intervalY.Max, coord.y);
            }
        }

        private void ChangeIntervalsForAxes(Vector2Int coord)
        {
            if (!intervalXForCertainY.ContainsKey(coord.y))
            {
                intervalXForCertainY[coord.y] = new Interval<int>(coord.x, coord.x);
            }
            else
            {
                intervalXForCertainY[coord.y].Min = Math.Min(
                    intervalXForCertainY[coord.y].Min,
                    coord.x);
                intervalXForCertainY[coord.y].Max = Math.Max(
                    intervalXForCertainY[coord.y].Max,
                    coord.x);
            }

            if (!intervalYForCertainX.ContainsKey(coord.x))
            {
                intervalYForCertainX[coord.x] = new Interval<int>(coord.y, coord.y);
            }
            else
            {
                intervalYForCertainX[coord.x].Min = Math.Min(
                    intervalYForCertainX[coord.x].Min,
                    coord.y);
                intervalYForCertainX[coord.x].Max = Math.Max(
                    intervalYForCertainX[coord.x].Max,
                    coord.y);
            }
        }

        public bool AddSafe(T newValue)
        {
            if (positionInArray.ContainsKey(newValue.Coord))
                return false;

            ChangeIntervals(newValue.Coord);
            ChangeIntervalsForAxes(newValue.Coord);
            values.Add(newValue);
            positionInArray[newValue.Coord] = values.Count - 1;
            return true;
        }

        public void Add(T newValue)
        {
            if (positionInArray.ContainsKey(newValue.Coord))
            {
                values[positionInArray[newValue.Coord]] = newValue;
            }
            else
            {
                ChangeIntervals(newValue.Coord);
                ChangeIntervalsForAxes(newValue.Coord);
                values.Add(newValue);
                positionInArray[newValue.Coord] = values.Count - 1;
            }
        }

        //.................................................................................................

        public T GetByCoord(Vector2Int coord)
        {
            if (positionInArray.ContainsKey(coord))
                return values[positionInArray[coord]];
            else
                return null;
        }

        public T GetByCoord(int x, int y)
        {
            return GetByCoord(new Vector2Int(x, y));
        }

        public T this[Vector2Int coord]
        {
            get
            {
                return GetByCoord(coord);
            }
        }

        public T this[int x, int y]
        {
            get
            {
                return GetByCoord(new Vector2Int(x, y));
            }
        }

        public T GetByIndex(int index)
        {
            if (index >= 0 && index < values.Count)
                return values[index];
            else
                return null;
        }

        public int GetIndex(Vector2Int coord)
        {
            if (positionInArray.ContainsKey(coord))
                return positionInArray[coord];
            else
                return -1;
        }

        public int GetIndex(int x, int y)
        {
            return GetIndex(new Vector2Int(x, y));
        }

        public Interval<int> GetXInterval()
        {
            if (values.Count == 0)
                return null;
            else
                return intervalX;
        }

        public Interval<int> GetXIntervalForCertainY(int y)
        {
            if (intervalXForCertainY.ContainsKey(y))
                return intervalXForCertainY[y];
            else
                return null;
        }

        public Interval<int> GetYInterval()
        {
            if (values.Count == 0)
                return null;
            else
                return intervalY;
        }

        public Interval<int> GetYIntervalForCertainX(int x)
        {
            if (intervalYForCertainX.ContainsKey(x))
                return intervalYForCertainX[x];
            else
                return null;
        }

        //.................................................................................................

        public bool Exists(Vector2Int coord)
        {
            return positionInArray.ContainsKey(coord);
        }

        public bool Exists(int x, int y)
        {
            return Exists(new Vector2Int(x, y));
        }

        //.................................................................................................

        public int Count()
        {
            return values.Count;
        }

        public void Sort()
        {
            values.Sort();
            for (int i = 0; i < values.Count; i++)
            {
                positionInArray[values[i].Coord] = i;
            }
        }

        public void Clear()
        {
            values.Clear();
            positionInArray.Clear();
            intervalXForCertainY.Clear();
            intervalYForCertainX.Clear();
        }
    }
}