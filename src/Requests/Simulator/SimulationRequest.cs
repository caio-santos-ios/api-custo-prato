using api_custo_prato.src.Calculations;

namespace api_custo_prato.src.Requests.Simulator
{
    /// <summary>
    /// Sandbox do simulador: uma CÓPIA da receita com os valores alterados. Nunca é gravada no banco.
    /// </summary>
    public class SimulationRequest
    {
        /// <summary>Receita original (opcional) para comparação "Original × Simulado".</summary>
        public string RecipeId { get; set; } = string.Empty;

        public RecipeInput Recipe { get; set; } = new();

        public GoalInput? Goal { get; set; }
    }
}
