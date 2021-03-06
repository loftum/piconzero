using System.Collections.Generic;
using Maths;

namespace Lego.Core.Description
{
    // Orientation: Nose pointing -X
    //      ____
    //  ---/    \
    //  <--0---0--
    //
    public class LegoCarDescriptor
    {
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public List<DistanceSensorDescriptor> DistanceSensors { get; set; } = new List<DistanceSensorDescriptor>();

        public static LegoCarDescriptor RaceCar() => new LegoCarDescriptor
        {
            Length = 42,
            Width = 20,
            Height = 13,
            DistanceSensors = new List<DistanceSensorDescriptor>
            {
                // Front left
                new DistanceSensorDescriptor
                {
                    ModelMatrix = Float4x4.CreateTranslation(-21f, 0, 0) *
                                  Float4x4.CreateTranslation(0, 0, 7f) *
                                  Float4x4.CreateRotation(Float.PI / 6, 0, 1f, 0)
                },
                // Front center
                new DistanceSensorDescriptor
                {
                    ModelMatrix = Float4x4.CreateTranslation(-21f, 0, 0)
                },
                // Front right
                new DistanceSensorDescriptor
                {
                    ModelMatrix = Float4x4.CreateTranslation(-21f, 0, 0) *
                                  Float4x4.CreateTranslation(0, 0, -7f) *
                                  Float4x4.CreateRotation(-Float.PI / 6, 0, 1f, 0)
                },
                // Rear right
                new DistanceSensorDescriptor
                {
                    ModelMatrix = Float4x4.CreateRotation(Float.PI, 0, 1f, 0) *
                                  Float4x4.CreateTranslation(-21f, 0, 0) *
                                  Float4x4.CreateTranslation(0, 0, 7f) *
                                  Float4x4.CreateRotation(Float.PI / 6, 0, 1f, 0)
                },
                // Rear center
                new DistanceSensorDescriptor
                {
                    ModelMatrix = Float4x4.CreateRotation(Float.PI, 0, 1f, 0) *
                                  Float4x4.CreateTranslation(-21f, 0, 0)
                },
                // Rear left
                new DistanceSensorDescriptor
                {
                    ModelMatrix = Float4x4.CreateRotation(Float.PI, 0, 1f, 0) *
                                  Float4x4.CreateTranslation(-21f, 0, 0) *
                                  Float4x4.CreateTranslation(0, 0, -7f) *
                                  Float4x4.CreateRotation(-Float.PI / 6, 0, 1f, 0)
                }
            }
        };
    }

    public class DistanceSensorDescriptor
    {
        public Float4x4 ModelMatrix { get; set; }
    }
}