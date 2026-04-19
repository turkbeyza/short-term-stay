Create an AI Agent chat application for the Query Listing, Book a Listing and Review a Listing APIs you created in the project. 


EXPECTED ARCHITECTURE

For chat application, use the empty next.js project I already created.


- Make sure all your API calls go through the gateway
- Consider using Firestore for Real Time Messaging

- Call a gemini API to Call LLM API 
- Develop an MCP server for your APIs
- LLM decides which MCP tool to call.
- MCP server maps that tool to the correct gateway endpoint.
- Gateway routes to your midterm API.
- Call Midterm APIs per message text. Assume your chat application uses constant
userid/password for authentication when needed. You can add more APIs if you wish
- Refresh chat API per API responses

- I think we can put the AI agent backend to the already existing api project. I think it would make everything easier. 

Simple implementation idea
Components
- Frontend: next.js chat UI (you can use chatbot template from next.js)
- Agent backend: .net
- LLM: gemini api
- MCP server: you can decide (I prefer .net)
- Gateway: your existing API gateway
- Midterm APIs: listing services (already implemented)

Minimal backend logic
- receive chat message
- send message + conversation history to LLM
- allow LLM to call MCP tools
- tool calls hit gateway endpoints
- return response to frontend 