package database

import (
	"database/sql"
	"fmt"
	"log"
	"os"
	"path/filepath"

	_ "modernc.org/sqlite" // this is the pure go sqlite driver installed, here we are using _ because we dont use the library directly we jsut need to register itself with the database/sql
)

var DB *sql.DB // DB is a global variable to hold our connections

func InitDB() {
	var err error

	//now we want to find user home directry to store the DB in a satndard place

	homeDir, err := os.UserHomeDir()
	if err != nil {
		log.Fatal("Could not find user home directory:", err)
	}

	// now if we doun the home directory we need to create a hidden folder called .tokenvault

	dbFolder := filepath.Join(homeDir, ".tokenvault")
	err = os.MkdirAll(dbFolder, 0755) // here 0755 means read/write for me, and read only for others
	if err != nil {
		log.Fatal("Could not create .tokenvault directory", err)
	}

	// now we have created the hidden folder, we need to define the full path to the DB file
	dbPath := filepath.Join(dbFolder, "token.db")
	fmt.Println("Database stored at:", dbPath)

	// now we need to connect to the database
	DB, err = sql.Open("sqlite", dbPath)
	if err != nil {
		log.Fatal("Could not connect to Database:", err)
	}

	// now we have conected to DB we need to create a table to store the tokens

	createTableQuery := `
	CREATE TABLE IF NOT EXISTS tokens (
		project_name TEXT PRIMARY_KEY,
		token_value TEXT,
		updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
	);`

	_, err = DB.Exec(createTableQuery)
	if err != nil {
		log.Fatal("Failed to create table:", err)
	}
}

// now we need to save the token comming from request (update or insert)
func SaveToken(project string, token string) error {

	//here we need to a query to insert ot replace the eisting token
	query := `INSERT OR REPLACE INTO tokens (project_name, token_value, updated_at) VALUES (?, ?, CURRENT_TIMESTAMP)`

	_, err := DB.Exec(query, project, token)
	return err
}

//now we need to fetch the token by the project name

func GetToken(project string) (string, error) {
	var token string
	query := `SELECT token_value from tokens WHERE project_name = ?`

	//using queryrow we c=only fetch the first row of the query
	err := DB.QueryRow(query, project).Scan(&token)

	if err != nil {
		return "", err
	}
	return token, nil
}
