using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Foundation;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;
using Visualizer.Rendering.Car.SceneGraph;

namespace Visualizer.Rendering.Car
{
    public class CarRendererFactory
    {
        public const int MaxInflightBuffers = 1;
        
        public static IMTLRenderPipelineState CreateRenderPipeline(IMTLDevice device, IMTLLibrary library, MTKView view, MDLVertexDescriptor vertexDescriptor)
        {
            var mtlVertexDescriptor = MTLVertexDescriptor.FromModelIO(vertexDescriptor);
            var vertexFunction = library.CreateFunction("vertex_main");
            var fragmentFunction = library.CreateFunction("fragment_main");
            var pipelineDescriptor = new MTLRenderPipelineDescriptor
            {
                Label = "RenderPipeline",
                SampleCount = view.SampleCount,
                VertexFunction = vertexFunction,
                FragmentFunction = fragmentFunction,
                DepthAttachmentPixelFormat = view.DepthStencilPixelFormat,
                StencilAttachmentPixelFormat = view.DepthStencilPixelFormat,
                VertexDescriptor = mtlVertexDescriptor
            };
            pipelineDescriptor.ColorAttachments[0].PixelFormat = view.ColorPixelFormat;

            var state = device.CreateRenderPipelineState(pipelineDescriptor, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }

            return state;
        }

        public static IMTLSamplerState CreateSamplerState(IMTLDevice device)
        {
            var samplerDescriptor = new MTLSamplerDescriptor
            {
                NormalizedCoordinates = true,
                MinFilter = MTLSamplerMinMagFilter.Linear,
                MagFilter = MTLSamplerMinMagFilter.Linear,
                MipFilter = MTLSamplerMipFilter.Linear
            };
            return device.CreateSamplerState(samplerDescriptor);
        }

        public static IMTLDepthStencilState CreateDepthStencilState(IMTLDevice device)
        {
            var depthStateDesc = new MTLDepthStencilDescriptor
            {
                DepthCompareFunction = MTLCompareFunction.Less,
                DepthWriteEnabled = true
            };

            return device.CreateDepthStencilState(depthStateDesc);
        }

        public static MDLVertexDescriptor CreateVertexDescriptor()
        {
            var vertextDescriptor = new MDLVertexDescriptor();
            vertextDescriptor.Layouts[0] = new MDLVertexBufferLayout((nuint) (Marshal.SizeOf<float>() * 6));
            vertextDescriptor.Attributes[0] = new MDLVertexAttribute(MDLVertexAttributes.Position.ToString(), MDLVertexFormat.Float3, 0, 0);
            vertextDescriptor.Attributes[1] = new MDLVertexAttribute(MDLVertexAttributes.Normal.ToString(), MDLVertexFormat.Float3, (nuint) (Marshal.SizeOf<float>() * 3), 0);
            //vertextDescriptor.Attributes[2] = new MDLVertexAttribute(MDLVertexAttributes.TextureCoordinate.ToString(), MDLVertexFormat.Float2, (nuint) (Marshal.SizeOf<float>() * 6), 0);
            return vertextDescriptor;
        }

        public static Scene BuildScene(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor)
        {
            var scene = new Scene
            {
                AmbientLightColor = new Vector3(.1f, .1f, .1f),
                Lights = new List<Light>
                {
                    new Light {WorldPosition = new Vector3(5, 5, 0), Color = new Vector3(.3f, .3f, .3f)},
                    new Light {WorldPosition = new Vector3(-5, 5, 0), Color = new Vector3(.3f, .3f, .3f)},
                    new Light {WorldPosition = new Vector3(0, -5, 0), Color = new Vector3(.3f, .3f, .3f)}
                }
            };

            var car = new Node("car")
            {
                Material = new Material {SpecularPower = 100f, SpecularColor = new Vector3(.8f, .8f, .8f)},
                VertexUniformsBuffer = library.Device.CreateBuffer((nuint) Marshal.SizeOf<VertexUniforms>() * MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault),
                FragmentUniformsBuffer = library.Device.CreateBuffer((nuint) Marshal.SizeOf<FragmentUniforms>() * MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault),
                Mesh = CreateTeapot(library, vertexDescriptor)// CreateBox(library),
            };
            car.VertexUniformsBuffer.Label = "Car VertexUniformsBuffer";
            car.FragmentUniformsBuffer.Label = "Car FragmentUniformsBuffer";
            Console.WriteLine($"VertexUniformsBuffer.length = {car.VertexUniformsBuffer.Length}");
            Console.WriteLine($"FragmentUniformsBuffer.length = {car.FragmentUniformsBuffer.Length}");
            scene.RootNode.Children.Add(car);

            return scene;
        }

        public static MTKMesh CreateTeapot(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor)
        {
            var bufferAllocator = new MTKMeshBufferAllocator(library.Device);
            var carAsset = new MDLAsset(NSUrl.FromFilename("teapot.obj"), vertexDescriptor, bufferAllocator);
            var mesh = MTKMesh.FromAsset(carAsset, library.Device, out _, out var error).First();
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh;
        }

        public static MTKMesh CreateBox(IMTLLibrary library)
        {
            using (var bufferAllocator = new MTKMeshBufferAllocator(library.Device))
            {
                MDLMesh mdl = MDLMesh.CreateBox(new Vector3(2f, 2f, 2f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, bufferAllocator);
                var boxMesh = new MTKMesh(mdl, library.Device, out var error);
                if (error != null)
                {
                    throw new NSErrorException(error);
                }
                return boxMesh;
            }
        }
    }
}