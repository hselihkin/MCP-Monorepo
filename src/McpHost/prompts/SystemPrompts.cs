using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McpHost.prompts
{
    internal class SystemPrompts
    {
        public string serverPrompt { get; set; }

        public string toolPrompt { get; set; }

        public string serverDetails { get; set; }

        public SystemPrompts() {
            this.toolPrompt = """
                Select the most appropiate tool which can be used to solve the users query.
                """;
        }

        public void addServerDetails(string details, string token)
        {
            serverDetails = details;
            this.serverPrompt = $"""
                ---DETAILS OF AVAILABLE SERVERS---
                {serverDetails}
                ----------------------------------

                ---JWT AUTHENTICATION TOKEN-------
                {token}
                ----------------------------------

                Your role is to give endpoints of the server that matches the users requirements.
                based on servers metadata and the users query, respond with only the endpoint of the selected server and nothing else.
                """;
        }

        public void addTokenDetails(string token) { 
            this.toolPrompt = $"""
                ---JWT AUTHENTICATION TOKEN-------
                {token}
                ----------------------------------

                Select the most appropiate tool which can be used to solve the users query.
                """;
        }
    }
}
