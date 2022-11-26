using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//an integer interval struct
[Serializable]
public struct IntRange
{
    [SerializeField]
    private uint from;
    [SerializeField]
    private uint to;
    public uint From
    {
        get { return from; }
    }
    public uint To
    {
        get 
        {
            if (to < from)
            {
                Debug.LogWarning("From is larger than to");
                return from;
            }
            else
                return to;
        }
    }
    public IntRange(uint from, uint to)
    {
        this.from = from;
        this.to = to;
    }
}

//the layout for the interval struct
[CustomPropertyDrawer(typeof(IntRange))]
public class IntRangeDrawerUIE : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var fromRectLabel = new Rect(position.x, position.y, 30, position.height);
        var fromRect = new Rect(position.x + 35, position.y, 50, position.height);
        var toRectLabel = new Rect(position.x + 90, position.y, 20, position.height);
        var toRect = new Rect(position.x + 115, position.y, 50, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.LabelField(fromRectLabel, "From");
        EditorGUI.PropertyField(fromRect, property.FindPropertyRelative("from"), GUIContent.none);
        EditorGUI.LabelField(toRectLabel, "To");
        EditorGUI.PropertyField(toRect, property.FindPropertyRelative("to"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

public enum Direction
{
    Right = 0,
    Down = 1,
    Left = 2,
    Up = 3
}

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

    public void addDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Right:
                this.Right = true;
                break;
            case Direction.Down:
                this.Down = true;
                break;
            case Direction.Left:
                this.Left = true;
                break;
            case Direction.Up:
                this.Up = true;
                break;
            default: break;
        }
    }

    public void removeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                this.Right = false;
                break;
            case Direction.Down:
                this.Down = false;
                break;
            case Direction.Left:
                this.Left = false;
                break;
            case Direction.Up:
                this.Up = false;
                break;
            default: break;
        }
    }
}

public class LevelGenerator : MonoBehaviour
{
    //it holds the relevant data for every room
    private List<RoomInfo> rooms;
    //it holds the index in rooms array for every pair of coordinates
    //if you have the coordinates, you can get the rest of the room data
    private Dictionary<Vector2Int, int> positionInArray;

    //RoomGenerationV1:
    //[SerializeField]
    private float initialChance = 0.8f;
    //[SerializeField]
    private float decreaseChance = 0.2f;

    //RoomGenerationV2:
    [SerializeField]
    private IntRange mainLineRooms = new(10, 20);
    //chance for the main line to go straight and not make many curves:
    [SerializeField, Range(0f, 1f)]
    private float chanceToGoStraight = 1f / 3f;
    [SerializeField]
    private uint maximumLoops = 2;
    [SerializeField, Range(0f, 1f)]
    private float chanceOfLoops = 0.5f;
    //how long can a loop get at it's maximum:
    [SerializeField]
    private uint maximumLoopAddedPoints = 3;
    //help to trnaslate enum Direction to practical changes in coordinates:
    private static readonly int[] directionX = { 1, 0, -1, 0 };
    private static readonly int[] directionY = { 0, -1, 0, 1 };

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

        roomGenerationV2();

        //adds the prefabs for every room
        for (int i = 0; i < rooms.Count; i++)
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

    void roomGenerationV1()
    {
        //queue that is used to go through all the rooms, similar to a BFS
        Queue<int> indexes = new Queue<int>();
        int activeIndex = AddRoom(0, 0);
        rooms[activeIndex].ChanceToExpand = initialChance;
        indexes.Enqueue(activeIndex);

        int newIndex;
        RoomInfo activeRoom, newRoom;

        while (indexes.Count > 0)
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
            if (UnityEngine.Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Left == false)
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
            if (UnityEngine.Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Right == false)
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
            if (UnityEngine.Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Up == false)
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
            if (UnityEngine.Random.Range(0.0f, 1.0f) < activeRoom.ChanceToExpand && activeRoom.Down == false)
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
    }

    //returns a tuple with the adjacent directions
    private Tuple<Direction, Direction> neighbourDirections(Direction direction)
    {
        if(direction == Direction.Right || direction == Direction.Left)
        {
            return new Tuple<Direction, Direction>(Direction.Down, Direction.Up);
        }
        else
        {
            return new Tuple<Direction, Direction>(Direction.Left, Direction.Right);
        }
    }

