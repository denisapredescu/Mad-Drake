using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MyUtility;

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

//informations for building special paths
[Serializable]
public struct SpecialPath
{
    [Range(0f, 1f)]
    public float chanceToSpawn;
    public IntRange normalRooms;
    public List<GameObject> specialRooms;
}

public enum Direction
{
    Right = 0,
    Down = 1,
    Left = 2,
    Up = 3
}

public class RoomInfo : IComparable<RoomInfo>, ICoordonates
{
    public int X { get; private set; }
    public int Y { get; private set; }
    private Vector2Int coord;
    public float ChanceToExpand { get; set; }
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
    public GameObject ownGround { get; set; }

    public RoomInfo(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
        coord = new Vector2Int(X, Y);
        ChanceToExpand = 0.0f;
        Up = Down = Left = Right = false;
        ownGround = null;
    }

    public void addDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Right:
                Right = true;
                break;
            case Direction.Down:
                Down = true;
                break;
            case Direction.Left:
                Left = true;
                break;
            case Direction.Up:
                Up = true;
                break;
            default: break;
        }
    }

    public void removeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                Right = false;
                break;
            case Direction.Down:
                Down = false;
                break;
            case Direction.Left:
                Left = false;
                break;
            case Direction.Up:
                Up = false;
                break;
            default: break;
        }
    }

    public int CompareTo(RoomInfo other)
    {
        if (X < other.X)
            return -1;
        else if (X > other.X)
            return 1;
        else if (Y < other.Y)
            return -1;
        else if (Y > other.Y)
            return 1;

        return 0;
    }

    public Vector2Int GetCoord()
    {
        return coord;
    }

    public void SetCoord(Vector2Int coord)
    {
        this.coord = coord;
        X = coord.x;
        Y = coord.y;
    }
}

public class LevelGenerator : MonoBehaviour
{
    //it holds the relevant data for every room
    private InfiniteMatrix<RoomInfo> rooms;

