package api

import (
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/kartikgujjar22/tokenvault/internal/database"
)

// TokenRequest defiens the the JSON body that we expect from POST /store endpoint
type TokenRequest struct {
	Project string `json:"project" binding:"required"`
	Token   string `json:"token" binding:"required"`
}

func StoreTokenHandler(c *gin.Context) {
	var req TokenRequest

	//here shouldBindJSON automatically checks of project and token exist in the reques or not
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// now we have to save it to db the project and the token
	err := database.SaveToken(req.Project, req.Token)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save token"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"status": "saved", "project": req.Project})
}

func GetTokenHandler(c *gin.Context) {
	projectName := c.Param("project")

	token, err := database.GetToken(projectName)
	if err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "token Not Found"})
		return
	}
	c.JSON(http.StatusOK, gin.H{"token": token})
}
