using System.Collections.Generic;
using UnityEngine;

namespace MyUtility
{
    public interface ICoordonates
    {
        public Vector2Int GetCoord();
        public void SetCoord(Vector2Int coord);
    }

    public class InfiniteMatrix<T> where T : ICoordonates
    {
        private List<T> values;
        private Dictionary<Vector2Int, int> positionInArray;

        public InfiniteMatrix()
        {
            values = new List<T>();
            positionInArray = new Dictionary<Vector2Int, int>();
        }

        public int Count()
        {
            return values.Count;
        }

        public T getValue(int index)
        {
            return values[index];
        }

        public int getIndex(Vector2Int coord)
        {
            return positionInArray[coord];
        }

        public int getIndex(int i, int j)
        {
            return positionInArray[new Vector2Int(i, j)];
        }

        public List<T> getAllValues()
        {
            return values;
        }

        public void Clear()
        {
            values = new List<T>();
            positionInArray = new Dictionary<Vector2Int, int>();
        }

        public void Add(T value)
        {
            T convertedValue = (T)value;

            if (positionInArray.ContainsKey(convertedValue.GetCoord()))
            {
                values[positionInArray[convertedValue.GetCoord()]] = convertedValue;
            }
            else
            {
                values.Add(convertedValue);
                positionInArray[convertedValue.GetCoord()] = values.Count - 1;
            }
        }

        public bool Exists(Vector2Int coord)
        {
            return positionInArray.ContainsKey(coord);
        }

        public bool Exists(int i, int j)
        {
            return positionInArray.ContainsKey(new Vector2Int(i, j));
        }

        public T this[Vector2Int coord]
        {
            get 
            {
                return values[positionInArray[coord]];
            }
        }

        public T this[int i, int j]
        {
            get
            {
                return values[positionInArray[new Vector2Int(i, j)]];
            }
        }
    }
}


