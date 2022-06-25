# Metaculus Discord Bot
- unofficial Discord bot for Metaculus
- in development, not releaseded yet!


## Features
- [x] searching the API and displaying questions in embeds
- [ ] DM alerts for questions
- [ ] channel alerts for questions
- ???

## User documentation
You can add the bot to your server with this [invite link]("https://not released yet")

### Low level commands
When the bot is running it is listening for *commands* prefixed by `!mc`, though this is not the intended usage.

- `!mc help`: displays this help message

 
- `!mc s[earch] <query>`: returns 5 best matches for the query

The bot's message is decorated with reaction emojis 1-5 when clicked by users will post the question link, causing an embed with a chart to appear.

- `!mc alert <question_id>` sets a resolution and swing alert for the question sent to DMs
- `!mc unalert <question_id>` removes the alert for the question
 
- `!mc channelalert <question_id>` sets a resolution and swing alert for the question sent to the current channel

### higher level controls
 
The `/metaculus` command is equivalent to `search`.

[//]: # (Adding a :warning: ⚠️ react to a message containing a question link will set a user alert for it.)

[//]: # (Adding a :rotating_light: 🚨 react to a message containing a question link will set a channel alert for it.)

[//]: # (Removing the react removes the alert.)



## Developer documentation

### Running the bot
The bot is a singleton so it's not intended to be run outside of (the single) production, running more instances can interfere with the slash command.
It uses PostgreSQL as its database.

If for some reason you want to run the bot, do this:
1. Install **dotnet6.0** and **PostgreSQL**
2. Create a valid *appsettings.json* from the *appsettings_template.json*
3. Install dependencies with `dotnet restore` in the project directory
4. Install **Entity Framework** tools `dotnet tool install --global dotnet-ef`
5. Create database migration `dotnet ef migrations add <migration_name>`
6. Apply migration to database `dotnet ef database update <migration_name>`
7. Run the bot with `dotnet run`

The repo also contains a Dockerfile that should be able to run the bot in a container.

### General design
The underlying library is [Discord.NET](https://discordnet.dev/index.html). The bot is composed of several services, modules and a Data Access Layer. 
#### Data
The data access layer uses a DbContext to access the PostgreSQL database.
Entity Framework is used so that the database schema can be generated automatically. 
Each request for the database has its own **MetaculusContext** which is created by a **MetaculusContextFactory**.

#### Services
**AlertService** every 6 hours for status on all relevant questions and sends alerts to users/channels.