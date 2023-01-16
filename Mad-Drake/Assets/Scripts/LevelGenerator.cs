using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MyUtility;
using System.Linq;

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

[Serializable]
public struct Collectible
{
    public GameObject gameObject;
    public int maximumNumber;
    [Range(0.0f, 0.99f)]
    public float chanceOfSpawn;
}

[Serializable]
public struct Enemy
{
    public GameObject enemy;
    public int valueOfEnemy;
}

public enum Direction
{
    Right = 0,
    Down = 1,
    Left = 2,
    Up = 3
}

public class InsideRoomObject : IntegerCoordinates<InsideRoomObject>
{
    public GameObject SpawnedObject { get; set; }
    public InsideRoomObject(GameObject gameObject, Vector2Int coord)
    {
        Coord = coord;
        SpawnedObject = gameObject;
    }
    public InsideRoomObject(GameObject gameObject, int x, int y)
    {
        Coord = new Vector2Int(x, y);
        SpawnedObject = gameObject;
    }
}

public class RoomInfo : IntegerCoordinates<RoomInfo>
{
    public float ChanceToExpand { get; set; }
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
    public GameObject OwnGround { get; set; }
    public InfiniteMatrix<InsideRoomObject> Collectibles { get; set; }
    public InfiniteMatrix<InsideRoomObject> Enemies { get; set; }

    public RoomInfo(Vector2Int coord)
    {
        Coord = coord;
        ChanceToExpand = 0.0f;
        Up = Down = Left = Right = false;
        OwnGround = null;
        Collectibles = new InfiniteMatrix<InsideRoomObject> { };
        Enemies = new InfiniteMatrix<InsideRoomObject> { };
    }

    public RoomInfo(int x, int y)
    {
        Coord = new Vector2Int(x, y);
        ChanceToExpand = 0.0f;
        Up = Down = Left = Right = false;
        OwnGround = null;
        Collectibles = new InfiniteMatrix<InsideRoomObject> { };
        Enemies = new InfiniteMatrix<InsideRoomObject> { };
    }

    public void AddDirection(Direction direction)
    {
        switch (direction)
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

    public void RemoveDirection(Direction direction)
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
}

public class LevelGenerator : MonoBehaviour
{
    //it holds the relevant data for every room
    private InfiniteMatrix<RoomInfo> rooms;

    //RoomGeneration:
    [SerializeField]
    private float roomWidth = 18.0f;
    [SerializeField]
    private float roomHeight = 10.0f;
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
    private GameObject playerIfUsingStartRooms = null;
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

    private readonly float approachToCenter = 0.5f;
    //this values is used to calculate the exact position for instantiation
    /*
     *                      C(0, 0) 
     *
     *         (x+1,y+1)
     *       P
     *(x, y)
     * 
     * P - the exact position, C - the center
     * the idea is to place it in a tileof size 1x1
    */
    [SerializeField]
    private int maximumXForSpawn = 8;
    [SerializeField]
    private int maximumYForSpawn = 4;
    //those 2 decide how further from center can an object be spawned
    //all of this is done in order to make sure the objects are not overlapping
    [SerializeField]
    private List<Collectible> collectibles;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Transform playerCamera;
    private Vector3 lastCameraPosition;
    private Vector2Int activeRoomCoordinates;
    private RoomController roomController;
    [SerializeField]
    private IntRange enemiesInRoom = new(3, 5);
    [SerializeField]
    private List<Enemy> enemies;

    //adds the new room in array and also the reference in the map
    private int AddRoom(int x, int y)
    {
        //returns the position in array of the room created
        if (rooms.AddSafe(new RoomInfo(x, y)))
            return rooms.Count() - 1;

        return -1;
    }

    private void Start()
    {
        rooms = new InfiniteMatrix<RoomInfo>();
        roomController = GetComponent<RoomController>();
        lastCameraPosition = playerCamera.position;
        activeRoomCoordinates.x = (int)(lastCameraPosition.x / roomWidth);
        activeRoomCoordinates.y = (int)(lastCameraPosition.y / roomHeight);

        RoomGeneration();

        //adds the prefabs for every room
        for (int i = 0; i < rooms.Count(); i++)
        {
            Build(i);
        }
    }

    private RoomInfo activeRoomForEnemies;
    private bool lockRoom = false;

