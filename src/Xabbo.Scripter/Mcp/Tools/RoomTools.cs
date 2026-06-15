using System.Collections.Generic;
using System.Linq;

using Xabbo.Core;
using Xabbo.Core.Game;

using Xabbo.Scripter.Services;

namespace Xabbo.Scripter.Mcp.Tools;

public sealed class RoomTools : IMcpToolProvider
{
    private readonly IScriptHost _host;

    public RoomTools(IScriptHost host)
    {
        _host = host;
    }

    private RoomManager Rooms => _host.GameManager.RoomManager;
    private ProfileManager Profile => _host.GameManager.ProfileManager;

    [McpTool("get_connection", "Get the scripter's connection state: whether it is connected and ready, the client type, version and hotel, and the current room id.")]
    public object GetConnection()
    {
        return new
        {
            connected = _host.CanExecute,
            client = _host.Extension.Client.ToString(),
            clientIdentifier = _host.Extension.ClientIdentifier,
            clientVersion = _host.Extension.ClientVersion,
            hotel = _host.Extension.Hotel.ToString(),
            inRoom = Rooms.IsInRoom,
            roomId = Rooms.CurrentRoomId,
            inQueue = Rooms.IsInQueue,
            ringingDoorbell = Rooms.IsRingingDoorbell
        };
    }

    [McpTool("get_room", "Take a snapshot of the current room for analysis: room metadata, the player (self), all users, pets and (capped) floor and wall furniture.")]
    public object GetRoom(
        [McpParameter("Maximum number of floor items and wall items to include each. Defaults to 200.")] int maxFurni = 200)
    {
        IRoom? room = Rooms.Room;
        if (room is null)
            return new { inRoom = false };

        List<IRoomUser> users = room.Users.ToList();
        List<IPet> pets = room.Pets.ToList();
        List<IFloorItem> floorItems = room.FloorItems.ToList();
        List<IWallItem> wallItems = room.WallItems.ToList();

        long? selfId = Profile.UserData?.Id;
        IRoomUser? self = selfId.HasValue ? users.FirstOrDefault(u => u.Id == selfId.Value) : null;

        IRoomData? data = room.Data;

        return new
        {
            inRoom = true,
            room = new
            {
                id = room.Id,
                name = data?.Name,
                ownerId = data?.OwnerId,
                ownerName = data?.OwnerName,
                access = data?.Access.ToString(),
                description = data?.Description,
                maxUsers = data?.MaxUsers,
                tags = data?.Tags,
                category = data?.Category.ToString(),
                isGroupRoom = data?.IsGroupRoom,
                groupName = data?.GroupName,
                allowPets = data?.AllowPets,
                model = room.Model,
                floor = room.Floor,
                wallpaper = room.Wallpaper,
                landscape = room.Landscape,
                door = new { x = room.DoorTile.X, y = room.DoorTile.Y, z = room.DoorTile.Z }
            },
            rights = new
            {
                isOwner = Rooms.IsOwner,
                hasRights = Rooms.HasRights,
                rightsLevel = Rooms.RightsLevel,
                canMute = Rooms.CanMute,
                canKick = Rooms.CanKick,
                canBan = Rooms.CanBan
            },
            self = self is null ? null : User(self),
            userCount = users.Count,
            users = users.Select(User).ToList(),
            petCount = pets.Count,
            pets = pets.Select(Pet).ToList(),
            floorItemCount = floorItems.Count,
            floorItems = floorItems.Take(maxFurni).Select(FloorItem).ToList(),
            wallItemCount = wallItems.Count,
            wallItems = wallItems.Take(maxFurni).Select(WallItem).ToList()
        };
    }

    [McpTool("get_self", "Get the player's own avatar in the current room (position, figure, state).")]
    public object GetSelf()
    {
        IRoom? room = Rooms.Room;
        long? selfId = Profile.UserData?.Id;

        if (room is null || selfId is null)
            throw new McpToolException("Not currently in a room.");

        IRoomUser? self = room.Users.FirstOrDefault(u => u.Id == selfId.Value);
        if (self is null)
            throw new McpToolException("Own avatar not found in the room.");

        return User(self);
    }

    [McpTool("get_profile", "Get the player's own account profile: identity, currencies and achievement score.")]
    public object GetProfile()
    {
        IUserData? data = Profile.UserData;

        return new
        {
            userData = data is null ? null : new
            {
                id = data.Id,
                name = data.Name,
                figure = data.Figure,
                gender = data.Gender.ToString(),
                motto = data.Motto
            },
            credits = Profile.Credits,
            diamonds = Profile.Diamonds,
            duckets = Profile.Duckets,
            achievementScore = Profile.AchievementScore,
            homeRoom = Profile.HomeRoom
        };
    }

    [McpTool("get_inventory", "Get a summary of the player's furni inventory (whether it is loaded and how many items it holds).")]
    public object GetInventory()
    {
        IInventory? inventory = _host.GameManager.InventoryManager.Inventory;

        if (inventory is null)
            return new { loaded = false, count = 0 };

        return new { loaded = true, count = inventory.Count() };
    }

    private static object User(IRoomUser u) => new
    {
        id = u.Id,
        index = u.Index,
        name = u.Name,
        motto = u.Motto,
        figure = u.Figure,
        gender = u.Gender.ToString(),
        position = new { x = u.X, y = u.Y, z = u.Z },
        direction = u.Direction,
        isIdle = u.IsIdle,
        isTyping = u.IsTyping,
        dance = u.Dance,
        handItem = u.HandItem,
        effect = u.Effect,
        achievementScore = u.AchievementScore,
        isModerator = u.IsModerator,
        hasRights = u.HasRights
    };

    private static object Pet(IPet p) => new
    {
        id = p.Id,
        index = p.Index,
        name = p.Name,
        figure = p.Figure,
        position = new { x = p.X, y = p.Y, z = p.Z },
        direction = p.Direction
    };

    private static object FloorItem(IFloorItem f) => new
    {
        id = f.Id,
        kind = f.Kind,
        type = f.Type.ToString(),
        ownerId = f.OwnerId,
        ownerName = f.OwnerName,
        position = new { x = f.X, y = f.Y, z = f.Z },
        direction = f.Direction,
        height = f.Height,
        state = f.State,
        extra = f.Extra
    };

    private static object WallItem(IWallItem w) => new
    {
        id = w.Id,
        kind = w.Kind,
        type = w.Type.ToString(),
        ownerId = w.OwnerId,
        ownerName = w.OwnerName,
        location = new { wx = w.WX, wy = w.WY, lx = w.LX, ly = w.LY, orientation = w.Orientation.ToString() },
        state = w.State,
        data = w.Data
    };
}
