using System.Collections.Generic;
using UnityEngine;

namespace MyUtility
{
    //necessary interface for an object to be used in an InfiniteMatrix object
    public interface ICoordonates
    {
        public Vector2Int GetCoord();
        public void SetCoord(Vector2Int coord);
    }

    //a class that can hold objects of a certain type, with any integer coordinates, on a 2D grid 
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

        //modifying methods:
        public void Clear()
        {
            values = new List<T>();
            positionInArray = new Dictionary<Vector2Int, int>();
        }

        public void AddValue(T value)
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

        //checking values existence:
        public bool Exists(Vector2Int coord)
        {
            return positionInArray.ContainsKey(coord);
        }

        public bool Exists(int i, int j)
        {
            return positionInArray.ContainsKey(new Vector2Int(i, j));
        }

        //accessing methods:
        public int GetIndex(Vector2Int coord)
        {
            return positionInArray[coord];
        }

        public int GetIndex(int i, int j)
        {
            return positionInArray[new Vector2Int(i, j)];
        }

        public T GetValueByCoord(Vector2Int coord)
        {
            return values[positionInArray[coord]];
        }

        public T GetValueByCoord(int i, int j)
        {
            return values[positionInArray[new Vector2Int(i, j)]];
        }

        public T GetValueByIndex(int index)
        {
            return values[index];
        }

        public List<T> GetAllValues()
        {
            return values;
        }
    }
}