    //verifies if the enemy is in the room
    private bool InRoom(Vector2Int center, Vector3 position)
    {
        float minX = center.x * roomWidth - roomWidth / 2 - 0.5f;
        float maxX = center.x * roomWidth + roomWidth / 2 + 0.5f;
        float minY = center.y * roomHeight - roomHeight / 2 - 0.5f;
        float maxY = center.y * roomHeight + roomHeight / 2 + 0.5f;

        if (position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY)
            return true;
        return false;
    }

    private bool HasActiveEnemies(RoomInfo room)
    {
        for (int i = 0; i < room.Enemies.Count(); i++)
            if (room.Enemies.GetByIndex(i).SpawnedObject.activeSelf && 
                InRoom(room.Coord, room.Enemies.GetByIndex(i).SpawnedObject.transform.position))
                return true;

        return false;
    }

    private void Update()
    {
        if (lastCameraPosition != playerCamera.position)
        {
            if (lastCameraPosition.x < playerCamera.position.x)
                activeRoomCoordinates.x++;
            else if (lastCameraPosition.x > playerCamera.position.x)
                activeRoomCoordinates.x--;

            if (lastCameraPosition.y < playerCamera.position.y)
                activeRoomCoordinates.y++;
            else if (lastCameraPosition.y > playerCamera.position.y)
                activeRoomCoordinates.y--;

            activeRoomForEnemies = rooms.GetByCoord(activeRoomCoordinates);

            if (HasActiveEnemies(activeRoomForEnemies))
            {
                roomController.CloseDoors();

                for (int i = 0; i < activeRoomForEnemies.Enemies.Count(); i++)
                {
                    GameObject enemy = activeRoomForEnemies.Enemies.GetByIndex(i).SpawnedObject;
                    enemy.GetComponent<UniversalSetTarget>().SetEnemyActivity(true);
                }

                lockRoom = true;
            }

            lastCameraPosition = playerCamera.position;
        }

        if (lockRoom && !HasActiveEnemies(activeRoomForEnemies))
        {
            roomController.OpenDoors();
            lockRoom = false;
        }
    }

    //returns a tuple with the adjacent directions
    private Tuple<Direction, Direction> NeighbourDirections(Direction direction)
    {
        if (direction == Direction.Right || direction == Direction.Left)
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
        return direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Down => Direction.Up,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            _ => direction,
        };
    }

    //function that checks if moving from a room with a certain direction would lead to another room
    private bool RoomExists(RoomInfo room, Direction direction)
    {
        if (!rooms.Exists(
            room.Coord.x + directionX[(int)direction],
            room.Coord.y + directionY[(int)direction]
            ))
            return false;

        return true;
    }

    //builds a certain number of rooms in one direction from a given room
    private int BuildRooms(RoomInfo room, Direction direction, int length)
    {
        int index = -1;
        for (int i = 0; i < length; i++)
        {
            if (RoomExists(room, direction))
            {
                int secondIndex = rooms.GetIndex(
                    room.Coord.x + directionX[(int)direction],
                    room.Coord.y + directionY[(int)direction]);

                room.AddDirection(direction);
                rooms.GetByIndex(secondIndex).AddDirection(OppositeDirection(direction));
                return -1;
            }
            else
            {
                int newIndex = AddRoom(
                    room.Coord.x + directionX[(int)direction],
                    room.Coord.y + directionY[(int)direction]
                    );

                room.AddDirection(direction);
                rooms.GetByIndex(newIndex).AddDirection(OppositeDirection(direction));

                index = newIndex;
                room = rooms.GetByIndex(index);
            }
        }

        return index;
    }

    private void BuildSpecialRooms(RoomInfo room, Direction direction, int length, List<GameObject> specialRooms, bool pushUntilPossible = false, bool movePlayer = false)
    {
        if (length > 0)
        {
            int index = BuildRooms(room, direction, length);
            room = rooms.GetByIndex(index);
        }

        foreach (GameObject obj in specialRooms)
        {
            int index = AddRoom(room.Coord.x + directionX[(int)direction], room.Coord.y + directionY[(int)direction]);
            int pas = 1;

            if (pushUntilPossible)
            {
                while (index == -1)
                {
                    RoomInfo intermRoom = rooms.GetByCoord(room.Coord.x + directionX[(int)direction] * pas, room.Coord.y + directionY[(int)direction] * pas);
                    intermRoom.AddDirection(direction);
                    intermRoom.AddDirection(OppositeDirection(direction));
                    pas++;
                    index = AddRoom(room.Coord.x + directionX[(int)direction] * pas, room.Coord.y + directionY[(int)direction] * pas);
                }
            }
            else
            {
                if (index == -1)
                    break;
            }

            room.AddDirection(direction);
            rooms.GetByIndex(index).AddDirection(OppositeDirection(direction));

            room = rooms.GetByIndex(index);
            room.OwnGround = obj;
        }

        if (movePlayer && playerIfUsingStartRooms != null)
            playerIfUsingStartRooms.transform.position = new(room.Coord.x * roomWidth, room.Coord.y * roomHeight, 0.0f);
    }

