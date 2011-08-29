using System.Collections.Concurrent;

namespace Rasterizr.PipelineStages.ShaderStages.VertexShader
{
	/// <summary>
	/// Applies a vertex program to the input vertices. The vertex program
	/// minimally computes the clip-space coordinates of the vertex
	/// positions and returns these as outputs to be used by the clipper
	/// and rasterizer.
	/// </summary>
	public class VertexShaderStage : PipelineStageBase<object, IVertexShaderOutput>
	{
		public IVertexShader VertexShader { get; set; }

		public override void Run(BlockingCollection<object> inputs, BlockingCollection<IVertexShaderOutput> outputs)
		{
			foreach (object input in inputs.GetConsumingEnumerable())
			{
				// Apply vertex shader.
				IVertexShaderOutput vertexShaderOutput = VertexShader.Execute(input);
				outputs.Add(vertexShaderOutput);
			}
			outputs.CompleteAdding();
		}

		// TODO: Implement cache for recently shaded vertices.
	}
}