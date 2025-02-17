using System;
using AltV.Net.Elements.Entities;

namespace AltV.Net.Mock
{
    public class MockBlip : MockWorldObject, IBlip
    {
        public MockBlip(IServer server, IntPtr nativePointer) : base(server, nativePointer, BaseObjectType.Blip)
        {
        }

        public bool IsGlobal { get; }
        public bool IsAttached { get; set; }
        public IEntity AttachedTo { get; set; }
        public byte BlipType { get; set; }
        public ushort Sprite { get; set; }
        public byte Color { get; set; }
        public bool Route { get; set; }
        public byte RouteColor { get; set; }

        public void Remove()
        {
            Alt.Server.RemoveBlip(this);
        }
    }
}