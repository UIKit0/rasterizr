﻿using System;
using System.Collections.Generic;
using Rasterizr.Diagnostics;
using Rasterizr.Pipeline;
using Rasterizr.Pipeline.GeometryShader;
using Rasterizr.Pipeline.InputAssembler;
using Rasterizr.Pipeline.OutputMerger;
using Rasterizr.Pipeline.PixelShader;
using Rasterizr.Pipeline.Rasterizer;
using Rasterizr.Pipeline.VertexShader;
using Rasterizr.Resources;
using Rasterizr.Util;
using SlimShader.Chunks.Xsgn;

namespace Rasterizr
{
	public class DeviceContext : DeviceChild
	{
		private readonly Device _device;
		private readonly InputAssemblerStage _inputAssembler;
		private readonly VertexShaderStage _vertexShader;
		private readonly GeometryShaderStage _geometryShader;
		private readonly RasterizerStage _rasterizer;
		private readonly PixelShaderStage _pixelShader;
		private readonly OutputMergerStage _outputMerger;

		public InputAssemblerStage InputAssembler
		{
			get { return _inputAssembler; }
		}

		public VertexShaderStage VertexShader
		{
			get { return _vertexShader; }
		}

		public GeometryShaderStage GeometryShader
		{
			get { return _geometryShader; }
		}

		public RasterizerStage Rasterizer
		{
			get { return _rasterizer; }
		}

		public PixelShaderStage PixelShader
		{
			get { return _pixelShader; }
		}

		public OutputMergerStage OutputMerger
		{
			get { return _outputMerger; }
		}

		internal DeviceContext(Device device)
            : base(device)
		{
			_device = device;
			_inputAssembler = new InputAssemblerStage(device);
			_vertexShader = new VertexShaderStage(device);
			_geometryShader = new GeometryShaderStage(device);
			_rasterizer = new RasterizerStage(device);
			_pixelShader = new PixelShaderStage(device);
			_outputMerger = new OutputMergerStage(device);
		}

		public virtual void ClearDepthStencilView(DepthStencilView depthStencilView, DepthStencilClearFlags clearFlags, float depth, byte stencil)
		{
			_device.Loggers.BeginOperation(OperationType.DeviceContextClearDepthStencilView, depthStencilView, clearFlags, depth, stencil);
			depthStencilView.Clear(clearFlags, depth, stencil);
		}

        public virtual void ClearRenderTargetView(RenderTargetView renderTargetView, Color4 color)
		{
			_device.Loggers.BeginOperation(OperationType.DeviceContextClearRenderTargetView, renderTargetView, color);

            var number = color.ToNumber4();
			_device.Loggers.AddPixelHistoryEvent(new SimpleEvent(number));
			renderTargetView.Clear(ref number);
		}

		public void Draw(int vertexCount, int startVertexLocation)
		{
			_device.Loggers.BeginOperation(OperationType.DeviceContextDraw, vertexCount, startVertexLocation);
			DrawInternal(_inputAssembler.GetVertexStream(_vertexShader.Shader.Bytecode.InputSignature, vertexCount, startVertexLocation));
		}

		public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
		{
			_device.Loggers.BeginOperation(OperationType.DeviceContextDrawIndexed, indexCount, startIndexLocation, baseVertexLocation);
			DrawInternal(_inputAssembler.GetVertexStreamIndexed(_vertexShader.Shader.Bytecode.InputSignature, indexCount, startIndexLocation, baseVertexLocation));
		}

		public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
		{
			_device.Loggers.BeginOperation(OperationType.DeviceContextDrawInstanced, vertexCountPerInstance,
				instanceCount, startVertexLocation, startInstanceLocation);
			DrawInternal(_inputAssembler.GetVertexStreamInstanced(_vertexShader.Shader.Bytecode.InputSignature, vertexCountPerInstance,
				instanceCount, startVertexLocation, startInstanceLocation));
		}

		private void DrawInternal(IEnumerable<InputAssemblerVertexOutput> vertexStream)
		{
			var vertexShaderOutputs = _vertexShader.Execute(vertexStream);
			var primitiveStream = _inputAssembler.GetPrimitiveStream(vertexShaderOutputs);

			IEnumerable<InputAssemblerPrimitiveOutput> rasterizerInputs;
			PrimitiveTopology rasterizerInputTopology;
			OutputSignatureChunk rasterizerInputSignature;
			if (_geometryShader.IsActive)
			{
				rasterizerInputs = _geometryShader.Execute(primitiveStream, _inputAssembler.PrimitiveTopology);
				rasterizerInputTopology = _geometryShader.OutputTopology;
				rasterizerInputSignature = _geometryShader.Shader.Bytecode.OutputSignature;
			}
			else
			{
				rasterizerInputs = primitiveStream;
				rasterizerInputTopology = _inputAssembler.PrimitiveTopology;
				rasterizerInputSignature = _vertexShader.Shader.Bytecode.OutputSignature;
			}

			var rasterizerOutputs = _rasterizer.Execute(rasterizerInputs, rasterizerInputTopology,
				rasterizerInputSignature, _pixelShader.Shader.Bytecode,
				_outputMerger.MultiSampleCount);

			var pixelShaderOutputs = _pixelShader.Execute(rasterizerOutputs);
			_outputMerger.Execute(pixelShaderOutputs);
		}

		public void GenerateMips(ShaderResourceView shaderResourceView)
		{
            GenerateMips((TextureBase) shaderResourceView.Resource);
		}

	    public void GenerateMips(TextureBase texture)
	    {
            Device.Loggers.BeginOperation(OperationType.GenerateMips, texture);
            texture.GenerateMips();
	    }
	}
}