    /*//RoomGenerationV1:
    //[SerializeField]
    private float initialChance = 0.8f;
    //[SerializeField]
    private float decreaseChance = 0.2f;*/

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
    [SerializeField]
    private List<SpecialPath> specialPaths;
    //adding special rooms at the end and start of the main line
    [SerializeField]
    private List<GameObject> specialStartRooms;
    [SerializeField]
    private List<GameObject> specialEndRooms;
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
        rooms.AddValue(new RoomInfo(x, y));
        //returns the position in array of the room created
        return rooms.Count() - 1;
    }

    private void Start()
    {
        rooms = new InfiniteMatrix<RoomInfo>();

        RoomGenerationV2();

        //adds the prefabs for every room
        for (int i = 0; i < rooms.Count(); i++)
        {
            Build(i);
        }
    }

    /*void roomGenerationV1()
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

            // There are 4 blocks very similar, for every direction, going to explain only this one.
            // If the probability falls in range, then it creates a room, the smaller the range,
            // the smaller the chance to expand the room.
            // The range is (0.0f, activeRoom.ChanceToExpand)
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
    }*/

    //returns a tuple with the adjacent directions
    private Tuple<Direction, Direction> NeighbourDirections(Direction direction)
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
    private Direction OppositeDirection(Direction direction)
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
    private bool RoomExists(RoomInfo room, Direction direction)
    {
        if (rooms.Exists(
                room.X + directionX[(int)direction],
                room.Y + directionY[(int)direction]
            ) == false)
        {
            return false;
        }

        return true;
    }

    //builds a certain number of rooms in one direction from a given room
    private int BuildRooms(RoomInfo room, Direction direction, int length)
    {
        int index = -1;
        for (int i = 0; i < length; i++)
        {
            if(RoomExists(room, direction))
            {
                int secondIndex = rooms.GetIndex(
                    room.X + directionX[(int)direction],
                    room.Y + directionY[(int)direction]);

                room.addDirection(direction);
                rooms.GetValueByIndex(secondIndex).addDirection(OppositeDirection(direction));
                return -1;
            }
            else
            {
                int newIndex = AddRoom(
                    room.X + directionX[(int)direction],
                    room.Y + directionY[(int)direction]
                    );

                room.addDirection(direction);
                rooms.GetValueByIndex(newIndex).addDirection(OppositeDirection(direction));

                index = newIndex;
                room = rooms.GetValueByIndex(index);
            }
        }

        return index;
    }

    private void BuildSpecialRooms(RoomInfo room, Direction direction, int length, List<GameObject> specialRooms)
    {
        int index = rooms.GetIndex(room.GetCoord());
        if(length > 0)
        {
            index = BuildRooms(room, direction, length);
            room = rooms.GetValueByIndex(index);
        }
        
        foreach(GameObject obj in specialRooms)
        {
            int newIndex = AddRoom(room.X + directionX[(int)direction], room.Y + directionY[(int)direction]);
            
            room.addDirection(direction);
            rooms.GetValueByIndex(newIndex).addDirection(OppositeDirection(direction));
            
            index = newIndex;
            room = rooms.GetValueByIndex(index);
            room.ownGround = obj;
        }
    }

    //generates the rooms
    void RoomGenerationV2()
    {
        Tuple<RoomInfo, RoomInfo> startAndEndRooms = CreateMainLine();
        CreateLoops();
        CreateSpecialPaths();
        CreateSpecialRoomsOnMainLine(startAndEndRooms.Item1, startAndEndRooms.Item2);
    }

    Tuple<RoomInfo, RoomInfo> CreateMainLine()
    {
        int activeIndex = AddRoom(0, 0);
        Direction activeDirection = Direction.Right;

        int mainRooms = Math.Max(
            UnityEngine.Random.Range(Convert.ToInt32(mainLineRooms.From), Convert.ToInt32(mainLineRooms.To) + 1),
            1);

        float probChangeDir = (1.0f - chanceToGoStraight) / 2.0f;
        for (int i = 0; i < mainRooms; i++)
        {
            RoomInfo activeRoom = rooms.GetValueByIndex(activeIndex);
            Tuple<Direction, Direction> neighbourDir = NeighbourDirections(activeDirection);
            float totalProb = 0.0f;

            //next 3 <if> add themselves to the probability pool if they are valid
            if (!RoomExists(activeRoom, activeDirection))
            {
                totalProb += chanceToGoStraight;
            }

            if (!RoomExists(activeRoom, neighbourDir.Item1) && neighbourDir.Item1 != Direction.Left)
            {
                totalProb += probChangeDir;
            }

            if (!RoomExists(activeRoom, neighbourDir.Item2) && neighbourDir.Item2 != Direction.Left)
            {
                totalProb += probChangeDir;
            }

            //random pick of the room
            float chosenProb = UnityEngine.Random.Range(0f, totalProb);
            bool changedDir = false;

            //next 3 <if> decide which room was picked
            if (!RoomExists(activeRoom, activeDirection))
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

            if (!changedDir && !RoomExists(activeRoom, neighbourDir.Item1) && neighbourDir.Item1 != Direction.Left)
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
            rooms.GetValueByIndex(newIndex).addDirection(OppositeDirection(activeDirection));
            rooms.GetValueByIndex(activeIndex).addDirection(activeDirection);
            activeIndex = newIndex;
        }

        //returns the first and the last room
        return new Tuple<RoomInfo, RoomInfo>(rooms.GetValueByIndex(0), rooms.GetValueByIndex(rooms.Count() - 1));
    }
    void CreateLoops()
    {
        if (maximumLoopAddedPoints == 0 || maximumLoops == 0)
            return;

        //rooms are in the right order and that is very usefull
        int lengthOfSortedPoints = rooms.Count(); //used to pick the 2 random points for the loop
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
            RoomInfo roomStart = rooms.GetValueByIndex(firstPoint);
            RoomInfo roomEnd = rooms.GetValueByIndex(secondPoint);
            Direction firstDir, secondDir, thirdDir = Direction.Left;
            int firstSteps, secondSteps, thirdSteps;

            if (roomStart.X == roomEnd.X)
            {
                int probability = UnityEngine.Random.Range(0, 2);
                if (probability == 0 && !RoomExists(roomStart, Direction.Left))
                {
                    firstDir = Direction.Left;
                }
                else if (!RoomExists(roomStart, Direction.Right))
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

                thirdDir = OppositeDirection(firstDir);
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
                if (probability == 0 && !RoomExists(roomStart, Direction.Up))
                {
                    firstDir = Direction.Up;
                }
                else if (!RoomExists(roomStart, Direction.Down))
                {
                    firstDir = Direction.Down;
                }
                else
                {
                    firstDir = Direction.Up;
                }

                secondDir = Direction.Right;
                thirdDir = OppositeDirection(firstDir);
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
                    if (probability == 0 && !RoomExists(roomStart, Direction.Up))
                    {
                        firstDir = Direction.Up;
                        firstSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                    }
                    else if (!RoomExists(roomStart, Direction.Right))
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
                    if (probability == 0 && !RoomExists(roomStart, Direction.Down))
                    {
                        firstDir = Direction.Down;
                        firstSteps = Math.Abs(roomStart.Y - roomEnd.Y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.X - roomEnd.X);
                    }
                    else if (!RoomExists(roomStart, Direction.Right))
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
            int activeIndex = BuildRooms(roomStart, firstDir, firstSteps);
            if (activeIndex != -1)
                activeIndex = BuildRooms(rooms.GetValueByIndex(activeIndex), secondDir, secondSteps);
            if (activeIndex != -1 && thirdSteps != 0)
                BuildRooms(rooms.GetValueByIndex(activeIndex), thirdDir, thirdSteps);
        }
    }
    void CreateSpecialPaths()
    {
        if (specialPaths.Count == 0) 
            return;

        int smallestX = rooms.GetValueByIndex(0).X;
        int biggestX = rooms.GetValueByIndex(0).X;
        Dictionary<int, int> smallestY = new Dictionary<int, int>();
        //for each X that has at least a room on it's axis, it gives the room that has the smallest value of Y
        Dictionary<int, int> biggestY = new Dictionary<int, int>();
        //for each X that has at least a room on it's axis, it gives the room that has the biggest value of Y

        //finding the rooms for the maps
        foreach (RoomInfo room in rooms.GetAllValues())
        {
            smallestX = Math.Min(smallestX, room.X);
            biggestX = Math.Max(biggestX, room.X);

            if(smallestY.ContainsKey(room.X))
            {
                smallestY[room.X] = Math.Min(smallestY[room.X], room.Y);
                biggestY[room.X] = Math.Max(biggestY[room.X], room.Y);
            }
            else
            {
                smallestY[room.X] = room.Y;
                biggestY[room.X] = room.Y;
            }
        }

        //building the paths
        foreach (SpecialPath specialPath in specialPaths)
        {
            //randomly choosing if it spawns or not
            if (UnityEngine.Random.Range(0f, 1f) <= specialPath.chanceToSpawn)
            {
                int numberOfRooms = UnityEngine.Random.Range(
                    Convert.ToInt32(specialPath.normalRooms.From),
                    Convert.ToInt32(specialPath.normalRooms.To) + 1);

                int x = UnityEngine.Random.Range(smallestX, biggestX + 1);
                float probDir = UnityEngine.Random.Range(0, 2);
                Direction dir;
                if (probDir == 0)
                    dir = Direction.Up;
                else
                    dir = Direction.Down;

                //choosing the room in a manner that assures that the new path doesn't collide with other rooms
                RoomInfo room;
                if (dir == Direction.Up)
                    room = rooms.GetValueByCoord(x, biggestY[x]);
                else
                    room = rooms.GetValueByCoord(x, smallestY[x]);

                //building the path
                BuildSpecialRooms(room, dir, numberOfRooms, specialPath.specialRooms);

                //updating the maps
                if (dir == Direction.Up)
                    biggestY[x] += numberOfRooms + specialPath.specialRooms.Count;
                else
                    smallestY[x] -= numberOfRooms + specialPath.specialRooms.Count;
            }
        }
    }
    void CreateSpecialRoomsOnMainLine(RoomInfo startRoom, RoomInfo endRoom)
    {
        if(specialStartRooms.Count > 0)
        {
            specialStartRooms.Reverse();
            BuildSpecialRooms(startRoom, Direction.Left, 0, specialStartRooms);
        }

        if(specialEndRooms.Count > 0)
            BuildSpecialRooms(endRoom, Direction.Right, 0, specialEndRooms);
    }

    //using the informations in the room, adds the proper prefab
    private void Build(int position)
    {
        RoomInfo room = rooms.GetValueByIndex(position);
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

        if(room.ownGround == null)
            Instantiate(grounds[UnityEngine.Random.Range(0, grounds.Length)], posInstantiate, Quaternion.identity);
        else
            Instantiate(room.ownGround, posInstantiate, Quaternion.identity);
    }
}
