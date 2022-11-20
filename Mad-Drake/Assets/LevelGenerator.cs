using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class RoomInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public float ChanceToExpand { get; set; }
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }

    public RoomInfo(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
        this.ChanceToExpand = 0.0f;
        Up = Down = Left = Right = false;
    }
}

public class LevelGenerator : MonoBehaviour
{
    //it holds the relevant data for every room
    private List<RoomInfo> rooms;
    //it holds the index in rooms array for every pair of coordinates
    //if you have the coordinates, you can get the rest of the room data
    private Dictionary<Vector2Int, int> positionInArray;
    [SerializeField]
    private float initialChance = 0.8f;
    [SerializeField]
    private float decreaseChance = 0.2f;

    [SerializeField]
    private GameObject doorsNone;

    [SerializeField]
    private GameObject doorsLeft;
    [SerializeField]
    private GameObject doorsUp;
    [SerializeField]
    private GameObject doorsRight;
    [SerializeField]
    private GameObject doorsDown;

    [SerializeField]
    private GameObject doorsLeftUp;
    [SerializeField]
    private GameObject doorsUpRight;
    [SerializeField]
    private GameObject doorsRightDown;
    [SerializeField]
    private GameObject doorsLeftDown;
    [SerializeField]
    private GameObject doorsLeftRight;
    [SerializeField]
    private GameObject doorsUpDown;

    [SerializeField]
    private GameObject doorsLeftUpRight;
    [SerializeField]
    private GameObject doorsUpRightDown;
    [SerializeField]
    private GameObject doorsLeftUpDown;
    [SerializeField]
    private GameObject doorsLeftRightDown;

    [SerializeField]
    private GameObject doorsAll;

    [SerializeField]
    private GameObject[] grounds;

    //adds the new room in array and also the reference in the map
    private int AddRoom(int x, int y)
    {
        rooms.Add(new RoomInfo(x, y));
        positionInArray[new Vector2Int(x, y)] = rooms.Count - 1;
        //returns the position in array of the room created
        return rooms.Count - 1;
    }

    private void Start()
    {
        rooms = new List<RoomInfo>();
        positionInArray = new Dictionary<Vector2Int, int>();

        //queue that is used to go through all the rooms, similar to a BFS
        Queue <int> indexes = new Queue<int>();
        int activeIndex = AddRoom(0, 0);
        rooms[activeIndex].ChanceToExpand = initialChance;
        indexes.Enqueue(activeIndex);

        int newIndex;
        RoomInfo activeRoom, newRoom;

        while(indexes.Count > 0)
        {
            activeIndex = indexes.Dequeue();
            //this is used for cleaner code and a little more performance
            activeRoom = rooms[activeIndex];

            /*
             * There are 4 blocks very similar, for every direction, going to explain only this one.
             * If the probability falls in range, then it creates a room, the smaller the range,
             * the smaller the chance to expand the room.
             * The range is (0.0f, activeRoom.ChanceToExpand)
            */
            if (Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Left == false)
            {
                activeRoom.Left = true;
                if (positionInArray.ContainsKey(new Vector2Int(activeRoom.X - 1, activeRoom.Y)) == false)
                {
                    newIndex = AddRoom(activeRoom.X - 1, activeRoom.Y);
                    indexes.Enqueue(newIndex);
                    newRoom = rooms[newIndex];
                    //decrease the chance so the map expands less the further it goes
                    newRoom.ChanceToExpand = activeRoom.ChanceToExpand - decreaseChance;
                    newRoom.Right = true;
                }
                else
                {
                    //if the room was created it doesn't need to be added to the queue again
                    newIndex = positionInArray[new Vector2Int(activeRoom.X - 1, activeRoom.Y)];
                    newRoom = rooms[newIndex];
                    newRoom.Right = true;
                }
            }

            //see the explanations written for the first block
            if (Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Right == false)
            {
                activeRoom.Right = true;
                if (positionInArray.ContainsKey(new Vector2Int(activeRoom.X + 1, activeRoom.Y)) == false)
                {
                    newIndex = AddRoom(activeRoom.X + 1, activeRoom.Y);
                    indexes.Enqueue(newIndex);
                    newRoom = rooms[newIndex];
                    newRoom.ChanceToExpand = activeRoom.ChanceToExpand - decreaseChance;
                    newRoom.Left = true;
                }
                else
                {
                    newIndex = positionInArray[new Vector2Int(activeRoom.X + 1, activeRoom.Y)];
                    newRoom = rooms[newIndex];
                    newRoom.Left = true;
                }
            }

            //see the explanations written for the first block
            if (Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Up == false)
            {
                activeRoom.Up = true;
                if (positionInArray.ContainsKey(new Vector2Int(activeRoom.X, activeRoom.Y + 1)) == false)
                {
                    newIndex = AddRoom(activeRoom.X, activeRoom.Y + 1);
                    indexes.Enqueue(newIndex);
                    newRoom = rooms[newIndex];
                    newRoom.ChanceToExpand = activeRoom.ChanceToExpand - decreaseChance;
                    newRoom.Down = true;
                }
                else
                {
                    newIndex = positionInArray[new Vector2Int(activeRoom.X, activeRoom.Y + 1)];
                    newRoom = rooms[newIndex];
                    newRoom.Down = true;
                }
            }

            //see the explanations written for the first block
            if (Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Down == false)
            {
                activeRoom.Down = true;
                if (positionInArray.ContainsKey(new Vector2Int(activeRoom.X, activeRoom.Y - 1)) == false)
                {
                    newIndex = AddRoom(activeRoom.X, activeRoom.Y - 1);
                    indexes.Enqueue(newIndex);
                    newRoom = rooms[newIndex];
                    newRoom.ChanceToExpand = activeRoom.ChanceToExpand - decreaseChance;
                    newRoom.Up = true;
                }
                else
                {
                    newIndex = positionInArray[new Vector2Int(activeRoom.X, activeRoom.Y - 1)];
                    newRoom = rooms[newIndex];
                    newRoom.Up = true;
                }
            }
        }

        //adds the prefabs for every room
        for(int i = 0; i < rooms.Count; i++)
        {
            /*Debug.Log(rooms[i].X.ToString() + " "
                + rooms[i].Y.ToString() + " "
                + (rooms[i].Left ? "Left " : "")
                + (rooms[i].Up ? "Up " : "")
                + (rooms[i].Right ? "Right " : "")
                + (rooms[i].Down ? "Down " : ""));*/
            Build(i);
        }
    }

