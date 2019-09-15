using System.Collections.Generic;
using GameFeel.Component;
using GameFeel.GameObject.Loot;
using Godot;
using GodotTools.Extension;
using GodotTools.Util;

namespace GameFeel.Singleton
{
    public class MetadataLoader : Node
    {
        public const string PRIMARY_CURRENCY_ID = "64d66021-351d-571b-8af4-97c503155558";

        public static Dictionary<string, Metadata> LootItemIdToMetadata = new Dictionary<string, Metadata>();
        public static Dictionary<string, Metadata> EntityIdToMetadata = new Dictionary<string, Metadata>();
        public static Dictionary<string, EquipmentMetadata> LootItemIdToEquipmentMetadata = new Dictionary<string, EquipmentMetadata>();

        public class Metadata
        {
            public string Id { get; private set; }
            public string DisplayName { get; private set; }
            public string ResourcePath { get; private set; }
            public Texture Icon { get; private set; }

            public Metadata(string id, string displayName, string resourcePath, Texture icon)
            {
                Id = id;
                DisplayName = displayName;
                ResourcePath = resourcePath;
                Icon = icon;
            }
        }

        public class EquipmentMetadata : Metadata
        {
            public int SlotIndex { get; private set; }

            public EquipmentMetadata(string id, string displayName, string resourcePath, Texture icon, int slotIdx) : base(id, displayName, resourcePath, icon)
            {
                SlotIndex = slotIdx;
            }
        }

        private delegate void FullPathLoader(string fullPath);

        public override void _Ready()
        {
            LoadMetadata();
        }

        private void LoadMetadata()
        {
            LoadScenesInDir("res://scenes/GameObject/Loot/", LoadItem);
            LoadScenesInDir("res://scenes/GameObject/", LoadEntity);
        }

        private void LoadScenesInDir(string dirPath, FullPathLoader fullPathLoader)
        {
            var dir = new Directory();
            var err = dir.Open(dirPath);
            if (err != Error.Ok)
            {
                Logger.Error("Could not load items code " + (int) err);
                return;
            }

            dir.ListDirBegin();

            while (true)
            {
                var path = dir.GetNext();
                if (string.IsNullOrEmpty(path))
                {
                    break;
                }

                if (path.EndsWith(".tscn"))
                {
                    fullPathLoader(dirPath + path);
                }
            }

            dir.ListDirEnd();
        }

        private void LoadItem(string fullPath)
        {
            var node = GD.Load<PackedScene>(fullPath).Instance();
            if (node is LootItem li && li.Id != null && li.Id.ToLower() != "null")
            {
                var info = new Metadata(li.Id, li.DisplayName, fullPath, li.Icon);
                LootItemIdToMetadata[li.Id] = info;
                if (li.EquipmentScene != null)
                {
                    var equipment = li.EquipmentScene.Instance();
                    if (equipment is Equipment e)
                    {
                        var equipmentInfo = new EquipmentMetadata(li.Id, li.DisplayName, equipment.Filename, li.Icon, e.SlotIndex);
                        LootItemIdToEquipmentMetadata[li.Id] = equipmentInfo;
                    }
                    else
                    {
                        Logger.Error("Tried to load equipment that is not equipment with item id" + li.Id);
                    }
                    equipment.QueueFree();
                }
            }
            node.QueueFree();
        }

        private void LoadEntity(string fullPath)
        {
            // TODO: use an entity data component to store data about an entity
            var node = GD.Load<PackedScene>(fullPath).Instance();
            var entityId = node.GetFirstNodeOfType<EntityDataComponent>()?.Id ?? string.Empty;
            if (!string.IsNullOrEmpty(entityId))
            {
                var info = new Metadata(entityId, node.GetName(), fullPath, null);
                EntityIdToMetadata[entityId] = info;
            }
            node.QueueFree();
        }
    }
}