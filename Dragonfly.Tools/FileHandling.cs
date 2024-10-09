using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public static class FileHandling
    {
        public static List<PositionAnalysisNode> GetPositionAnalysisNodesFromEvaluationFolder(string analysisFolder)
        {
            var analysisFile = Path.Combine(analysisFolder, "_evaluation_positions.ep.json"); // TODO: make a constant somewhere

            return GetPositionAnalysisNodesFromFile(analysisFile);
        }

        public static List<PositionAnalysisNode> GetPositionAnalysisNodesFromFile(string filename)
        {
            // Open analysis file, slurp json
            List<PositionAnalysisNode> ret;
            using (var filestream = new FileStream(filename, FileMode.Open))
            {
                ret = JsonSerializer.Deserialize<List<PositionAnalysisNode>>(filestream);
            }
            return ret;
        }

        public static List<EvaluationNode> GetEvaluationNodesFromFile(string filename)
        {
            // Open evaluation file, slurp json
            List<EvaluationNode> ret;
            using (var filestream = new FileStream(filename, FileMode.Open))
            {
                ret = JsonSerializer.Deserialize<List<EvaluationNode>>(filestream);
            }
            return ret;
        }
    }
}
