package main

import (
	"bufio"
	"fmt"
	"os"
	"strings"

	"github.com/spf13/cobra"
)

const scriptPart1 = `
	const projectMap = {
`

const scriptPart2 = `};

var currentPort = pm.request.url.port;
var currentHost = pm.request.url.getHost();

var project = projectMap[currentPort];

if (!project){
	project = projectMap[currentHost];
}

if (!project){
	console.log("TokenVault: No project mapped for port" + currentPort + ". Skipping auti-fetch");
}else{
	console.log("TokenVault: Detected Port " + currentPort + " -> Fetching token for ' " + project + "'");

	pm.sendRequest({
		url: 'http://localhost:9999/fetch/' + project,
		method: 'GET'
	}, function (err, res) {
			if (!err && res.code === 200){
				var data = res.json();
				pm.environment.set("token", data.token);
				console.log("TokenVault: Token updated for "+ project);
			}else{
				console.log("Tokenvault: Token not found for " + project);
			}
		}
	)
}
`

var SetupCmd = &cobra.Command{
	Use:   "setup-postman",
	Short: "Generates the automation script for Postman with interactive config",
	Run: func(cmd *cobra.Command, args []string) {
		reader := bufio.NewReader(os.Stdin)
		var configLines []string

		fmt.Println("Lets configure your Postman Automation Script")
		fmt.Println("Enter up to 3 Port-to-Project mappings.")
		fmt.Println(" (Press Enter without typing to finish early)")
		fmt.Println("---------------------------------------------")

		for i := 1; i <= 3; i++ {
			fmt.Printf("Mapping #%d - Enter Port (e.g. 5000): ", i)
			port, _ := reader.ReadString('\n')
			port = strings.TrimSpace(port)

			if port == "" {
				break
			}

			fmt.Printf("Mapping #%d - Enter Project Name (e.g. facebook): ", i)
			project, _ := reader.ReadString('\n')
			project = strings.TrimSpace(project)

			if project == "" {
				fmt.Println("Project name cannot be empty. Skipping this entry.")
				continue
			}

			line := fmt.Sprintf(`    "%s": "%s",`, port, project)
			configLines = append(configLines, line)
			fmt.Println("    Saved")
		}

		if len(configLines) == 0 {
			configLines = append(configLines, `    "5000": "example-project", //Default Example`)
		}

		finialScript := scriptPart1 + strings.Join(configLines, "\n") + "\n" + scriptPart2

		fileName := "tokenvault_script.js"
		err := os.WriteFile(fileName, []byte(finialScript), 0644)
		if err != nil {
			fmt.Println("Error creating script:", err)
		}

		fmt.Println("---------------------------------------------------")
		fmt.Printf("Success! Generated '%s' with your settings.\n", fileName)
		fmt.Println("Copy the content of this file into your Postman Collection -> Pre-request Script tab.")

	},
}
