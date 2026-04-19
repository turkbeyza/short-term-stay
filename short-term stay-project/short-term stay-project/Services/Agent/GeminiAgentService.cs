using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace short_term_stay_project.Services.Agent;

public interface IGeminiAgentService
{
    Task<string> ChatAsync(string userMessage, string sessionId);
}

public class GeminiAgentService : IGeminiAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ILogger<GeminiAgentService> _logger;

    private static readonly Dictionary<string, ChatHistory> _sessionHistories = new();

    private static readonly string SystemPrompt =
        "## PERSONA\n" +
        "You are the 'Stay AI Agent', a concierge for a short-term rental platform. " +
        "Your job is to help users search for stays, make bookings, and submit reviews.\n\n" +
        "## CRITICAL RULES — FOLLOW EXACTLY\n" +
        "1. **Always call the tool first**: When a user asks for listings or suggestions, you MUST call `query_listings` immediately. NEVER say there are no results without calling the tool first.\n" +
        "2. **Trust the tool response**: The tool returns real JSON data from the database. Present everything it returns. Only say nothing is available if the tool explicitly returns 'No listings found matching the given criteria'.\n" +
        "3. **Do not hallucinate**: Never invent listings, prices, or availability. Only present what the tool returns.\n" +
        "4. **Date conversion**: ALWAYS convert user dates (e.g. '20.07.2027', 'next Friday') to yyyy-MM-dd before tool calls. Never ask the user to reformat a date.\n" +
        "5. **Proactive search**: If city/country is missing, call query_listings with only noOfPeople set to show all available options.\n\n" +
        "## OUTPUT FORMAT\n" +
        "- Respond in Markdown.\n" +
        "- When the tool returns listings, you MUST include ALL of them in this exact JSON block at the end of your response:\n" +
        "[LISTINGS_JSON][{\"id\":1,\"city\":\"City\",\"country\":\"Country\",\"price\":100,\"noOfPeople\":2,\"rating\":4.5}][/LISTINGS_JSON]\n" +
        "- Map the tool JSON fields: id->id, city->city, country->country, price->price, noOfPeople->noOfPeople, averageRating->rating.\n" +
        "- Include EVERY listing from the tool response in the JSON block, do not truncate.\n" +
        "- Only omit the JSON block if the tool explicitly returned no results.";

    public GeminiAgentService(Kernel kernel, ILogger<GeminiAgentService> logger)
    {
        _kernel = kernel;
        _logger = logger;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> ChatAsync(string userMessage, string sessionId)
    {
        try
        {
            if (!_sessionHistories.TryGetValue(sessionId, out var history))
            {
                history = new ChatHistory(SystemPrompt);
                _sessionHistories[sessionId] = history;
            }

            history.AddUserMessage(userMessage);

            var executionSettings = new GeminiPromptExecutionSettings
            {
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
            };

            // Agentic loop: keep invoking until the model produces a final text response
            // (no more pending tool calls). Gemini SK connector handles tool dispatch
            // internally per iteration, but we loop to ensure multi-step tool chains work.
            const int maxIterations = 6;
            ChatMessageContent? finalResponse = null;

            for (int i = 0; i < maxIterations; i++)
            {
                var response = await _chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings,
                    _kernel);

                _logger.LogInformation(
                    "[AgentLoop iter={Iter}] Role={Role} Content={Content}",
                    i, response.Role, response.Content?.Substring(0, Math.Min(200, response.Content?.Length ?? 0)));

                history.Add(response);

                // If the model produced actual text content it's done
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    finalResponse = response;
                    break;
                }

                // Otherwise the SK connector already executed the tool calls and appended
                // tool results to history — loop again so the model sees them.
            }

            var content = finalResponse?.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Agent loop exhausted {Max} iterations without a text response.", maxIterations);
                content = "I was unable to complete the request after multiple attempts. Please try again.";
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GeminiAgentService.ChatAsync");
            return $"I encountered an issue processing your request. Details: {ex.Message}";
        }
    }
}