    //using the informations in the room, adds the proper prefab
    private void Build(int position)
    {
        RoomInfo room = rooms[position];
        Vector3 posInstantiate = new Vector3(room.X * 18.0f, room.Y * 10.0f, 0.0f);
        if(!room.Left && !room.Up && !room.Right && !room.Down) //1
        {
            Instantiate(doorsNone, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && !room.Up && !room.Right && !room.Down) //2
        {
            Instantiate(doorsLeft, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && room.Up && !room.Right && !room.Down) //3
        {
            Instantiate(doorsUp, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && !room.Up && room.Right && !room.Down) //4
        {
            Instantiate(doorsRight, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && !room.Up && !room.Right && room.Down) //5
        {
            Instantiate(doorsDown, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && room.Up && !room.Right && !room.Down) //6
        {
            Instantiate(doorsLeftUp, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && !room.Up && room.Right && !room.Down) //7
        {
            Instantiate(doorsLeftRight, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && !room.Up && !room.Right && room.Down) //8
        {
            Instantiate(doorsLeftDown, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && room.Up && room.Right && !room.Down) //9
        {
            Instantiate(doorsUpRight, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && room.Up && !room.Right && room.Down) //10
        {
            Instantiate(doorsUpDown, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && !room.Up && room.Right && room.Down) //11
        {
            Instantiate(doorsRightDown, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && room.Up && room.Right && !room.Down) //12
        {
            Instantiate(doorsLeftUpRight, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && room.Up && !room.Right && room.Down) //13
        {
            Instantiate(doorsLeftUpDown, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && !room.Up && room.Right && room.Down) //14
        {
            Instantiate(doorsLeftRightDown, posInstantiate, Quaternion.identity);
        }
        else if (!room.Left && room.Up && room.Right && room.Down) //15
        {
            Instantiate(doorsUpRightDown, posInstantiate, Quaternion.identity);
        }
        else if (room.Left && room.Up && room.Right && room.Down) //16
        {
            Instantiate(doorsAll, posInstantiate, Quaternion.identity);
        }

        Instantiate(grounds[Random.Range(0, grounds.Length)], posInstantiate, Quaternion.identity);
    }
}
