using System;
using System.Text;
using System.Threading.Tasks;
using RazorLight;
using Toxon.Photography.Generation.Models;

namespace Toxon.Photography.Generation
{
    public class RazorFile : File
    {
        private readonly IRazorLightEngine _razor;
        private readonly string _templateName;
        private readonly object _model;

        public RazorFile(IRazorLightEngine razor, string templateName, object model, string outputName) : base(outputName, "text/html")
        {
            _razor = razor;
            _templateName = templateName;
            _model = model;
        }

        public override async Task<ReadOnlyMemory<byte>> GenerateAsync()
        {
            var renderedPage = await _razor.CompileRenderAsync(_templateName, _model);

            return Encoding.UTF8.GetBytes(renderedPage);
        }
    }
}
