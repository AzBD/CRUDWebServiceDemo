# Implementing CRUD service for managing user profiles

## _How to run the application_

> Requirements
> **Install Vistual Studio or Rider or similar IDE**

## Hosting the web serivce

- Open the solution
- Select **WebAPI** project
- Right click on it and run the project
- This will host the service locally

## Running the console application

- Open the solution
- Select **RunWebAPI** project
- Right click on it and run the project
- This will open up the console/terminal
- Follow the instructions to perform CRUD operations on the user records that are hosted locally and stored in SQLite

## Authentication

This project creates a JWT tokens on user record is successfully created with expires after a day. This JWT token is required to:

- Update a specific user record
- Delete a specific user record

This JWT token is **not** required to:

- Access a specific user record
- Get active user records
- Get all user records

## Limitations

- Delete functionality will "soft delete" the record but won't wipe out the record from SQLite. This is intentional. If the record needs to be wiped out permanently that can be done by uncommenting the code in **WebAPI.Controllers.UserController.cs ln:128**
- The token is not encrypted, it can be easily captured and decoded in https://jwt.io/
- This is just a simple application to host basic web service and test it. Not a production ready application.

## Demo

A video is added in the root level to show the demo of this app. In this video, the application is running on Visual Studio - Mac.