    //gives the opposite direction
    private Direction oppositeDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Left: return Direction.Right;
            case Direction.Down: return Direction.Up;
            case Direction.Right: return Direction.Left;
            case Direction.Up: return Direction.Down;
            default: return direction;
        }
    }

    //function that checks if moving from a room with a certain direction would lead to another room
    private bool roomExists(RoomInfo room, Direction direction)
    {
        if (positionInArray.ContainsKey(
            new Vector2Int(
                room.X + directionX[(int)direction],
                room.Y + directionY[(int)direction]
            )) == false)
        {
            return false;
        }

        return true;
    }

    //builds a certain number of rooms in one direction from a given room
    private int buildRooms(RoomInfo room, Direction direction, int length)
    {
        int index = -1;
        for (int i = 0; i < length; i++)
        {
            if(roomExists(room, direction))
            {
                int secondIndex = positionInArray[new Vector2Int(
                    room.X + directionX[(int)direction],
                    room.Y + directionY[(int)direction])];

                room.addDirection(direction);
                rooms[secondIndex].addDirection(oppositeDirection(direction));
                return -1;
            }
            else
            {
                int newIndex = AddRoom(
                    room.X + directionX[(int)direction],
                    room.Y + directionY[(int)direction]
                    );

                room.addDirection(direction);
                rooms[newIndex].addDirection(oppositeDirection(direction));

                index = newIndex;
                room = rooms[index];
            }
        }

        return index;
    }

    //generates the rooms
    void roomGenerationV2()
    {
        CreateMainLine();
        CreateLoops();
    }

    void CreateMainLine()
    {
        int activeIndex = AddRoom(0, 0);
        Direction activeDirection = Direction.Right;

        int mainRooms = Math.Max(
            UnityEngine.Random.Range(Convert.ToInt32(mainLineRooms.From), Convert.ToInt32(mainLineRooms.To) + 1),
            1);

        float probChangeDir = (1.0f - chanceToGoStraight) / 2.0f;
        for (int i = 0; i < mainRooms; i++)
        {
            RoomInfo activeRoom = rooms[activeIndex];
            Tuple<Direction, Direction> neighbourDir = neighbourDirections(activeDirection);
            float totalProb = 0.0f;

            //next 3 <if> add themselves to the probability pool if they are valid
            if (!roomExists(activeRoom, activeDirection))
            {
                totalProb += chanceToGoStraight;
            }

            if (!roomExists(activeRoom, neighbourDir.Item1) && neighbourDir.Item1 != Direction.Left)
            {
                totalProb += probChangeDir;
            }

            if (!roomExists(activeRoom, neighbourDir.Item2) && neighbourDir.Item2 != Direction.Left)
            {
                totalProb += probChangeDir;
            }

            //random pick of the room
            float chosenProb = UnityEngine.Random.Range(0f, totalProb);
            bool changedDir = false;

            //next 3 <if> decide which room was picked
            if (!roomExists(activeRoom, activeDirection))
            {
                if (chosenProb > chanceToGoStraight)
                {
                    chosenProb -= chanceToGoStraight;
                }
                else
                {
                    changedDir = true;
                }
            }

            if (!changedDir && !roomExists(activeRoom, neighbourDir.Item1) && neighbourDir.Item1 != Direction.Left)
            {
                if (chosenProb > probChangeDir)
                {
                    chosenProb -= probChangeDir;
                }
                else
                {
                    activeDirection = neighbourDir.Item1;
                    changedDir = true;
                }
            }

            if (!changedDir)
            {
                activeDirection = neighbourDir.Item2;
            }

            //add the room and the doors
            int newIndex = AddRoom(
                activeRoom.X + directionX[(int)activeDirection],
                activeRoom.Y + directionY[(int)activeDirection]);
            rooms[newIndex].addDirection(oppositeDirection(activeDirection));
            rooms[activeIndex].addDirection(activeDirection);
            activeIndex = newIndex;
        }
    }
    void CreateLoops()
    {
        if (maximumLoopAddedPoints == 0 || maximumLoops == 0)
            return;

        //rooms are in the right order and that is very usefull
        int lengthOfSortedPoints = rooms.Count; //used to pick the 2 random points for the loop
        //building the loops
        for (int i = 0; i < maximumLoops; i++)
        {
            if (UnityEngine.Random.Range(0f, 1f) > chanceOfLoops)
                continue;

            //choosing randomly the points
            int firstPoint = UnityEngine.Random.Range(0, lengthOfSortedPoints - 1); //Count - 1 + 1(it's exclusive)
            int secondPoint = UnityEngine.Random.Range(
                firstPoint + 1,
                Math.Min(firstPoint + Convert.ToInt32(maximumLoopAddedPoints) + 1, lengthOfSortedPoints - 1) + 1 //same here
                );

            //choosing directions and how many rooms are built in each direction
            RoomInfo roomStart = rooms[firstPoint];
            RoomInfo roomEnd = rooms[secondPoint];
            Direction firstDir, secondDir, thirdDir = Direction.Left;
            int firstSteps, secondSteps, thirdSteps;

            if (roomStart.X == roomEnd.X)
            {
                int probability = UnityEngine.Random.Range(0, 2);
                if (probability == 0 && !roomExists(roomStart, Direction.Left))
                {
                    firstDir = Direction.Left;
                }
                else if (!roomExists(roomStart, Direction.Right))
                {
                    firstDir = Direction.Right;
                }
                else
                {
                    firstDir = Direction.Left;
                }


                if (roomStart.Y < roomEnd.Y)
                {
                    secondDir = Direction.Up;
                }
                else
                {
                    secondDir = Direction.Down;
                }

                thirdDir = oppositeDirection(firstDir);
                secondSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                /*
                 * not making a loop stupid, it makes sure that the number of rooms that put a distance between
                 * the main corridor and the main line is not too high compared to the length of the corridor
                 */
                firstSteps = thirdSteps = 1 + UnityEngine.Random.Range(
                    0,
                    Math.Min(secondSteps - 1, Convert.ToInt32((maximumLoopAddedPoints - secondSteps) / 2) + 1)
                    );
            }
            else if (roomStart.Y == roomEnd.Y)
            {
                int probability = UnityEngine.Random.Range(0, 2);
                if (probability == 0 && !roomExists(roomStart, Direction.Up))
                {
                    firstDir = Direction.Up;
                }
                else if (!roomExists(roomStart, Direction.Down))
                {
                    firstDir = Direction.Down;
                }
                else
                {
                    firstDir = Direction.Up;
                }

                secondDir = Direction.Right;
                thirdDir = oppositeDirection(firstDir);
                secondSteps = Math.Abs(roomStart.X - roomEnd.X);
                /*
                 * not making a loop stupid, it makes sure that the number of rooms that put a distance between
                 * the main corridor and the main line is not too high compared to the length of the corridor
                 */
                firstSteps = thirdSteps = 1 + UnityEngine.Random.Range(
                    0,
                    Math.Min(secondSteps - 1, Convert.ToInt32((maximumLoopAddedPoints - secondSteps) / 2) + 1)
                    );
            }
            else
            {
                int probability = UnityEngine.Random.Range(0, 2);
                if (roomStart.Y < roomEnd.Y)
                {
                    if (probability == 0 && !roomExists(roomStart, Direction.Up))
                    {
                        firstDir = Direction.Up;
                        firstSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                    }
                    else if (!roomExists(roomStart, Direction.Right))
                    {
                        firstDir = Direction.Right;
                        firstSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                        secondDir = Direction.Up;
                        secondSteps = Mathf.Abs(roomStart.Y - roomEnd.Y);
                    }
                    else
                    {
                        firstDir = Direction.Up;
                        firstSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                    }
                }
                else
                {
                    if (probability == 0 && !roomExists(roomStart, Direction.Down))
                    {
                        firstDir = Direction.Down;
                        firstSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                    }
                    else if (!roomExists(roomStart, Direction.Right))
                    {
                        firstDir = Direction.Right;
                        firstSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                        secondDir = Direction.Down;
                        secondSteps = Mathf.Abs(roomStart.Y - roomEnd.Y);
                    }
                    else
                    {
                        firstDir = Direction.Down;
                        firstSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                    }
                }

                thirdSteps = 0;
            }

            //building the path
            int activeIndex = buildRooms(roomStart, firstDir, firstSteps);
            if (activeIndex != -1)
                activeIndex = buildRooms(rooms[activeIndex], secondDir, secondSteps);
            if (activeIndex != -1 && thirdSteps != 0)
                buildRooms(rooms[activeIndex], thirdDir, thirdSteps);
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

        Instantiate(grounds[UnityEngine.Random.Range(0, grounds.Length)], posInstantiate, Quaternion.identity);
    }
}