    //generates the rooms
    void RoomGeneration()
    {
        Tuple<RoomInfo, RoomInfo> startAndEndRooms = CreateMainLine();
        CreateLoops();
        CreateSpecialPaths();
        CreateSpecialRoomsOnMainLine(startAndEndRooms.Item1, startAndEndRooms.Item2);
        AddCollectibles();
        AddEnemies();
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
            RoomInfo activeRoom = rooms.GetByIndex(activeIndex);
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
                if (FloatMath.Larger(chosenProb, chanceToGoStraight))
                {
                    chosenProb -= chanceToGoStraight;
                }
                else
                {
                    changedDir = true;
                }
            }

            if (!changedDir && !RoomExists(activeRoom, neighbourDir.Item1) && neighbourDir.Item1 != Direction.Left && FloatMath.SmallerOrEqual(chosenProb, probChangeDir))
            {
                activeDirection = neighbourDir.Item1;
                changedDir = true;
            }

            if (!changedDir)
            {
                activeDirection = neighbourDir.Item2;
            }

            //add the room and the doors
            int newIndex = AddRoom(
                activeRoom.Coord.x + directionX[(int)activeDirection],
                activeRoom.Coord.y + directionY[(int)activeDirection]);
            rooms.GetByIndex(newIndex).AddDirection(OppositeDirection(activeDirection));
            rooms.GetByIndex(activeIndex).AddDirection(activeDirection);
            activeIndex = newIndex;
        }

        //returns the first and the last room
        return new Tuple<RoomInfo, RoomInfo>(rooms.GetByIndex(0), rooms.GetByIndex(rooms.Count() - 1));
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
            RoomInfo roomStart = rooms.GetByIndex(firstPoint);
            RoomInfo roomEnd = rooms.GetByIndex(secondPoint);
            Direction firstDir, secondDir, thirdDir = Direction.Left;
            int firstSteps, secondSteps, thirdSteps;

            if (roomStart.Coord.x == roomEnd.Coord.x)
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


                if (roomStart.Coord.y < roomEnd.Coord.y)
                {
                    secondDir = Direction.Up;
                }
                else
                {
                    secondDir = Direction.Down;
                }

                thirdDir = OppositeDirection(firstDir);
                secondSteps = Math.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                /*
                 * not making a loop stupid, it makes sure that the number of rooms that put a distance between
                 * the main corridor and the main line is not too high compared to the length of the corridor
                 */
                firstSteps = thirdSteps = 1 + UnityEngine.Random.Range(
                    0,
                    Math.Min(secondSteps - 1, Convert.ToInt32((maximumLoopAddedPoints - secondSteps) / 2) + 1)
                    );
            }
            else if (roomStart.Coord.y == roomEnd.Coord.y)
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
                secondSteps = Math.Abs(roomStart.Coord.x - roomEnd.Coord.x);
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
                if (roomStart.Coord.y < roomEnd.Coord.y)
                {
                    if (probability == 0 && !RoomExists(roomStart, Direction.Up))
                    {
                        firstDir = Direction.Up;
                        firstSteps = Math.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.Coord.x - roomEnd.Coord.x);
                    }
                    else if (!RoomExists(roomStart, Direction.Right))
                    {
                        firstDir = Direction.Right;
                        firstSteps = Mathf.Abs(roomStart.Coord.x - roomEnd.Coord.x);
                        secondDir = Direction.Up;
                        secondSteps = Mathf.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                    }
                    else
                    {
                        firstDir = Direction.Up;
                        firstSteps = Math.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.Coord.x - roomEnd.Coord.x);
                    }
                }
                else
                {
                    if (probability == 0 && !RoomExists(roomStart, Direction.Down))
                    {
                        firstDir = Direction.Down;
                        firstSteps = Math.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.Coord.x - roomEnd.Coord.x);
                    }
                    else if (!RoomExists(roomStart, Direction.Right))
                    {
                        firstDir = Direction.Right;
                        firstSteps = Mathf.Abs(roomStart.Coord.x - roomEnd.Coord.x);
                        secondDir = Direction.Down;
                        secondSteps = Mathf.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                    }
                    else
                    {
                        firstDir = Direction.Down;
                        firstSteps = Math.Abs(roomStart.Coord.y - roomEnd.Coord.y);
                        secondDir = Direction.Right;
                        secondSteps = Mathf.Abs(roomStart.Coord.x - roomEnd.Coord.x);
                    }
                }

                thirdSteps = 0;
            }

            //building the path
            int activeIndex = BuildRooms(roomStart, firstDir, firstSteps);
            if (activeIndex != -1)
                activeIndex = BuildRooms(rooms.GetByIndex(activeIndex), secondDir, secondSteps);
            if (activeIndex != -1 && thirdSteps != 0)
                BuildRooms(rooms.GetByIndex(activeIndex), thirdDir, thirdSteps);
        }
    }
    void CreateSpecialPaths()
    {
        if (specialPaths.Count == 0)
            return;

        Interval<int> intervalX = rooms.GetXInterval();

        //building the paths
        foreach (SpecialPath specialPath in specialPaths)
        {
            //randomly choosing if it spawns or not
            if (UnityEngine.Random.Range(0f, 1f) <= specialPath.chanceToSpawn)
            {
                int numberOfRooms = UnityEngine.Random.Range(
                    Convert.ToInt32(specialPath.normalRooms.From),
                    Convert.ToInt32(specialPath.normalRooms.To) + 1);

                int x = UnityEngine.Random.Range(intervalX.Min, intervalX.Max + 1);
                float probDir = UnityEngine.Random.Range(0, 2);
                Direction dir;
                if (probDir == 0)
                    dir = Direction.Up;
                else
                    dir = Direction.Down;

                //choosing the room in a manner that assures that the new path doesn't collide with other rooms
                RoomInfo room;
                Interval<int> intervalY = rooms.GetYIntervalForCertainX(x);
                if (dir == Direction.Up)
                    room = rooms.GetByCoord(x, intervalY.Max);
                else
                    room = rooms.GetByCoord(x, intervalY.Min);

                //building the path
                BuildSpecialRooms(room, dir, numberOfRooms, specialPath.specialRooms);
            }
        }
    }
    void CreateSpecialRoomsOnMainLine(RoomInfo startRoom, RoomInfo endRoom)
    {
        if (specialStartRooms.Count > 0)
        {
            specialStartRooms.Reverse();
            BuildSpecialRooms(startRoom, Direction.Left, 0, specialStartRooms, true, true);
        }

        if (specialEndRooms.Count > 0)
            BuildSpecialRooms(endRoom, Direction.Right, 0, specialEndRooms, true);
    }
    private List<int> GetBasicRoomsIndexes()
    {
        List<int> basicRoomsIndexes = new();

        //gets all non-special rooms indexes
        for (int i = 0; i < rooms.Count(); i++)
        {
            if (rooms.GetByIndex(i).OwnGround == null)
                basicRoomsIndexes.Add(i);
        }

        return basicRoomsIndexes;
    }
    void AddCollectibles()
    {
        List<int> basicRoomsIndexes = GetBasicRoomsIndexes();

        foreach (var collectibleInfo in collectibles)
        {
            //spawns the collectibles
            for (int i = 0; i < collectibleInfo.maximumNumber; i++)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > collectibleInfo.chanceOfSpawn)
                    continue;

                int index = UnityEngine.Random.Range(0, basicRoomsIndexes.Count);
                RoomInfo room = rooms.GetByIndex(basicRoomsIndexes[index]);
                //it picks a basic room

                int pickedX = UnityEngine.Random.Range(-maximumXForSpawn, maximumXForSpawn + 1);
                int pickedY = UnityEngine.Random.Range(-maximumYForSpawn, maximumYForSpawn + 1);
                if (pickedX == 0)
                    pickedX += UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                if (pickedY == 0)
                    pickedY += UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                //it makes no sense for either x or y to be in the center

                if (!room.Collectibles.Exists(pickedX, pickedY))
                {
                    Vector3 position = Vector3.zero;

                    if (pickedX > 0)
                        position.x = room.Coord.x * roomWidth + pickedX - approachToCenter;
                    else
                        position.x = room.Coord.x * roomWidth + pickedX + approachToCenter;

                    if (pickedY > 0)
                        position.y = room.Coord.y * roomHeight + pickedY - approachToCenter;
                    else
                        position.y = room.Coord.y * roomHeight + pickedY + approachToCenter;

                    GameObject collectibleInstance = Instantiate(collectibleInfo.gameObject, position, Quaternion.identity);
                    room.Collectibles.Add(new InsideRoomObject(collectibleInstance, pickedX, pickedY));
                }
            }
        }
    }

    private readonly int[] fullDirectionX = { -1, 0, 1, 1, 1, 0, -1, -1 };
    private readonly int[] fullDirectionY = { 1, 1, 1, 0, -1, -1, -1, 0 };

    private bool InRoomBounds(int x, int y)
    {
        if (x >= -maximumXForSpawn + 1 && x <= maximumXForSpawn - 1 && y >= -maximumYForSpawn + 1 && y <= maximumYForSpawn - 1)
            return true;
        return false;
    }

    void AddEnemies()
    {
        List<int> basicRoomsIndexes = GetBasicRoomsIndexes();
        int sumOfValues = enemies.Sum(value => value.valueOfEnemy);

        //spawning enemies in all basic rooms
        basicRoomsIndexes.ForEach(index =>
        {
            RoomInfo room = rooms.GetByIndex(index);
            //the number of enemies to spawn in a room
            int spawn = UnityEngine.Random.Range((int)enemiesInRoom.From, (int)enemiesInRoom.To + 1);

            for (int i = 0; i < spawn; i++)
            {
                int valueChosen = UnityEngine.Random.Range(1, sumOfValues + 1);
                int currentValue = 0;
                int indexOfEnemy = 0;

                //picking the enemy
                for (int j = 0; j < enemies.Count(); j++)
                {
                    if (j == enemies.Count() - 1 || currentValue + enemies[j].valueOfEnemy + enemies[j + 1].valueOfEnemy > valueChosen)
                    {
                        indexOfEnemy = j;
                        break;
                    }
                    else
                    {
                        currentValue += enemies[j].valueOfEnemy;
                    }
                }

                //picking the room tile
                int pickedX = UnityEngine.Random.Range(-maximumXForSpawn + 1, maximumXForSpawn);
                int pickedY = UnityEngine.Random.Range(-maximumYForSpawn + 1, maximumYForSpawn);
                if (pickedX == 0)
                    pickedX += UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                if (pickedY == 0)
                    pickedY += UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;

                int foundX = -1000;
                int foundY = -1000;

                //trying with the neighbouring tiles if the chosen tile is full
                if (!room.Enemies.Exists(pickedX, pickedY))
                {
                    foundX = pickedX;
                    foundY = pickedY;
                }
                else
                {
                    for (int d = 0; d < 9; d++)
                    {
                        if (InRoomBounds(pickedX + fullDirectionX[d], pickedY + fullDirectionY[d]) &&
                        !room.Enemies.Exists(pickedX + fullDirectionX[d], pickedY + fullDirectionY[d]))
                        {
                            foundX = pickedX + fullDirectionX[d];
                            foundY = pickedY + fullDirectionY[d];
                            break;
                        }
                    }
                }

                //spawn the enemy only if an empty tile was found
                if (foundX != -1000 && foundY != -1000)
                {
                    Vector3 position = Vector3.zero;

                    if (pickedX > 0)
                        position.x = room.Coord.x * roomWidth + pickedX - approachToCenter;
                    else
                        position.x = room.Coord.x * roomWidth + pickedX + approachToCenter;

                    if (pickedY > 0)
                        position.y = room.Coord.y * roomHeight + pickedY - approachToCenter;
                    else
                        position.y = room.Coord.y * roomHeight + pickedY + approachToCenter;

                    GameObject enemyInstance = Instantiate(enemies[indexOfEnemy].enemy, position, Quaternion.identity);
                    enemyInstance.GetComponent<UniversalSetTarget>().SetTarget(player);
                    room.Enemies.Add(new InsideRoomObject(enemyInstance, pickedX, pickedY));
                }
            }
        });
    }

    //using the informations in the room, adds the proper prefab
    private void Build(int position)
    {
        RoomInfo room = rooms.GetByIndex(position);
        Vector3 posInstantiate = new(room.Coord.x * roomWidth, room.Coord.y * roomHeight, 0.0f);
        if (!room.Left && !room.Up && !room.Right && !room.Down) //1
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

        if (room.OwnGround == null)
            Instantiate(grounds[UnityEngine.Random.Range(0, grounds.Length)], posInstantiate, Quaternion.identity);
        else
            Instantiate(room.OwnGround, posInstantiate, Quaternion.identity);
    }
}