package main

import (
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/kartikgujjar22/tokenvault/internal/api"
	"github.com/kartikgujjar22/tokenvault/internal/database"
	"github.com/spf13/cobra"
)

func main() {
	var rootCmd = &cobra.Command{
		Use:   "tokenvault",
		Short: "A developer tool for token management",
		Run: func(cmd *cobra.Command, args []string) {
			fmt.Println("TokenVault CLI installed.")
			fmt.Println("Run 'tokenvault serve' to start the server.")
		},
	}

	var serveCmd = &cobra.Command{
		Use:   "serve",
		Short: "Start the local server",
		Run: func(cmd *cobra.Command, args []string) {
			fmt.Println("Initializing Database for Server...")
			database.InitDB()

			r := gin.Default()

			r.GET("/ping", func(c *gin.Context) {
				c.JSON(http.StatusOK, gin.H{
					"message": "pong",
					"status":  "TokenVault Server is Healthy",
				})
			})

			r.POST("/store", api.StoreTokenHandler)
			r.GET("/fetch/:project", api.GetTokenHandler)

			fmt.Println("Starting server on http://localhost:9999...")
			r.Run(":9999")
		},
	}

	rootCmd.AddCommand(serveCmd)

	rootCmd.AddCommand(SetupCmd)

	rootCmd.Execute()
}
